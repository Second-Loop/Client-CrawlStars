using Core;
using Cysharp.Threading.Tasks;
using Popup;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Scene {
    public class MainSceneHandler : BaseSceneHandler {
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingButton;
        [SerializeField] private Button selectCharacterButton;
        [SerializeField] private Button selectModeButton;

        [SerializeField] private TextMeshProUGUI selectedCharacterText;
        [SerializeField] private TextMeshProUGUI selectedModeText;
        
        [SerializeField] private Toggle botModeToggle;

        protected override void Start() {
            base.Start();
            playButton.onClick.AddListener(OnClickPlayButton);
            settingButton.onClick.AddListener(OnClickSettingButton);
            selectCharacterButton.onClick.AddListener(OnClickSelectCharacterButton);
            selectModeButton.onClick.AddListener(OnClickSelectModeButton);

            botModeToggle.onValueChanged.AddListener(OnValueChangedBotMode);

            selectedModeText.text = ModeManager.Instance.CurGameMode.ToString();
            selectedCharacterText.text = CharacterManager.Instance.MyCharacterType.ToString();
        }

        private void OnClickPlayButton() {
            PopupManager.Instance.ShowAsync(nameof(MatchingPopup)).Forget();
        }

        private void OnClickSettingButton() { }
        
        private void OnClickSelectCharacterButton() {
            OnClickSelectCharacterAsync().Forget();
        }
        
        private void OnClickSelectModeButton() {
            OnClickSelectModeAsync().Forget();
        }

        private void OnValueChangedBotMode(bool isOn) {
            GameManager.Instance.IsBotModeActivated = isOn;
        }

        private async UniTask OnClickSelectModeAsync() {
            await PopupManager.Instance.ShowAsync(nameof(ModePopup));
            selectedModeText.text = ModeManager.Instance.CurGameMode.ToString();
        }

        private async UniTask OnClickSelectCharacterAsync() {
            await PopupManager.Instance.ShowAsync(nameof(CharacterPopup));
            selectedCharacterText.text = CharacterManager.Instance.MyCharacterType.ToString();
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
