using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Network {
    public sealed class NetworkManager : SingletonMonoBehaviour<NetworkManager> {
        private const string restBaseUrl = "http://localhost:3000";
        private const string websocketUrl = "ws://localhost:3000/ws";

        private WebSocketClient socket;
        private string jwtAccessToken;

        public RestApiClient Rest { get; private set; }

        protected override void Awake() {
            base.Awake();
            Rest = new RestApiClient(restBaseUrl);
        }

        private void Update() {
            socket?.DispatchMessageQueue();
        }

        public void SetJwtToken(string accessToken) {
            jwtAccessToken = accessToken;
            Rest.SetJwtToken(accessToken);
        }

        public async UniTask ConnectSocketAsync() {
            if (socket != null) {
                await socket.DisconnectAsync();
            }

            socket = new WebSocketClient(websocketUrl);
            RegisterSocketLogHandlers(socket);
            socket.Connect(jwtAccessToken);
        }

        public async UniTask SendSocketJsonAsync<T>(T message) {
            if (socket == null) {
                Debug.LogError("NetworkManager.SendSocketJsonAsync::socket is not created.");
                return;
            }

            await socket.SendJsonAsync(message);
        }

#region Test
        private void RegisterSocketLogHandlers(WebSocketClient socketClient) {
            socketClient.Opened += () => Debug.Log("WebSocket OnOpen");
            socketClient.MessageReceived += message => Debug.Log($"WebSocket Message: {message}");
            socketClient.ErrorReceived += error => Debug.LogError($"WebSocket Error: {error}");
            socketClient.Closed += closeCode => Debug.Log($"WebSocket Closed: {closeCode}");
        }

        public async UniTask TestRestApiAsync() {
            HealthResponse health = await Rest.GetAsync<HealthResponse>("health");
            Debug.Log($"REST Health: ok={health.Ok}, message={health.Message}");

            LoginResponse login = await Rest.PostAsync<LoginRequest, LoginResponse>(
                "auth/login",
                new LoginRequest {
                    Email = "test@example.com",
                    Password = "password"
                }
            );

            SetJwtToken(login.AccessToken);
            Debug.Log($"REST Login: userId={login.UserId}, nickname={login.Nickname}");

            UserResponse me = await Rest.GetAsync<UserResponse>("users/me");
            Debug.Log($"REST UsersMe: id={me.Id}, email={me.Email}, nickname={me.Nickname}, level={me.Level}");
        }
#endregion
    }
}
