using System;
using System.Threading;
using Core.Player;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Managing;
using Network;
using UnityEngine;
using UnityEngine.UI;

namespace Popup {
    public class MatchingPopup : PopupHandler {
        [SerializeField] Transform loadingIndicator;

        private CancellationTokenSource playerCts;
        private CancellationTokenSource timeoutCts;
        private const int TimeoutMilliseconds = 12000;

        public override void SetData(Param param, int sortingOrder) {
            base.SetData(param, sortingOrder);

            SetDataAsync().Forget();
        }

        private async UniTask SetDataAsync() {
            while (Application.internetReachability == NetworkReachability.NotReachable) {
                var param = new TwoButtonPopup.Param("Check out", "Please check your internet connection then click the OK button.");
                var res = (TwoButtonPopup.Result)await PopupManager.Instance.ShowAsync("TwoButtonPopup", param);
                if (!res.isClickedOk) {
                    RequestPopupClosing();
                    return;
                }
            }

            if (!NetworkManager.Instance.IsInitialized) {
                NetworkManager.Instance.Initialize();
            }

            playerCts = new CancellationTokenSource();
            timeoutCts = new CancellationTokenSource(TimeoutMilliseconds);
            StartMatching().Forget();
        }

        private async UniTask StartMatching() {
            loadingIndicator.DORotate(new Vector3(0, 0, -360f), 1f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);

            // cts가 null이 될 수 있으므로 값 복사
            var playerCt = playerCts.Token;
            var timeoutCt = timeoutCts.Token;

            ReadyEventMessageDto response = null;
            try {
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(playerCt, timeoutCt);
                response = await NetworkManager.Instance.MatchAsync(linkedCts.Token);
            } catch (Exception ex) {
                await NetworkManager.Instance.DisconnectSocketAsync();

                PlayerManager.Instance.MyId = null;
                PlayerManager.Instance.MyTeam = null;

                if (timeoutCt.IsCancellationRequested || ex is not OperationCanceledException) {
                    RequestPopupClosing();
                    Debug.LogError(ex);
                    var param = new OneButtonPopup.Param("Network Error", $"Please try again later.\n({ex.Message})");
                    PopupManager.Instance.ShowAsync("TwoButtonPopup", param).Forget();
                }
                return;
            }

            SceneController.Instance.ChangeSceneAsync(SceneController.PlaySceneName,
                () => GameManager.Instance.Initialize(response),
                GameManager.Instance.OnEnterPlayScene).Forget();

            RequestPopupClosing();
        }

        public override void Dispose(Result result = null) {
            base.Dispose(result);

            playerCts?.Cancel();
            playerCts?.Dispose();
            playerCts = null;

            // 유저가 취소한 상황이라 Cancel 하지 않음
            timeoutCts?.Dispose();
            timeoutCts = null;
            
            loadingIndicator.DOKill();
        }
    }
}