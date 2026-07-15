using Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utility;

public abstract class SelectItem : MonoBehaviour {
    [SerializeField] protected Button button;
    [SerializeField] protected Image bg;
    [SerializeField] protected Image icon;
    [SerializeField] protected TextMeshProUGUI title;
    [SerializeField] protected TextMeshProUGUI subTitle;

    public enum BgColor {Green, Purple, Blue}

    public void SetBgColor(BgColor color) {
        bg.sprite = SpriteCacheHelper.Get($"innerbg_{color}");
    }

    public abstract void Release();
    protected abstract void OnClickButton(int selectedItem);
}