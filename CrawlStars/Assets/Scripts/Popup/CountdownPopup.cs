using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Popup {
    public class CountdownPopup : PopupHandler {
        public class Param : PopupHandler.Param {
            public int? countdownSeconds;
            public Param(int? countdownSeconds) {
                this.countdownSeconds = countdownSeconds;
            }
        }

        [SerializeField] private GameObject countdownGroup;
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private TextMeshProUGUI startText;

        public override bool CanCloseWithEsc => false;
        
        private const int FallbackSeconds = 5;
        private const int DelayMilliSeconds = 1000;

        public override void SetData(PopupHandler.Param param, int sortingOrder) {
            base.SetData(param, sortingOrder);

            var validParam = param as Param;
            if (validParam == null) {
                Debug.LogError("CountdownPopup.SetData::invalid param");
                RequestPopupClosing();
                return;
            }

            countdownGroup.gameObject.SetActive(false);
            startText.transform.localScale = Vector3.zero;

            int countdownSeconds = validParam.countdownSeconds ?? FallbackSeconds;
            StartCountdownAsync(countdownSeconds - 1).Forget();
        }

        private async UniTaskVoid StartCountdownAsync(int countdownSeconds) {
            countdownText.text = $"{countdownSeconds} seconds..";
            countdownGroup.gameObject.SetActive(true);

            for (int i = countdownSeconds; i > 0; --i) {
                countdownText.text = $"{i} seconds..";
                await UniTask.Delay(DelayMilliSeconds);
            }

            countdownGroup.gameObject.SetActive(false);
            startText.transform.DOScale(1f, 0.5f);
            await UniTask.Delay(DelayMilliSeconds);
            RequestPopupClosing();
        }
        
    }
}