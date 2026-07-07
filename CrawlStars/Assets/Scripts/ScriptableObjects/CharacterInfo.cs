using System;
using Core;
using Core.Player;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterInfo", menuName = "Scriptable Objects/CharacterInfo")]
public class CharacterInfo : ScriptableObject {

    [Serializable]
    public class CharacterItemInfo {
        public PlayerData.CharacterType character;
        public string title;
        public string subTitle;
        public string iconSpriteName;
    }

    public CharacterItemInfo[] items;
}
