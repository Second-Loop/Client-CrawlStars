using Core;
using Cysharp.Threading.Tasks;
using Managing;
using Network;
using UnityEngine;

namespace Scene {
    public class SplashSceneHandler : MonoBehaviour {
        private void Awake() {
            Screen.SetResolution(1920, 1080, true);
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
        }

        private void Start() {
            InitializeAsync().Forget();
        }

        private async UniTask InitializeAsync() {
            await GameConfig.LoadAsync();

            await SceneController.Instance.ChangeSceneAsync(SceneController.MainSceneName);
        }
    }
}