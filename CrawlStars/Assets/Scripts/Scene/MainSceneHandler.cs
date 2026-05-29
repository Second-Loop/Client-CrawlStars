using Core.Player;
using Cysharp.Threading.Tasks;
using Popup;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Managing {
    public class MainSceneHandler : MonoBehaviour {
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingButton;
        [SerializeField] private Button exitButton;

        private bool isClickedExit;

        private void Start() {
            playButton.onClick.AddListener(OnClickPlayButton);
            settingButton.onClick.AddListener(OnClickSettingButton);
            exitButton.onClick.AddListener(OnClickExitButton);
        }

        private void OnClickPlayButton() {
            PopupManager.Instance.ShowAsync(nameof(MatchingPopup)).Forget();
        }

        private void OnClickSettingButton() {
        }

        private void OnClickExitButton() {
            if (isClickedExit) return;

            isClickedExit = true;
            ClickExitInternal().Forget();
        }

        private async UniTask ClickExitInternal() {
            var param = new TwoButtonPopup.Param("Exit", "Are you sure you want to exit?");
            var result = await PopupManager.Instance.ShowAsync(nameof(TwoButtonPopup), param);
            if (((TwoButtonPopup.Result)result).isClickedOk) {
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
            isClickedExit = false;
        }
    }
}