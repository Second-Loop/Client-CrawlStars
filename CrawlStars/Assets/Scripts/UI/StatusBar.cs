using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusBar : MonoBehaviour {
    [SerializeField] private Image barBg;
    [SerializeField] private Image barImage;
    [SerializeField] private TextMeshProUGUI progressText;

    private int maxValue;

    // 임시
    public float Value => barImage.fillAmount;

    public void Initialize(int maxValue) {
        this.maxValue = maxValue;
        barBg.fillAmount = 1f;
        barImage.fillAmount = 1f;
        progressText.text = maxValue.ToString();
    }

    public void MoveValue(int to) {
        float toPercent = to / (float)maxValue;
        if (Mathf.Approximately(toPercent, barImage.fillAmount)) return;

        barImage.fillAmount = toPercent;
        progressText.text = to.ToString();

        barBg.DOFillAmount(toPercent, 0.5f);
    }
}
