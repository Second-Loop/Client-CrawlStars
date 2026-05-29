using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour {
    [SerializeField] private Image progressBar;
    [SerializeField] private TextMeshProUGUI progressText;

    public void Initialize() {
        progressBar.fillAmount = 0;
        progressText.text = "Loading...";
    }

    public void SetValue(float value) {
        progressBar.fillAmount = value;
        progressText.text = $"{(int)(value * 100)}%";
    }
}
