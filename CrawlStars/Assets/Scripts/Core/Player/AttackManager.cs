using Core.Character;
using UnityEngine;

namespace Core.Player {

    // 데이이터에만 접근 가능하도록 한정적으로 열어두기 위함
    public interface IAttackCooldownSource {
        int CurrentCharges { get; }
        int MaxCharges { get; }
        float NormalProgress { get; }
        float SkillProgress { get; }
        bool IsSkillCharged { get; }
    }

    public class AttackManager : MonoBehaviour, IAttackCooldownSource {
        private CooldownController cooldownController;
        private AuthoritativeCombatState authoritativeCombatState;
        private bool serverAuthoritative;

        public int CurrentCharges => serverAuthoritative
            ? authoritativeCombatState?.CurrentCharges ?? 0
            : cooldownController?.CurrentCharges ?? 0;
        public int MaxCharges => serverAuthoritative
            ? authoritativeCombatState?.MaxCharges ?? 1
            : cooldownController?.MaxCharges ?? 1;
        public float NormalProgress => serverAuthoritative
            ? authoritativeCombatState?.NormalProgress ?? 0f
            : cooldownController?.NormalProgress ?? 0f;
        public float SkillProgress => serverAuthoritative
            ? authoritativeCombatState?.SkillProgress ?? 0f
            : cooldownController?.SkillProgress ?? 0f;
        public bool IsSkillCharged => serverAuthoritative
            ? authoritativeCombatState?.IsSkillCharged ?? false
            : cooldownController?.IsSkillCharged ?? false;

        public void Initialize(bool serverAuthoritative = false) {
            this.serverAuthoritative = serverAuthoritative;
            var character = CharacterManager.Instance.MyCharacter;

            if (serverAuthoritative) {
                cooldownController = null;
                authoritativeCombatState = new AuthoritativeCombatState(
                    character.maxBullets,
                    GameConfig.NormalAttackCoolDown,
                    character.skillAttackCoolDown
                );
                return;
            }

            authoritativeCombatState = null;
            cooldownController = new CooldownController(
                character.maxBullets,
                GameConfig.NormalAttackCoolDown, 
                character.skillAttackCoolDown
            );
        }

        private void Update() {
            if (serverAuthoritative) {
                authoritativeCombatState?.Tick(Time.deltaTime);
            } else {
                cooldownController?.Tick(Time.deltaTime);
            }
        }

        public void ObserveSnapshot(long tick, PlayerData player) {
            if (!serverAuthoritative) return;
            authoritativeCombatState?.Observe(tick, player);
        }

        public void Reset() {
            authoritativeCombatState?.Reset();
        }

        public bool TryNormalAttack() => serverAuthoritative
            ? authoritativeCombatState?.CanNormalAttack ?? false
            : cooldownController?.TryNormalAttack() ?? false;

        public bool TrySkillAttack() => serverAuthoritative
            ? authoritativeCombatState?.CanSkillAttack ?? false
            : cooldownController?.TrySkillAttack() ?? false;
    }
}
