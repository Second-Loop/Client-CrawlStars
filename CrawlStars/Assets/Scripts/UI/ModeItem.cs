using Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utility;

public class ModeItem : SelectItem {
    public void SetData(ModeInfo.ModeItemInfo info, UnityAction buttonAction) {
       icon.sprite = SpriteCacheHelper.Get(info.iconSpriteName);
       if (icon.sprite != null) {
           icon.SetNativeSize();
       }
       title.text = info.title;
       subTitle.text = info.subTitle;
       button.onClick.AddListener(() => OnClickButton((int)info.gameMode));
       button.onClick.AddListener(buttonAction);
    }

    public override void Release() {
        icon.sprite = null;
        button.onClick.RemoveAllListeners();
    }

    protected override void OnClickButton(int selectedItem) {
        ModeManager.Instance.CurGameMode = (ModeManager.GameMode)selectedItem;
    }
}
