using Cysharp.Threading.Tasks;
using Popup;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Scene {
    public class MainSceneHandler : BaseSceneHandler {
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingButton;

        protected override void Start() {
            base.Start();
            playButton.onClick.AddListener(OnClickPlayButton);
            settingButton.onClick.AddListener(OnClickSettingButton);
        }

        private void OnClickPlayButton() {
            PopupManager.Instance.ShowAsync(nameof(MatchingPopup)).Forget();
        }

        private void OnClickSettingButton() {
        }

        protected override async UniTask ClickLeaveInternal() {
            var param = new TwoButtonPopup.Param("Exit", "Are you sure you want to exit?");
            var result = await PopupManager.Instance.ShowAsync(nameof(TwoButtonPopup), param);
            if (result is TwoButtonPopup.Result { isClickedOk: true }) {
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
            isClickedLeave = false;
        }
    }
}