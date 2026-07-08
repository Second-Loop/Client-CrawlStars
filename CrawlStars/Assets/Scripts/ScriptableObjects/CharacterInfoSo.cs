using System;
using Core;
using Core.Player;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterInfo", menuName = "Scriptable Objects/CharacterInfo")]
public class CharacterInfoSo : ScriptableObject {

    [Serializable]
    public class CharacterItemInfo {
        public CharacterManager.CharacterType characterType;
        public string description;
        public string iconSpriteName;
    }

    public CharacterItemInfo[] items;
}
