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

            while (progress < 1f) {
                await UniTask.Delay(100);
                if (ct.IsCancellationRequested) return;

                progress += 0.1f;
                progressBar.SetValue(progress);
            }

            SceneController.Instance.ChangeSceneAsync(SceneController.PlaySceneName,
                GameManager.Instance.Initialize,
                PlayerManager.Instance.FocusCamera).Forget();

            RequestClosing();
        }

        public override void Dispose(Result result = null) {
            base.Dispose(result);

            cts?.Cancel();
            cts?.Dispose();
            cts = null;
        }
    }
}