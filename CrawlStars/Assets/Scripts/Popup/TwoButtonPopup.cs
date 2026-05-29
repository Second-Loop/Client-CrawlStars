using UnityEngine;
using UnityEngine.UI;

namespace Popup {
    public class TwoButtonPopup : CommonPopup {
        public class Result : PopupHandler.Result {
            public bool isClickedOk;
            public Result(bool isClickedOk) {
                this.isClickedOk = isClickedOk;
            }
        }

        [SerializeField] protected Button noButton;

        protected override void Start() {
            base.Start();
            noButton.onClick.AddListener(OnClickNo);
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            noButton.onClick.RemoveListener(OnClickNo);
        }

        protected override void OnClickQuit() {
            RequestClosing(new Result(false));
        }

        protected override void OnClickOk() {
            RequestClosing(new Result(true));
        }

        protected void OnClickNo() {
            RequestClosing(new Result(false));
        }
    }
}