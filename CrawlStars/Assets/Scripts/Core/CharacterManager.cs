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

        public CharacterType MyCharacterType { get; private set; }
        public CharacterInfo.Definition MyCharacter { get; private set; }

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

        /// <summary>
        /// 비동기 회피로 TryReInitialize 하지 않았기에 캐릭터 선택 팝업에서만 호출
        /// 팝업에서 불렀다는 것은 데이터 초기화가 유효하다는 뜻이기 때문
        /// </summary>
        public void SetMyCharacter(CharacterType type) {
            MyCharacterType = type;
            if (characterInfo?.Data == null || !characterInfo.Data.TryGetValue(type, out var definition)) {
                Debug.LogError($"CharacterManager.SetMyCharacter::there is no data for {type}");
                return;
            }
            MyCharacter = definition;
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
