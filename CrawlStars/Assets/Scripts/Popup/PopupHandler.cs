using Cysharp.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Popup {
    public abstract class PopupHandler : MonoBehaviour {
        public abstract class Param { }

        public abstract class Result { }

        [SerializeField] protected Canvas canvas;
        [SerializeField] protected TextMeshProUGUI titleText;
        [SerializeField] protected Button quitButton;
        [SerializeField] protected Button okButton;

        private UniTaskCompletionSource<Result> tcs;

        protected virtual void Awake() {
            tcs = new UniTaskCompletionSource<Result>();
        }

        protected virtual void Start() {
            if (quitButton != null) quitButton.onClick.AddListener(OnClickQuit);
            if (okButton != null) okButton.onClick.AddListener(OnClickOk);
        }

        protected virtual void OnDestroy() {
            if (quitButton != null) quitButton.onClick.RemoveListener(OnClickQuit);
            if (okButton != null) okButton.onClick.RemoveListener(OnClickOk);
        }

        public UniTask<Result> WaitForCloseAsync() {
            return tcs.Task;
        }

        // PopupManager 에서 호출될 메서드
        public virtual void Dispose(Result result = null) {
            tcs?.TrySetResult(result);
        }

        // PopupManager 에서 호출될 메서드
        public virtual void SetData(Param param, int sortingOrder) {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
        }

        protected virtual void OnClickQuit() {
            RequestClosing();
        }

        protected virtual void OnClickOk() {
            RequestClosing();
        }

        // 팝업에서 닫을 때, 팝업의 결괏값 전달 필요
        protected void RequestClosing(Result result = null) {
            PopupManager.Instance.Close(this, result);
        }
    }
}