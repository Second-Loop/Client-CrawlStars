using System;
using Core;
using UnityEngine;

[CreateAssetMenu(fileName = "ModeInfo", menuName = "Scriptable Objects/ModeInfo")]
public class ModeInfo : ScriptableObject {

    [Serializable]
    public class ModeItemInfo {
        public ModeManager.GameMode gameMode;
        public string title;
        public string subTitle;
        public string iconSpriteName;
    }

    public ModeItemInfo[] items;
}
