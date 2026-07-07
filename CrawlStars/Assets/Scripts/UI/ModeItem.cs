using Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utility;

public class ModeItem : MonoBehaviour {
    [SerializeField] private Button button;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI subTitle;

    public void SetData(ModeInfo.ModeItemInfo info, UnityAction buttonAction) {
       icon.sprite = SpriteCacheHelper.Get(info.iconSpriteName);
       title.text = info.title;
       subTitle.text = info.subTitle;
       button.onClick.AddListener(() => OnClickButton(info.gameMode));
       button.onClick.AddListener(buttonAction);
    }

    public void Release() {
        icon.sprite = null;
        button.onClick.RemoveAllListeners();
    }

    private void OnClickButton(ModeManager.GameMode gameMode) {
        ModeManager.Instance.CurGameMode = gameMode;
    }
}
