using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using NativeWebSocket;
using Newtonsoft.Json;
using UnityEngine;

namespace Network {
    public sealed class WebSocketClient {
        private readonly string url;
        private readonly Queue<string> pendingMessages = new();
        private WebSocket socket;
        private bool connectionStarted;

        public event Action Opened;
        public event Action<string> MessageReceived;
        public event Action<string> ErrorReceived;
        public event Action<WebSocketCloseCode> Closed;

        public bool IsConnected => socket?.State == WebSocketState.Open;

        public WebSocketClient(string url) {
            this.url = url;
        }

        public void Connect(string jwtToken = null) {
            if (connectionStarted) {
                throw new InvalidOperationException("WebSocketClient.ConnectAsync::connection already started.");
            }

            connectionStarted = true;

            var header = string.IsNullOrEmpty(jwtToken) 
                ? null 
                : new Dictionary<string, string> {{ "Authorization", $"Bearer {jwtToken}" }};
            socket = new WebSocket(url, header);
            socket.OnOpen += HandleOpen;
            socket.OnMessage += HandleMessage;
            socket.OnError += HandleError;
            socket.OnClose += HandleClose;

            ConnectInternal().Forget();
        }

        public async UniTask SendStringAsync(string message) {
            if (socket == null) return;

            // 연결 도중에 보내려고 하면 pending시킴
            if (socket.State == WebSocketState.Connecting) {
                pendingMessages.Enqueue(message);
                return;
            }

            if (!IsConnected) {
                Debug.LogError($"WebSocketClient.SendTextAsync::not connected. failed to send message: {message}");
                return;
            }
            await socket.SendText(message);
        }

        public async UniTask SendJsonAsync<T>(T message) {
            string json = JsonConvert.SerializeObject(message);
            await SendStringAsync(json);
        }

        public void Disconnect() {
            if (socket == null) return;

            pendingMessages.Clear();
            UnregisterEvents(socket);

            try {
                socket.CancelConnection();
            } catch (Exception e) {
                Debug.LogWarning($"WebSocket cancel ignored during disconnect: {e.Message}");
            } finally {
                socket = null;
            }
        }

        // NativeWebSocket 내부 큐에 저장된 메시지를 꺼내와 각 이벤트를 호출하는 과정
        public void DispatchMessageQueue() {
#if !UNITY_WEBGL || UNITY_EDITOR
            socket?.DispatchMessageQueue();
#endif
        }

        private async UniTaskVoid ConnectInternal() {
            if (socket == null) return;

            try {
                // 반환 자체는 소켓이 닫힐 때 하지만, 예외를 핸들링하기 위함
                await socket.Connect();
            } catch (Exception e) {
                if (socket != null) {
                    UnregisterEvents(socket);
                    socket = null;
                }
                ErrorReceived?.Invoke(e.Message);
            }
        }
        
        private async UniTaskVoid SendPendingMessages() {
            while (pendingMessages.Count > 0 && IsConnected) {
                string message = pendingMessages.Dequeue();
                await socket.SendText(message);
            }
        }
        
        private void UnregisterEvents(WebSocket targetSocket) {
            targetSocket.OnOpen -= HandleOpen;
            targetSocket.OnMessage -= HandleMessage;
            targetSocket.OnError -= HandleError;
            targetSocket.OnClose -= HandleClose;
        }

#region Events
        private void HandleOpen() {
            Opened?.Invoke();
            SendPendingMessages().Forget();
        }

        private void HandleMessage(byte[] bytes) {
            string message = Encoding.UTF8.GetString(bytes);
            MessageReceived?.Invoke(message);
        }

        private void HandleError(string error) {
            ErrorReceived?.Invoke(error);
        }

        private void HandleClose(WebSocketCloseCode closeCode) {
            // 서버로부터 먼저 끊기는 상황 대응
            if (socket != null) {
                UnregisterEvents(socket);
                socket = null;
            }

            Closed?.Invoke(closeCode);
            pendingMessages.Clear();
        }
#endregion
    }
}
