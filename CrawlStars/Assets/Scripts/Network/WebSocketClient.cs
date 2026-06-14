using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NativeWebSocket;
using Newtonsoft.Json;
using UnityEngine;

namespace Network {
    public sealed class WebSocketClient {
        private static readonly TimeSpan CloseTimeout = TimeSpan.FromSeconds(3);

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

        public async UniTask DisconnectAsync() {
            WebSocket targetSocket = socket;
            if (targetSocket == null) return;

            pendingMessages.Clear();
            UnregisterEvents(targetSocket);
            socket = null;

            WebSocketCloseCode closeCode = WebSocketCloseCode.Abnormal;
            try {
                if (targetSocket.State == WebSocketState.Open) {
                    var closeTask = targetSocket.Close();

                    // 서버가 응답하지 않을 시 타임아웃 처리
                    var completedTask = await Task.WhenAny(closeTask, Task.Delay(CloseTimeout));

                    // closeTask에서 발생한 예외 잡는 용도
                    if (completedTask == closeTask) {
                        await closeTask;
                        closeCode = WebSocketCloseCode.Normal;
                    } else {
                        Debug.LogWarning($"WebSocket close timed out after {CloseTimeout.TotalSeconds} seconds.");
                        closeTask.AsUniTask().Forget(e => LogUnexpectedCloseError(e, "after timeout"));
                    }
                }
            } catch (System.Net.WebSockets.WebSocketException e)
                when (e.WebSocketErrorCode == System.Net.WebSockets.WebSocketError.ConnectionClosedPrematurely) {
                // 서버가 close frame 응답 없이 연결을 먼저 종료한 경우, finally에서 로컬 소켓을 정리
            } catch (Exception e) {
                Debug.LogError($"WebSocketClient.DisconnectAsync::close failed/{e.Message}");
            } finally {
                // 연결 중인 소켓과 close handshake에 응답하지 않는 소켓을 정리
                targetSocket.CancelConnection();
                Closed?.Invoke(closeCode);
            }
        }

        public void Abort() {
            WebSocket targetSocket = socket;
            if (targetSocket == null) return;

            pendingMessages.Clear();
            UnregisterEvents(targetSocket);
            socket = null;

            targetSocket.CancelConnection();
            Closed?.Invoke(WebSocketCloseCode.Abnormal);
        }

        private static void LogUnexpectedCloseError(Exception exception, string context) {
            if (exception is System.Net.WebSockets.WebSocketException webSocketException &&
                webSocketException.WebSocketErrorCode ==
                System.Net.WebSockets.WebSocketError.ConnectionClosedPrematurely) {
                return;
            }
            Debug.LogWarning($"WebSocket close completed with an error {context}: {exception.Message}");
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
