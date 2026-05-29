using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusBar : MonoBehaviour {
    [SerializeField] private Image barBg;
    [SerializeField] private Image barImage;
    [SerializeField] private TextMeshProUGUI progressText;

    private int maxValue;

    public void Initialize(int maxValue) {
        this.maxValue = maxValue;
        barBg.fillAmount = 1f;
        barImage.fillAmount = 1f;
        progressText.text = maxValue.ToString();
    }

    public void SetValue(int value) {
        barBg.fillAmount = value / (float)maxValue;
        barImage.fillAmount = value / (float)maxValue;
        progressText.text = value.ToString();
    }

    public async UniTask SetValueAsync(int value) {
        
    }
}
