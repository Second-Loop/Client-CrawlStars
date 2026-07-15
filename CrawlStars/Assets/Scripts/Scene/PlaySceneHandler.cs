using Cysharp.Threading.Tasks;
using Core.Player;
using Managing;
using Network;
using Popup;
using UnityEngine;
using UnityEngine.UI;

namespace Scene {
    public class PlaySceneHandler : BaseSceneHandler {
        [SerializeField] private BenchMarker benchMarker;
        [SerializeField] private AimRenderer aimRenderer;
        [SerializeField] private CooldownView cooldownView;
        [SerializeField] private GameObject waitingCurtain;

        protected override void Start() {
            base.Start();
            GameManager.Instance.RegisterOnSendInput(benchMarker.OnPressKey);
            GameManager.Instance.RegisterOnDetectInput(aimRenderer.OnPressKey);

            NetworkManager.Instance.SnapshotReceived += benchMarker.OnReceiveSnapshot;
            NetworkManager.Instance.SnapshotReceived += HideWaitingCurtain;

            cooldownView.Initialize(GameManager.Instance.AttackCooldownSource);

            waitingCurtain.SetActive(true);
            cooldownView.gameObject.SetActive(false);
        }

        private void OnDestroy() {
            GameManager.Instance.UnregisterOnSendInput(benchMarker.OnPressKey);
            GameManager.Instance.UnregisterOnDetectInput(aimRenderer.OnPressKey);

            NetworkManager.Instance.SnapshotReceived -= benchMarker.OnReceiveSnapshot;
            NetworkManager.Instance.SnapshotReceived -= HideWaitingCurtain;

            cooldownView.Clear();
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

        private void HideWaitingCurtain(SnapshotDto snapshot) {
            if (snapshot.Status != "starting") return;

            waitingCurtain.SetActive(false);
            cooldownView.gameObject.SetActive(true);
            NetworkManager.Instance.SnapshotReceived -= HideWaitingCurtain;
        }
    }
}