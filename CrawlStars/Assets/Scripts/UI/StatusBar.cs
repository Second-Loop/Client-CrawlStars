using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusBar : MonoBehaviour {
    [SerializeField] private Image barBg;
    [SerializeField] private Image barImage;
    [SerializeField] private TextMeshProUGUI progressText;

    private int maxValue;
    
    private static readonly Color32 MyColor = new Color32(14, 164, 0, 255);
    private static readonly Color32 MySideColor = new Color32(0, 205, 219, 255);
    private static readonly Color32 OtherSideColor = new Color32(210, 59, 69, 255);

    // 임시
    public float Value => barImage.fillAmount;

    public void Initialize(int maxValue) {
        this.maxValue = maxValue;
        barBg.fillAmount = 1f;
        barImage.fillAmount = 1f;
        if (progressText) progressText.text = maxValue.ToString();
        gameObject.SetActive(true);
    }

    public void SetColor(bool isMe, bool isMySide) {
        barImage.color = isMe ? MyColor : (isMySide ? MySideColor : OtherSideColor);
    }

    public void MoveValue(int to) {
        float toPercent = to / (float)maxValue;
        if (Mathf.Approximately(toPercent, barImage.fillAmount)) return;

        barImage.fillAmount = toPercent;
        if (progressText) progressText.text = to.ToString();

        barBg.DOFillAmount(toPercent, 0.5f);
    }

    public void SetNormalizedValue(float value, string text = null) {
        value = Mathf.Clamp01(value);

        barImage.fillAmount = value;
        barBg.fillAmount = value;
        if (progressText) progressText.text = text ?? Mathf.RoundToInt(value * maxValue).ToString();
        gameObject.SetActive(true);
    }
}
