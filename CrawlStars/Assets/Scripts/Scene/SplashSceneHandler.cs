using Core;
using Cysharp.Threading.Tasks;
using Managing;
using Network;
using UnityEngine;
using Utility;

namespace Scene {
    public class SplashSceneHandler : MonoBehaviour {
        private void Awake() {
            // Screen.SetResolution(1920, 1080, true);
            Screen.SetResolution(1280, 720, false);
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
            var configTask = ErrorHandler.DoWithReTryAsync(GameConfig.LoadAsync, "GameConfig.LoadAsync", 3);
            var modeTask = ErrorHandler.DoWithReTryAsync(ModeManager.Instance.InitializeAsync, "ModeManager.InitializeAsync", 3);
            var characterTask = ErrorHandler.DoWithReTryAsync(CharacterManager.Instance.InitializeAsync, "CharacterManager.InitializeAsync", 3);

            await UniTask.WhenAll(configTask, modeTask, characterTask);

            await SceneController.Instance.ChangeSceneAsync(SceneController.MainSceneName);
        }
    }
}