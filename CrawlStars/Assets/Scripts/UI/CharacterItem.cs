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

public class CharacterItem : MonoBehaviour {
    [SerializeField] private Button button;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI characterName;
    [SerializeField] private TextMeshProUGUI description;

    public void SetData(KeyValuePair<CharacterManager.CharacterType, CharacterInfo.Definition> info, UnityAction buttonAction) {
        icon.sprite = SpriteCacheHelper.Get(info.Value.iconSpriteName);
        characterName.text = info.Key.ToString();
        description.text = info.Value.description;
        button.onClick.AddListener(() => OnClickButton(info.Key));
        button.onClick.AddListener(buttonAction);
    }

    public void Release() {
        icon.sprite = null;
        button.onClick.RemoveAllListeners();
    }

    private void OnClickButton(CharacterManager.CharacterType character) {
        CharacterManager.Instance.SetMyCharacter(character);
    }
}
