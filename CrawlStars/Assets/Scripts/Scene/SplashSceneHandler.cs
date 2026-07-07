using Core;
using Cysharp.Threading.Tasks;
using Managing;
using Network;
using UnityEngine;

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
            var configTask = GameConfig.LoadAsync();
            var modeTask = ModeManager.Instance.InitializeAsync();
            var characterTask = CharacterManager.Instance.InitializeAsync();

            await UniTask.WhenAll(configTask, modeTask, characterTask);

            await SceneController.Instance.ChangeSceneAsync(SceneController.MainSceneName);
        }
    }
}