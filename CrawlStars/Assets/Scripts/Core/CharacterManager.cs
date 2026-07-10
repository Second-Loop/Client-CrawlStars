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

        private const string SelectedCharacterTypeKey = "SCTK";

        public async UniTask<bool> InitializeAsync() {
            var handle = Addressables.LoadAssetAsync<CharacterInfoSo>("CharacterInfo");
            var res = await handle.ToUniTask();

            CharacterInfoSo clientCharacterInfoSo = null;
            if (handle.Status == AsyncOperationStatus.Succeeded) {
                clientCharacterInfoSo = res;
            } else {
                Debug.LogError($"CharacterManager.Initialize::failed to load CharacterInfo/{handle.Status}/{handle.OperationException}");
                return false;
            }

            characterInfo = new CharacterInfo(clientCharacterInfoSo);

            // 이전에 저장했던 캐릭터 불러오기
            int selectedType = PlayerPrefs.GetInt(SelectedCharacterTypeKey);
            SetMyCharacter((CharacterType)selectedType);
            return true;
        }

        public void SetMyCharacter(CharacterType type) {
            MyCharacterType = type;
            MyCharacter = GetCharacterInfo(type);

            if (MyCharacter == null) {
                Debug.LogError($"CharacterManager.SetMyCharacter::there is no data for {type}, so fallback to {CharacterType.A} type");
                type = CharacterType.A;
                MyCharacterType = type;
                MyCharacter = GetCharacterInfo(type);
            }

            PlayerPrefs.SetInt(SelectedCharacterTypeKey, (int)type);
        }

        public IReadOnlyDictionary<CharacterType, CharacterInfo.Definition> GetCharacterInfo() {
            return characterInfo?.Data;
        }

        public CharacterInfo.Definition GetCharacterInfo(CharacterType type) {
            if (characterInfo?.Data == null || !characterInfo.Data.TryGetValue(type, out var definition)) {
                Debug.LogError($"CharacterManager.GetCharacterInfoAsync::there is no data for {type}");
                return null;
            }
            return definition;
        }
    }
}
