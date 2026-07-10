using UnityEngine;

namespace Core.Player {
    public class CooldownController {
        private float normalSeconds;
        private float skillSeconds;
        private float normalTimer;
        private float skillTimer;

        public int CurrentCharges { get; private set; }
        public int MaxCharges { get; }
        public float NormalProgress => IsFull ? 1f : Mathf.Clamp01(normalTimer / normalSeconds);
        private bool IsFull => CurrentCharges >= MaxCharges;
        
        public float SkillProgress => IsSkillCharged ? 1f : Mathf.Clamp01(skillTimer / skillSeconds);
        public bool IsSkillCharged { get; private set; } = true;

        public CooldownController(int maxBullet, float normalSeconds, float skillSeconds) {
            MaxCharges = Mathf.Max(1, maxBullet);
            this.normalSeconds = Mathf.Max(1f, normalSeconds);
            this.skillSeconds = Mathf.Max(1f, skillSeconds);
            CurrentCharges = MaxCharges;
        }

        public void Tick(float deltaTime) {
            if (IsFull) {
                normalTimer = 0f;
            } else {
                normalTimer += deltaTime;

                while (normalTimer >= normalSeconds && CurrentCharges < MaxCharges) {
                    normalTimer -= normalSeconds;
                    CurrentCharges++;
                }

                if (IsFull) {
                    normalTimer = 0f;
                }
            }

            if (IsSkillCharged) {
                skillTimer = 0f;
            } else {
                skillTimer += deltaTime;

                if (skillTimer >= skillSeconds) {
                    IsSkillCharged = true;
                    skillTimer = 0f;
                }
            }
        }

        public bool TryNormalAttack() {
            if (CurrentCharges > 0) {
                --CurrentCharges;
                return true;
            }
            return false;
        }

        public bool TrySkillAttack() {
            if (IsSkillCharged) {
                IsSkillCharged = false;
                return true;
            }
            return false;
        }
    }
}