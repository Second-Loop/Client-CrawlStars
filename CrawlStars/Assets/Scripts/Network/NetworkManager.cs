using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Network {
    public class NetworkManager : SingletonMonoBehaviour<NetworkManager> {
        private NetworkConfig config;
        private WebSocketClient socketClient;
        private string jwtAccessToken;

        public RestApiClient RestClient { get; private set; }
        public bool IsInitialized { get; private set; }

        protected override void Awake() {
            base.Awake();
            Initialize();
        }

        private void Update() {
            socketClient?.DispatchMessageQueue();
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

        private void ConnectSocket(string path) {
            if (!IsInitialized || string.IsNullOrEmpty(path)) {
                Debug.LogError("NetworkManager.ConnectSocketAsync::not initialized or invalid parameter");
                return;
            }

            DisconnectSocket();

            socketClient = new WebSocketClient(config.GetWebSocketUrl(path));
            RegisterSocketLogEvents(socketClient);
            socketClient.Connect(jwtAccessToken);
        }

        public void DisconnectSocket() {
            if (socketClient == null) return;

            socketClient.Disconnect();
            socketClient = null;
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

            ConnectSocket(response.WebSocketPath);
            return response;
        }

#region Test
        private static void RegisterSocketLogEvents(WebSocketClient socketClient) {
            socketClient.Opened += () => Debug.Log("WebSocket OnOpen");
            socketClient.MessageReceived += message => Debug.Log($"WebSocket Message: {message}");
            socketClient.ErrorReceived += error => Debug.LogError($"WebSocket Error: {error}");
            socketClient.Closed += closeCode => Debug.Log($"WebSocket Closed: {closeCode}");
        }

        public async UniTask<NetworkTestSession> TestRestApiAsync(CancellationToken ct) {
            // 1. 서버 상태 받아오기
            // HealthResponse health = await Rest.GetAsync<HealthResponse>("health");
            // Debug.Log($"REST Health: status={health.Status}, service={health.Service}");

            // 2. 방 리스트 받아오기
            // RoomListResponse roomList = await Rest.GetAsync<RoomListResponse>("rooms");
            // Debug.Log($"REST Rooms: count={roomList?.Rooms?.Length ?? 0}");

            // 3. 방 만들기: 여기서 방 Id 받음
            RoomResponse createdRoom = await RestClient.PostAsync<object, RoomResponse>("rooms", null);
            if (string.IsNullOrEmpty(createdRoom?.Id)) {
                throw new InvalidOperationException("REST CreateRoom failed: room id is empty.");
            }
            Debug.Log($"REST CreateRoom: id={createdRoom.Id}, status={createdRoom.Status}");
            ct.ThrowIfCancellationRequested();

            // 4. 플레이어 방에 참가시키기: 여기서 플레이어 Id 받음
            PlayerResponse player = await RestClient.PostAsync<object, PlayerResponse>($"rooms/{createdRoom.Id}/players", null);
            Debug.Log($"REST CreateRoomPlayer: id={player.Id}, team={player.Team}, slot={player.Slot}");
            ct.ThrowIfCancellationRequested();

            // 5. 방 상태 받아오기
            // RoomResponse room = await Rest.GetAsync<RoomResponse>($"rooms/{createdRoom.Id}");
            // Debug.Log($"REST GetRoom: id={room.Id}, status={room.Status}, players={room.Players?.Length ?? 0}");

            // 6. 방 시작하기
            RoomResponse startedRoom = await RestClient.PostAsync<object, RoomResponse>($"rooms/{createdRoom.Id}/start", null);
            Debug.Log($"REST StartRoom: id={startedRoom.Id}, status={startedRoom.Status}, tick={startedRoom.LatestSnapshot?.Tick ?? 0}");
            ct.ThrowIfCancellationRequested();

            return new NetworkTestSession(createdRoom.Id, player.Id);
        }
#endregion
    }
}
