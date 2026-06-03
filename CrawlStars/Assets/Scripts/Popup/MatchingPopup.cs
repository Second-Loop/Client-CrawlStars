using System;
using System.Threading;
using Core.Player;
using Cysharp.Threading.Tasks;
using Managing;
using UnityEngine;

namespace Popup {
    public class MatchingPopup : PopupHandler {
        [SerializeField] ProgressBar progressBar;

        private CancellationTokenSource cts;

        public override void SetData(Param param, int sortingOrder) {
            base.SetData(param, sortingOrder);

            progressBar.Initialize();
            cts = new CancellationTokenSource();
            StartMatching(cts.Token).Forget();
        }

        private async UniTask StartMatching(CancellationToken ct) {
            float progress = 0f;

            var matchTask = GameManager.Instance.MatchAsync(ct);
            while (!matchTask.GetAwaiter().IsCompleted) {
                await UniTask.Delay(100);
                if (ct.IsCancellationRequested) {
                    GameManager.Instance.CancelMatch();
                    return;
                }

                progress += 0.1f;
                progressBar.SetValue(Mathf.Clamp(progress, 0f, 0.95f));
            }
            
            try {
                // 예외 잡기용
                await matchTask;
            } catch (Exception ex) {
                GameManager.Instance.CancelMatch();
                if (ex is not OperationCanceledException) {
                    RequestPopupClosing();
                    Debug.LogError(ex);
                }
                return;
            }

            progressBar.SetValue(1f);

            SceneController.Instance.ChangeSceneAsync(SceneController.PlaySceneName,
                GameManager.Instance.Initialize,
                PlayerManager.Instance.FocusCamera).Forget();

            RequestPopupClosing();
        }

        public override void Dispose(Result result = null) {
            base.Dispose(result);

            cts?.Cancel();
            cts?.Dispose();
            cts = null;
        }
    }
}