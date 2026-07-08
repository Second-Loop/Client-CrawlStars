using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core {
    public class CharacterInfo {
        public class Definition {
            public string description;
            public string iconSpriteName;
            public float normalAttackDistance;
            public float skillAttackDistance;
            public int skillAttackCoolDown;
            public int maxBullets;
        }
        
        public IReadOnlyDictionary<CharacterManager.CharacterType, Definition> Data { get; }

        public CharacterInfo(CharacterInfoSo infoSo) {
            if (infoSo == null) {
                Debug.LogError("CharacterInfo.ctor::invalid parameter");
                throw new ArgumentNullException();
            }

            var data = new Dictionary<CharacterManager.CharacterType, Definition>();

            foreach (var playerConfig in GameConfig.PlayerConfigs) {
                Definition definition = new Definition();
                definition.normalAttackDistance = playerConfig.normalAttackDistance;
                definition.skillAttackDistance = playerConfig.skillAttackDistance;
                definition.skillAttackCoolDown = playerConfig.skillAttackCoolDown;
                definition.maxBullets = playerConfig.maxBullets;

                var type = (CharacterManager.CharacterType)playerConfig.type;

                CharacterInfoSo.CharacterItemInfo clientData = null;
                foreach (var item in infoSo.items) {
                    if (item.characterType == type) {
                        clientData = item;
                        break;
                    }
                }

                if (clientData == null) {
                    Debug.LogError($"CharacterInfo.ctor::no data in scriptable objects for {type}");
                    continue;
                }

                definition.description = clientData.description;
                definition.iconSpriteName = clientData.iconSpriteName;

                if (!data.TryAdd(type, definition)) {
                    Debug.LogError($"CharacterInfo.ctor::duplicate types in playerConfig/{type}");
                }
            }

            Data = data;
            GameConfig.PlayerConfigs = null;
        }
    }
}