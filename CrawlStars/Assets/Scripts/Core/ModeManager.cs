using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Core {
    public class ModeManager {
        private static ModeManager instance;
        public static ModeManager Instance => instance ??= new ModeManager();
        
        public enum GameMode { Solo, Team }

        public GameMode CurGameMode { get; set; }

        private ModeInfo modeInfo;

        public async UniTask InitializeAsync() {
            var handle = Addressables.LoadAssetAsync<ModeInfo>("ModeInfo");
            var res = await handle.ToUniTask();

            if (handle.Status == AsyncOperationStatus.Succeeded) {
                modeInfo = res;
            } else {
                Debug.LogError($"ModeManager.Initialize::failed to load ModeInfo/{handle.Status}/{handle.OperationException}");
            }
        }

        public async UniTask<ModeInfo> GetModeInfoAsync() {
            if (modeInfo == null) {
                await InitializeAsync();
            }

            if (modeInfo == null) {
                Debug.LogError("ModeManager.GetModeInfoAsync::failed to initialize");
                return null;
            }
            return modeInfo;
        }
    }
}