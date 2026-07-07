using Core;
using Core.Player;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utility;

public class CharacterItem : MonoBehaviour {
    [SerializeField] private Button button;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI subTitle;

    public void SetData(CharacterInfo.CharacterItemInfo info, UnityAction buttonAction) {
        icon.sprite = SpriteCacheHelper.Get(info.iconSpriteName);
        title.text = info.title;
        subTitle.text = info.subTitle;
        button.onClick.AddListener(() => OnClickButton(info.character));
        button.onClick.AddListener(buttonAction);
    }

    public void Release() {
        icon.sprite = null;
        button.onClick.RemoveAllListeners();
    }

    private void OnClickButton(PlayerData.CharacterType character) {
        CharacterManager.Instance.CurCharacter = character;
    }
}
