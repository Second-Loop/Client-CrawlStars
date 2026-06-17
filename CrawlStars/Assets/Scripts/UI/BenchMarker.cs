using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BenchMarker : MonoBehaviour {
    [SerializeField] private Image keyW;
    [SerializeField] private Image keyA;
    [SerializeField] private Image keyS;
    [SerializeField] private Image keyD;
    [SerializeField] private Image mouse;
    [SerializeField] private TextMeshProUGUI latencyText;

    public void OnPressKey(Vector2 moveDir, Vector2 attackDir) {
        keyA.color = moveDir.x < 0 ? Color.red : Color.white;
        keyD.color = moveDir.x > 0 ? Color.red : Color.white;
        keyS.color = moveDir.y < 0 ? Color.red : Color.white;
        keyW.color = moveDir.y > 0 ? Color.red : Color.white;

        if (attackDir != Vector2.zero) {
            mouse.DOKill();
            mouse.color = Color.red;
            mouse.DOColor(Color.white, 0.5f);
        }
    }
}
