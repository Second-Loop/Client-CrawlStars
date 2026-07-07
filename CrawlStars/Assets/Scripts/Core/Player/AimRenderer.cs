using Core;
using UnityEngine;
using UnityEngine.UI;
using Utility;

public class AimRenderer : MonoBehaviour {
    [SerializeField] private Image aimLine;
    
    private readonly Color32 normalColor = new Color32(255, 255, 255, 120);
    private readonly Color32 skillColor = new Color32(255, 255, 0, 120);

    private const float ThicknessFactor = 120f;

    public void OnPressKey(Vector2 attackDir, bool usedSkill) {
        if (attackDir == Vector2.zero) {
            aimLine.gameObject.SetActive(false);
            return;
        }

        aimLine.color = usedSkill ? skillColor : normalColor;
        aimLine.rectTransform.sizeDelta = new Vector2(300f, GameConfig.ProjectileRadius * ThicknessFactor);
        float angle = MathUtil.GetAngle(attackDir);
        aimLine.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
        if (!aimLine.gameObject.activeSelf) {
            aimLine.gameObject.SetActive(true);
        }
    }
}
