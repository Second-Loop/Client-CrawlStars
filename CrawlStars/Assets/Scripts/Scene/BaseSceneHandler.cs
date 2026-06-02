using Cysharp.Threading.Tasks;
using Managing;
using Popup;
using UnityEngine;
using UnityEngine.UI;

namespace Scene {
    public abstract class BaseSceneHandler : MonoBehaviour {
        [SerializeField] protected Button leaveButton;

        protected bool isClickedLeave;

        protected virtual void Start() {
            leaveButton.onClick.AddListener(OnClickLeaveButton);
        }

        protected virtual void Update() {
            if (!Input.GetKeyDown(KeyCode.Escape)) return;
            if (SceneController.Instance.IsChanging) return;

            if (PopupManager.Instance.HasOpenPopup) {
                PopupManager.Instance.CloseTop();
            } else {
                ClickLeaveInternal().Forget();
            }
        }
        
        private void OnClickLeaveButton() {
            if (isClickedLeave) return;

            isClickedLeave = true;
            ClickLeaveInternal().Forget();
        }

        protected abstract UniTask ClickLeaveInternal();
    }
}