using System;
using System.Text;
using Cysharp.Threading.Tasks;
using NativeWebSocket;
using Newtonsoft.Json;

namespace Network {
    public sealed class WebSocketClient {
        private readonly string url;
        private WebSocket websocket;
        private UniTaskCompletionSource connectCompletionSource;

        public event Action Connected;
        public event Action<string> TextReceived;
        public event Action<string> ErrorReceived;
        public event Action<WebSocketCloseCode> Closed;

        public bool IsConnected =>
            websocket != null && websocket.State == WebSocketState.Open;

        public WebSocketClient(string url) {
            this.url = url;
        }

        public async UniTask ConnectAsync(string jwtToken = null) {
            if (websocket != null) {
                await DisconnectAsync();
            }

            if (!string.IsNullOrEmpty(jwtToken)) {
                websocket = new WebSocket(
                    url,
                    new System.Collections.Generic.Dictionary<string, string> {
                        { "Authorization", "Bearer " + jwtToken }
                    }
                );
            } else {
                websocket = new WebSocket(url);
            }

            websocket.OnOpen += HandleOpen;
            websocket.OnMessage += HandleMessage;
            websocket.OnError += HandleError;
            websocket.OnClose += HandleClose;

            connectCompletionSource = new UniTaskCompletionSource();
            _ = websocket.Connect();

            try {
                await connectCompletionSource.Task;
            } finally {
                connectCompletionSource = null;
            }
        }

        public async UniTask SendTextAsync(string message) {
            if (!IsConnected) {
                throw new InvalidOperationException("WebSocket is not connected.");
            }

            await websocket.SendText(message);
        }

        public async UniTask SendJsonAsync<T>(T message) {
            string json = JsonConvert.SerializeObject(message);
            await SendTextAsync(json);
        }

        public async UniTask DisconnectAsync() {
            if (websocket == null) {
                return;
            }

            websocket.OnOpen -= HandleOpen;
            websocket.OnMessage -= HandleMessage;
            websocket.OnError -= HandleError;
            websocket.OnClose -= HandleClose;

            if (websocket.State == WebSocketState.Open ||
                websocket.State == WebSocketState.Connecting) {
                await websocket.Close();
            }

            websocket = null;
        }

        public void DispatchMessageQueue() {
#if !UNITY_WEBGL || UNITY_EDITOR
            websocket?.DispatchMessageQueue();
#endif
        }

        private void HandleOpen() {
            connectCompletionSource?.TrySetResult();
            Connected?.Invoke();
        }

        private void HandleMessage(byte[] bytes) {
            string message = Encoding.UTF8.GetString(bytes);
            TextReceived?.Invoke(message);
        }

        private void HandleError(string error) {
            connectCompletionSource?.TrySetException(new InvalidOperationException(error));
            ErrorReceived?.Invoke(error);
        }

        private void HandleClose(WebSocketCloseCode closeCode) {
            connectCompletionSource?.TrySetException(
                new InvalidOperationException($"WebSocket closed before connection completed: {closeCode}")
            );
            Closed?.Invoke(closeCode);
        }
    }
}
