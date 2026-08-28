using Cysharp.Threading.Tasks;
using Core.Player;
using Managing;
using Network;
using Popup;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scene {
    public class PlaySceneHandler : BaseSceneHandler {
        [SerializeField] private BenchMarker benchMarker;
        [SerializeField] private AimRenderer aimRenderer;
        [SerializeField] private CooldownView cooldownView;
        [SerializeField] private GameObject waitingCurtain;
        [SerializeField] private TextMeshProUGUI infoText;

        protected override void Start() {
            base.Start();
            GameManager.Instance.RegisterOnDetectInput(aimRenderer.OnPressKey);

            NetworkManager.Instance.InputSubmitted += benchMarker.OnInputSubmitted;
            NetworkManager.Instance.SnapshotReceived += benchMarker.OnReceiveSnapshot;
            NetworkManager.Instance.SnapshotReceived += HandleUIBeforeStart;

            cooldownView.Initialize(GameManager.Instance.AttackCooldownSource);
            aimRenderer.Initialize();

            waitingCurtain.SetActive(true);
            cooldownView.gameObject.SetActive(false);
        }

        protected override void Update() {
            if (Input.GetKeyDown(KeyCode.B)) {
                bool isActive = !benchMarker.gameObject.activeSelf;
                benchMarker.gameObject.SetActive(isActive);
                infoText.text = $"Press 'B' to {(isActive ? "hide" : "show")} benchmarker";
            }
            base.Update();
        }

        private void OnDestroy() {
            GameManager.Instance.UnregisterOnDetectInput(aimRenderer.OnPressKey);

            NetworkManager.Instance.InputSubmitted -= benchMarker.OnInputSubmitted;
            NetworkManager.Instance.SnapshotReceived -= benchMarker.OnReceiveSnapshot;
            NetworkManager.Instance.SnapshotReceived -= HandleUIBeforeStart;

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

        private void HandleUIBeforeStart(SnapshotDto snapshot) {
            if (snapshot.Status == "starting") {
                waitingCurtain.SetActive(false);
                return;
            }

            if (snapshot.Status != "started") return;

            cooldownView.gameObject.SetActive(true);
            NetworkManager.Instance.SnapshotReceived -= HandleUIBeforeStart;
        }
    }
}