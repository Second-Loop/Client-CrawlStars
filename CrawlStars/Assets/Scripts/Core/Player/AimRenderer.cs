using Core;
using UnityEngine;
using UnityEngine.UI;
using Utility;

public class AimRenderer : MonoBehaviour {
    [SerializeField] private Image aimLine;
    
    private static readonly Color32 NormalColor = new Color32(255, 255, 255, 120);
    private static readonly Color32 SkillColor = new Color32(255, 255, 0, 120);

    private const float ThicknessFactor = 120f;

    public void OnPressKey(Vector2 attackDir, bool usedSkill) {
        if (attackDir == Vector2.zero) {
            aimLine.gameObject.SetActive(false);
            return;
        }

        aimLine.color = usedSkill ? SkillColor : NormalColor;
        aimLine.rectTransform.sizeDelta = new Vector2(300f, GameConfig.ProjectileRadius * ThicknessFactor);
        float angle = MathUtil.GetAngle(attackDir);
        aimLine.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
        if (!aimLine.gameObject.activeSelf) {
            aimLine.gameObject.SetActive(true);
        }
    }
}
