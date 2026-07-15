using System.Collections.Generic;
using Core;
using Core.Player;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utility;
using CharacterInfo = Core.CharacterInfo;

public class CharacterItem : SelectItem {
    public void SetData(KeyValuePair<CharacterManager.CharacterType, CharacterInfo.Definition> info, UnityAction buttonAction) {
        icon.sprite = SpriteCacheHelper.Get(info.Value.iconSpriteName);
        if (icon.sprite != null) {
            icon.SetNativeSize();
        }
        title.text = info.Key.ToString();
        subTitle.text = info.Value.description;
        button.onClick.AddListener(() => OnClickButton((int)info.Key));
        button.onClick.AddListener(buttonAction);
    }

    public override void Release() {
        icon.sprite = null;
        button.onClick.RemoveAllListeners();
    }

    protected override void OnClickButton(int selectedItem) {
        CharacterManager.Instance.SetMyCharacter((CharacterManager.CharacterType)selectedItem);
    }
}
