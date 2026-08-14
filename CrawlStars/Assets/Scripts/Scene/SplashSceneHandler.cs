using Core;
using Core.Character;
using Core.Mode;
using Cysharp.Threading.Tasks;
using Managing;
using Network;
using UnityEngine;
using Utility;

namespace Scene {
    public class SplashSceneHandler : MonoBehaviour {
        private void Awake() {
            Screen.SetResolution(1920, 1080, false);
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            Application.runInBackground = true;

            UniTaskScheduler.UnobservedTaskException += exception => {
                Debug.LogError(exception.ToString());
            };
        }

        private void Start() {
            InitializeAsync().Forget();
        }

        private async UniTask InitializeAsync() {
            await ErrorHandler.DoWithReTryAsync(GameConfig.LoadAsync, "GameConfig.LoadAsync", 3);

            // GameConfig.LoadAsync 이후 실행해야 함
            var modeTask = ErrorHandler.DoWithReTryAsync(ModeManager.Instance.InitializeAsync, "ModeManager.InitializeAsync", 3);
            var characterTask = ErrorHandler.DoWithReTryAsync(CharacterManager.Instance.InitializeAsync, "CharacterManager.InitializeAsync", 3);

            await UniTask.WhenAll(modeTask, characterTask);

            await SceneController.Instance.ChangeSceneAsync(SceneController.MainSceneName);
        }
    }
}