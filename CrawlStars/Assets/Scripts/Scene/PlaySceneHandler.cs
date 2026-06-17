using Cysharp.Threading.Tasks;
using Managing;
using Network;
using Popup;
using UnityEngine;
using UnityEngine.UI;

namespace Scene {
    public class PlaySceneHandler : BaseSceneHandler {
        [SerializeField] private BenchMarker benchMarker;

        protected override void Start() {
            base.Start();
            GameManager.Instance.RegisterInputAction(benchMarker.OnPressKey);
            NetworkManager.Instance.SnapshotReceived += benchMarker.OnReceiveSnapshot;
        }

        private void OnDestroy() {
            GameManager.Instance.UnregisterInputAction(benchMarker.OnPressKey);
            NetworkManager.Instance.SnapshotReceived -= benchMarker.OnReceiveSnapshot;
        }

        protected override async UniTask ClickLeaveInternal() {
            GameManager.Instance.SetActiveInput(false);

            var param = new TwoButtonPopup.Param("Leave", "Are you sure you want to leave this game?");
            var result = await PopupManager.Instance.ShowAsync(nameof(TwoButtonPopup), param);
            if (result is TwoButtonPopup.Result { isClickedOk: true }) {
                SceneController.Instance.ChangeSceneAsync(SceneController.MainSceneName, GameManager.Instance.Dispose).Forget();
                return;
            }

            GameManager.Instance.SetActiveInput(true);
            isClickedLeave = false;
        }
    }
}