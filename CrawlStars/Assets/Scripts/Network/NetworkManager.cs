using System;
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

        public RestApiClient RestClient { get; private set; }
        public bool IsInitialized { get; private set; }
        public bool IsMatched { get; private set; }

        public event Action<SnapshotDto> SnapshotReceived;

        protected override void Awake() {
            base.Awake();
            Initialize();
        }

        private void Update() {
            socketClient?.DispatchMessageQueue();
        }

        private void OnApplicationQuit() {
            socketClient?.Abort();
            socketClient = null;
        }

        public void Initialize() {
            config = NetworkConfig.Load();
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

        public async UniTask<MatchResponse> MatchAsync(CancellationToken ct) {
            MatchResponse response = await RestClient.PostAsync<object, MatchResponse>("matchmaking/join", null);
            if (response == null) {
                Debug.LogError("NetworkManager.MatchAsync::response of matchmaking is null");
                return null;
            }
            Debug.Log($"Room Id: {response.Room.Id}, Status: {response.Room.Status}, MaxPlayers: {response.Room.MaxPlayers}");
            Debug.Log($"My Id: {response.Player.Id}, Slot: {response.Player.Slot}, Team: {response.Player.Team}");
            PlayerManager.Instance.MyId = response.Player.Id;
            ct.ThrowIfCancellationRequested();

            // 방 입장
            await ConnectSocketAsync(response.WebSocketPath, ct);
            ct.ThrowIfCancellationRequested();

            // 다른 유저 기다리기
            await UniTask.WaitUntil(() => IsMatched, cancellationToken: ct);

            // await SendSocketJsonAsync(new InputMessage {
            //     MoveDir = new NetworkVector2 { X = 1f, Y = 0f },
            //     AttackDir = new NetworkVector2 { X = 1f, Y = 0f },
            //     PressedAttack = false
            // });
            return response;
        }

        private void RegisterSocketLogEvents(WebSocketClient socketClient) {
            socketClient.Opened += () => Debug.Log("WebSocket OnOpen");
            socketClient.MessageReceived += message => Debug.Log($"WebSocket Message: {message}");
            socketClient.ErrorReceived += error => Debug.LogError($"WebSocket Error: {error}");
            socketClient.Closed += closeCode => Debug.Log($"WebSocket Closed: {closeCode}");

            socketClient.Closed += closeCode => IsMatched = false;
            socketClient.MessageReceived += HandleSocketMessage;
        }

        private void HandleSocketMessage(string message) {
            try {
                IsMatched = true;

                var envelope = JsonConvert.DeserializeObject<MessageEnvelope>(message);
                switch (envelope?.Type) {
                    case "snapshot":
                        var snapshotMessage = JsonConvert.DeserializeObject<SnapshotMessage>(message);
                        if (snapshotMessage?.Snapshot != null) {
                            SnapshotReceived?.Invoke(snapshotMessage.Snapshot);
                        }
                        break;
                    case "error":
                        var errorMessage = JsonConvert.DeserializeObject<ErrorMessage>(message);
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
