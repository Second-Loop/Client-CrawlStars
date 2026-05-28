using Core.Player;
using Cysharp.Threading.Tasks;
using Popup;
using UnityEngine;
using UnityEngine.UI;

namespace Managing {
    public class PlaySceneHandler : MonoBehaviour {
        [SerializeField] private Button leaveButton;

        private bool isClickedLeave;

        private void Start() {
            leaveButton.onClick.AddListener(OnClickLeaveButton);
        }

        private void OnClickLeaveButton() {
            if (isClickedLeave) return;

            isClickedLeave = true;
            ClickLeaveInternal().Forget();
        }

        private async UniTask ClickLeaveInternal() {
            var param = new TwoButtonPopup.Param("Leave", "Are you sure you want to leave this game?");
            var result = await PopupManager.Instance.ShowAsync(nameof(TwoButtonPopup), param);
            if (((TwoButtonPopup.Result)result).isClickedOk) {
                SceneController.Instance.ChangeSceneAsync(SceneController.MainSceneName, GameManager.Instance.Dispose).Forget();
            }
            isClickedLeave = false;
        }
    }
}