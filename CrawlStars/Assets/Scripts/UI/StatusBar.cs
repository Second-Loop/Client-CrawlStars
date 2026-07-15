using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility;

public class StatusBar : MonoBehaviour {
    [SerializeField] private Image barBg;
    [SerializeField] private Image barImage;
    [SerializeField] private TextMeshProUGUI progressText;

    [SerializeField] private GameObject dividerRoot;
    [SerializeField] private RectTransform[] dividers;

    private int maxValue;
    
    private const string MyBg = "progress_green";
    private const string MySideBg = "progress_blue";
    private const string OtherSideBg = "progress_red";

    // 임시
    public float Value => barImage.fillAmount;

    public void Initialize(int maxValue) {
        this.maxValue = maxValue;
        barBg.fillAmount = 1f;
        barImage.fillAmount = 1f;
        if (progressText) progressText.text = maxValue.ToString();
        if (dividerRoot) dividerRoot.SetActive(false);
        gameObject.SetActive(true);
    }

    public void Dispose() {
        barImage.sprite = null;
    }

    public void SetDivider(int count) {
        if (dividerRoot == null || dividers == null) return;

        count = Mathf.Clamp(count, 1, 5);
        int activeDividerCount = Mathf.Min(count - 1, dividers.Length);

        dividerRoot.SetActive(activeDividerCount > 0);
        for (int i = 0; i < dividers.Length; i++) {
            var divider = dividers[i];
            if (divider == null) continue;

            bool isActive = i < activeDividerCount;
            divider.gameObject.SetActive(isActive);
            if (!isActive) continue;

            float normalizedPosition = (i + 1) / (float)count;
            divider.anchorMin = new Vector2(normalizedPosition, divider.anchorMin.y);
            divider.anchorMax = new Vector2(normalizedPosition, divider.anchorMax.y);
            divider.anchoredPosition = new Vector2(0f, divider.anchoredPosition.y);
        }
    }

    public void SetColor(bool isMe, bool isMySide) {
        var spriteName = isMe ? MyBg : (isMySide ? MySideBg : OtherSideBg);
        barImage.sprite = SpriteCacheHelper.Get(spriteName);
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
