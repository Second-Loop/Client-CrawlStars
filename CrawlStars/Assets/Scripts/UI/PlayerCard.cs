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
    
    private static readonly Color32 MySideBgColor = new Color32(54, 175, 255, 255);
    private static readonly Color32 OtherSideBgColor = new Color32(207, 35, 45, 255);

    public void SetData(PlayerData.CharacterType type, string name, bool isMySide) {
        characterImage.sprite = SpriteCacheHelper.Get($"Character_{type}");
        nameText.text = name;
        bg.color = isMySide ? MySideBgColor : OtherSideBgColor;
    }

    public void Release() {
        characterImage.sprite = null;
    }
}
