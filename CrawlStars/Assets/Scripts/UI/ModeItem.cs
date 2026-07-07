using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility;

public class ModeItem : MonoBehaviour {
    [SerializeField] private Button button;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI subTitle;

    public void SetData(ModeInfo.ModeItemInfo info) {
       icon.sprite = SpriteCacheHelper.Get(info.iconSpriteName);
       title.text = info.title;
       subTitle.text = info.subTitle;
    }

    public void Release() {
        icon.sprite = null;
    }
}
