using Cysharp.Threading.Tasks;
using Managing;
using UnityEngine;

namespace Scene {
    public class SplashSceneHandler : MonoBehaviour {
        private void Awake() {
            Screen.SetResolution(1920, 1080, true);
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
        }

        private void Start() {
            SceneController.Instance.ChangeSceneAsync(SceneController.MainSceneName).Forget();
        }
    }
}