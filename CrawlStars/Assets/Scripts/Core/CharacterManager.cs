using Core.Player;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Core {
    public class CharacterManager {
        private static CharacterManager instance;
        public static CharacterManager Instance => instance ??= new CharacterManager();

        public PlayerData.CharacterType CurCharacter { get; set; }

        private CharacterInfo characterInfo;

        public async UniTask InitializeAsync() {
            var handle = Addressables.LoadAssetAsync<CharacterInfo>("CharacterInfo");
            var res = await handle.ToUniTask();

            if (handle.Status == AsyncOperationStatus.Succeeded) {
                characterInfo = res;
            } else {
                Debug.LogError($"CharacterManager.Initialize::failed to load CharacterInfo/{handle.Status}/{handle.OperationException}");
            }
        }

        public async UniTask<CharacterInfo> GetCharacterInfoAsync() {
            if (characterInfo == null) {
                await InitializeAsync();
            }

            if (characterInfo == null) {
                Debug.LogError("CharacterManager.GetCharacterInfoAsync::failed to initialize");
                return null;
            }

            return characterInfo;
        }
    }
}
