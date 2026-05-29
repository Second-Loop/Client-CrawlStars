using System;
using CameraControl;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utility;

namespace Managing {
    public class SceneController : SingletonMonoBehaviour<SceneController> {
        [SerializeField] private GameObject loadingScreen;
        [SerializeField] private ProgressBar progressBar;

        public const string PlaySceneName = "Play";
        public const string MainSceneName = "Main";
        
        private bool isChangingScene;
        private const float FakeLoadingProgress = 0.9f;

        private void Start() {
            loadingScreen.gameObject.SetActive(false);
        }

        public async UniTask ChangeSceneAsync(string sceneName, Action beforeActivateAction = null, Action afterActivateAction = null) {
            if (isChangingScene) {
                Debug.LogWarning($"SceneController.ChangeScene::already changing scene to {sceneName}");
                return;
            }

            isChangingScene = true;
            Scene currentScene = SceneManager.GetActiveScene();

            // 다음 씬 Load
            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (op == null) {
                Debug.LogError($"SceneController.ChangeScene::failed {sceneName}");
                isChangingScene = false;
                return;
            }

            progressBar.Initialize();
            loadingScreen.gameObject.SetActive(true);

            op.allowSceneActivation = false;

            while (op.progress < FakeLoadingProgress) {
                float progress = Mathf.Clamp01(op.progress / 0.9f);
                progressBar.SetValue(progress);
                await UniTask.Yield();
            }

            beforeActivateAction?.Invoke();
            op.allowSceneActivation = true;

            // fake loading
            float fakeProgress = FakeLoadingProgress;
            while (fakeProgress < 1f) {
                await UniTask.Delay(50);
                fakeProgress += 0.01f;
                progressBar.SetValue(fakeProgress);
            }
            await UniTask.WaitUntil(() => op.isDone);

            // 다음 씬 Activate
            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            SceneManager.SetActiveScene(loadedScene);

            // 기존 씬 Unload
            op = SceneManager.UnloadSceneAsync(currentScene);
            if (op != null) {
                await UniTask.WaitUntil(() => op.isDone);
            } else {
                Debug.LogError($"SceneController.ChangeSceneAdditive::failed unload {currentScene.name}");
            }

            CommonCache.OnChangeScene();
            afterActivateAction?.Invoke();

            loadingScreen.SetActive(false);
            isChangingScene = false;
        }
    }
}