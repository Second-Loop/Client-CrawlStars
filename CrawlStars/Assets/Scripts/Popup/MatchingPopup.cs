using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Managing;
using Network;
using UnityEngine;
using UnityEngine.UI;

namespace Popup {
    public class MatchingPopup : PopupHandler {
        [SerializeField] Transform loadingIndicator;

        private CancellationTokenSource cts;

        public override void SetData(Param param, int sortingOrder) {
            base.SetData(param, sortingOrder);

            cts = new CancellationTokenSource();
            StartMatching(cts.Token).Forget();
        }

        private async UniTask StartMatching(CancellationToken ct) {
            loadingIndicator.DORotate(new Vector3(0, 0, -360f), 1f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);

            ReadyEventMessageDto response = null;
            try {
                response = await NetworkManager.Instance.MatchAsync(ct);
            } catch (Exception ex) {
                await NetworkManager.Instance.DisconnectSocketAsync();
                if (ex is not OperationCanceledException) {
                    RequestPopupClosing();
                    Debug.LogError(ex);
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

            cts?.Cancel();
            cts?.Dispose();
            cts = null;
            
            loadingIndicator.DOKill();
        }
    }
}