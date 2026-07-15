using Core;
using Core.Player;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utility;

public class PlayerCard : MonoBehaviour {
    [SerializeField] private Image bg;
    [SerializeField] private Image characterImage;
    [SerializeField] private TextMeshProUGUI nameText;
    
    private const string MyTeamBgName = "playercard_myteam";
    private const string OpponentBgName = "playercard_opponent";

    public void SetData(CharacterManager.CharacterType type, string name, bool isMySide) {
        var info = CharacterManager.Instance.GetCharacterInfo(type);
        if (info != null) {
            characterImage.sprite = SpriteCacheHelper.Get(info.iconSpriteName);
            if (characterImage.sprite != null) {
                characterImage.SetNativeSize();
            }
        }
        nameText.text = name;

        bg.sprite = SpriteCacheHelper.Get(isMySide ? MyTeamBgName : OpponentBgName);
    }

    public void Dispose() {
        characterImage.sprite = null;
        bg.sprite = null;
    }
}
