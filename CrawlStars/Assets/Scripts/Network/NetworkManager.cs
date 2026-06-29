using System;
using System.Net;
using System.Threading;
using Core.Player;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Network {
    public class NetworkManager : SingletonMonoBehaviour<NetworkManager> {
        private NetworkConfig config;
        private WebSocketClient socketClient;
        private string jwtAccessToken;
        private UniTask initializationTask;
        private ReadyEventMessageDto matchedReadyEvent;

        public RestApiClient RestClient { get; private set; }
        public bool IsInitialized { get; private set; }
        public bool IsMatched { get; private set; }

        public event Action<SnapshotDto> SnapshotReceived;
        public event Action<GameEndMessageDto> GameEndReceived;

        protected override void Awake() {
            base.Awake();
            initializationTask = InitializeAsync().Preserve();
            initializationTask.Forget();
        }

        private void Update() {
            socketClient?.DispatchMessageQueue();
        }

        private void OnApplicationQuit() {
            socketClient?.Abort();
            socketClient = null;
        }

        private async UniTask InitializeAsync() {
            config = await NetworkConfig.LoadAsync();
            if (config != null) {
                RestClient = new RestApiClient(config.RestBaseUrl);
            }
            IsInitialized = RestClient != null;
        }

        public void SetJwtToken(string accessToken) {
            if (!IsInitialized || string.IsNullOrEmpty(accessToken)) {
                Debug.LogError("NetworkManager.SetJwtToken::not initialized or invalid parameter");
                return;
            }

            jwtAccessToken = accessToken;
            RestClient.SetJwtToken(accessToken);
        }

        private async UniTask ConnectSocketAsync(string path, CancellationToken ct) {
            if (!IsInitialized || string.IsNullOrEmpty(path)) {
                Debug.LogError("NetworkManager.ConnectSocketAsync::not initialized or invalid parameter");
                return;
            }

            await DisconnectSocketAsync();
            ct.ThrowIfCancellationRequested();

            socketClient = new WebSocketClient(config.GetWebSocketUrl(path));
            RegisterSocketLogEvents(socketClient);
            socketClient.Connect(jwtAccessToken);
        }

        public UniTask DisconnectSocketAsync() {
            if (socketClient == null) return UniTask.CompletedTask;

            // 복사해서 사용하기 때문에 연달아서 Connect 해도 충돌 없음
            var targetClient = socketClient;
            socketClient = null;
            return targetClient.DisconnectAsync();
        }

        public async UniTask SendSocketJsonAsync<T>(T message) {
            if (!IsInitialized || message == null) {
                Debug.LogError("NetworkManager.SendSocketJsonAsync::not initialized or invalid parameter");
                return;
            }

            if (socketClient == null) {
                Debug.LogError("NetworkManager.SendSocketJsonAsync::socket is not created.");
                return;
            }

            await socketClient.SendJsonAsync(message);
        }

        public UniTask SendReadyAckAsync() => SendSocketJsonAsync(new ReadyAckMessageDto());

        public async UniTask<ReadyEventMessageDto> MatchAsync(CancellationToken ct) {
            await initializationTask;
            ct.ThrowIfCancellationRequested();
            if (!IsInitialized) {
                throw new InvalidOperationException("NetworkManager initialization failed.");
            }

            IsMatched = false;
            matchedReadyEvent = null;
            MatchDto dto = await RestClient.PostAsync<object, MatchDto>("matchmaking/join", null);
            if (dto == null) {
                Debug.LogError("NetworkManager.MatchAsync::response of matchmaking is null");
                throw new WebException("matchmaking response is null");
            }
            Debug.Log($"Room Id: {dto.Room.Id}, MaxPlayers: {dto.Room.MaxPlayers}\n" +
                      $"My Id: {dto.Player.Id}, Slot: {dto.Player.Slot}, Team: {dto.Player.Team}");
            PlayerManager.Instance.MyId = dto.Player.Id;
            ct.ThrowIfCancellationRequested();

            // 방 입장
            await ConnectSocketAsync(dto.WebSocketPath, ct);
            ct.ThrowIfCancellationRequested();

            // 다른 유저 기다리기
            await UniTask.WaitUntil(() => IsMatched, cancellationToken: ct);
            return matchedReadyEvent;
        }

        private void RegisterSocketLogEvents(WebSocketClient socketClient) {
            socketClient.Opened += () => Debug.Log("WebSocket OnOpen");
            socketClient.MessageReceived += HandleSocketMessage;
            socketClient.ErrorReceived += error => Debug.LogError($"WebSocket Error: {error}");
            socketClient.Closed += closeCode => {
                IsMatched = false;
                matchedReadyEvent = null;
                Debug.Log($"WebSocket Closed: {closeCode}");
            };
        }

        private void HandleSocketMessage(string message) {
            try {
                var envelope = JsonConvert.DeserializeObject<MessageEnvelope>(message);
                switch (envelope?.Type) {
                    case "Ready":   // 모든 유저 매칭됨
                        var readyEvent = JsonConvert.DeserializeObject<ReadyEventMessageDto>(message);
                        if (readyEvent?.Map == null || readyEvent.Players == null) {
                            Debug.LogWarning("NetworkManager.HandleSocketMessage::ready event data is null");
                            return;
                        }
                        matchedReadyEvent = readyEvent;
                        IsMatched = true;
                        break;
                    case "snapshot":
                        var snapshotMessage = JsonConvert.DeserializeObject<SnapshotMessageDto>(message);
                        if (snapshotMessage?.Snapshot == null) {
                            Debug.LogWarning($"NetworkManager.HandleSocketMessage::message snapshot is null");
                            return;
                        }
                        SnapshotReceived?.Invoke(snapshotMessage.Snapshot);
                        break;
                    case "GameEnd":
                        var gameEndMessage = JsonConvert.DeserializeObject<GameEndMessageDto>(message);
                        if (gameEndMessage == null ||
                            string.IsNullOrEmpty(gameEndMessage.PlayerId) ||
                            string.IsNullOrEmpty(gameEndMessage.Result)) {
                            Debug.LogWarning("NetworkManager.HandleSocketMessage::game end data is invalid");
                            return;
                        }
                        GameEndReceived?.Invoke(gameEndMessage);
                        break;
                    case "error":
                        var errorMessage = JsonConvert.DeserializeObject<ErrorMessageDto>(message);
                        Debug.LogError($"WebSocket API Error: {errorMessage?.Error?.Code}/{errorMessage?.Error?.Message}");
                        break;
                }
            } catch (JsonException e) {
                Debug.LogError($"NetworkManager.HandleSocketMessage::invalid message/{e.Message}");
            }
        }

        private sealed class MessageEnvelope {
            [JsonProperty("Type")] public string Type { get; set; }
        }
    }
}
