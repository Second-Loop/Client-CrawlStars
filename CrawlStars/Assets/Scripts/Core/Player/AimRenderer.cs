using Core;
using UnityEngine;
using UnityEngine.UI;
using Utility;

public class AimRenderer : MonoBehaviour {
    [SerializeField] private Image aimLine;

    private float normalAttackDistance;
    private float skillAttackDistance;
    
    private static readonly Color32 NormalColor = new Color32(255, 255, 255, 120);
    private static readonly Color32 SkillColor = new Color32(255, 255, 0, 120);

    private const float ThicknessFactor = 120f;
    private const float DistanceFactor = 130f;

    public void Initialize() {
        normalAttackDistance = CharacterManager.Instance.MyCharacter.normalAttackDistance;
        skillAttackDistance = CharacterManager.Instance.MyCharacter.skillAttackDistance;
    }

    public void OnPressKey(Vector2 attackDir, bool usedSkill) {
        if (attackDir == Vector2.zero) {
            aimLine.gameObject.SetActive(false);
            return;
        }

        aimLine.color = usedSkill ? SkillColor : NormalColor;

        var distance = usedSkill ? skillAttackDistance : normalAttackDistance;
        aimLine.rectTransform.sizeDelta = new Vector2(distance * DistanceFactor, GameConfig.ProjectileRadius * ThicknessFactor);

        float angle = MathUtil.GetAngle(attackDir);
        aimLine.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);

        if (!aimLine.gameObject.activeSelf) {
            aimLine.gameObject.SetActive(true);
        }
    }
}
