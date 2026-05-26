using UnityEngine;

namespace Network {
    public sealed class NetworkBehaviour : MonoBehaviour {
        private static NetworkBehaviour instance;
        public static NetworkBehaviour Instance => instance;

        public NetworkService Service { get; private set; }

        private void Awake() {
            if (instance != null) {
                Debug.LogError($"{nameof(NetworkBehaviour)} Instance가 {name}에서 중복 초기화 시도되었습니다.");
                return;
            }

            instance = this;
            Service = new NetworkService(
                restBaseUrl: "http://localhost:3000",
                websocketUrl: "ws://localhost:3000/ws"
            );
            DontDestroyOnLoad(gameObject);
        }

        private void Update() {
            Service.Socket.DispatchMessageQueue();
        }
    }
}