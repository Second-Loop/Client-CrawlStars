using UnityEngine;

namespace Core.Player {
    public class CooldownView : MonoBehaviour {
        [SerializeField] private StatusBar normalAttackBar;
        [SerializeField] private StatusBar skillAttackBar;

        private IAttackCooldownSource source;

        public void Initialize(IAttackCooldownSource source) {
            if (source == null) {
                Debug.LogError("CooldownView.Initialize::invalid parameter");
                return;
            }

            this.source = source;

            normalAttackBar?.Initialize(100);
            skillAttackBar?.Initialize(100);

            Refresh();
        }

        public void Clear() {
            source = null;
        }

        private void LateUpdate() {
            if (source == null) return;

            Refresh();
        }

        private void Refresh() {
            int maxCharges = Mathf.Max(1, source.MaxCharges);
            int currentCharges = Mathf.Clamp(source.CurrentCharges, 0, maxCharges);
            float normalValue = currentCharges >= maxCharges
                ? 1f
                : (currentCharges + source.NormalProgress) / maxCharges;

            normalAttackBar?.SetNormalizedValue(normalValue, $"{currentCharges}/{maxCharges}");

            string skillText = source.IsSkillCharged
                ? "Ready"
                : $"{Mathf.RoundToInt(source.SkillProgress * 100f)}%";
            skillAttackBar?.SetNormalizedValue(source.SkillProgress, skillText);
        }
    }
}
