using Cysharp.Threading.Tasks;

namespace Network {
    public sealed class NetworkService {
        public RestApiClient Rest { get; }
        public WebSocketClient Socket { get; }

        public string JwtAccessToken { get; private set; }

        public NetworkService(string restBaseUrl, string websocketUrl) {
            Rest = new RestApiClient(restBaseUrl);
            Socket = new WebSocketClient(websocketUrl);
        }

        public void SetJwtToken(string accessToken) {
            JwtAccessToken = accessToken;
            Rest.SetJwtToken(accessToken);
        }

        public async UniTask ConnectSocketAsync() {
            await Socket.ConnectAsync(JwtAccessToken);
        }

        public async UniTask DisconnectSocketAsync() {
            await Socket.DisconnectAsync();
        }

        public void DispatchSocketMessageQueue() {
            Socket.DispatchMessageQueue();
        }
    }
}
