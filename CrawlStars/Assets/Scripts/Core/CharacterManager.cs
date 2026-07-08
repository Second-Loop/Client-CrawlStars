using System.Collections.Generic;
using Core.Player;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Core {
    public class CharacterManager {
        public enum CharacterType {
            A, B, C
        }

        private static CharacterManager instance;
        public static CharacterManager Instance => instance ??= new CharacterManager();

        private CharacterInfo characterInfo;

        public CharacterType CurCharacter { get; set; }

        public async UniTask InitializeAsync() {
            var handle = Addressables.LoadAssetAsync<CharacterInfoSo>("CharacterInfo");
            var res = await handle.ToUniTask();

            CharacterInfoSo clientCharacterInfoSo = null;
            if (handle.Status == AsyncOperationStatus.Succeeded) {
                clientCharacterInfoSo = res;
            } else {
                Debug.LogError($"CharacterManager.Initialize::failed to load CharacterInfo/{handle.Status}/{handle.OperationException}");
                return;
            }

            characterInfo = new CharacterInfo(clientCharacterInfoSo);
        }

        public async UniTask<IReadOnlyDictionary<CharacterType, CharacterInfo.Definition>> GetCharacterInfoAsync() {
            bool isDataValid = await TryReInitialize();
            if (!isDataValid) return null;

            return characterInfo.Data;
        }

        public async UniTask<CharacterInfo.Definition> GetCharacterInfoAsync(CharacterType type) {
            bool isDataValid = await TryReInitialize();
            if (!isDataValid) return null;

            if (!characterInfo.Data.TryGetValue(type, out var definition)) {
                Debug.LogError($"CharacterManager.GetCharacterInfoAsync::there is no data for {type}");
                return null;
            }
            return definition;
        }

        private async UniTask<bool> TryReInitialize() {
            if (characterInfo == null) {
                await InitializeAsync();
                if (characterInfo == null) {
                    Debug.LogError("CharacterManager.GetCharacterInfoAsync::failed to initialize");
                    return false;
                }
            }
            return true;
        }
    }
}
