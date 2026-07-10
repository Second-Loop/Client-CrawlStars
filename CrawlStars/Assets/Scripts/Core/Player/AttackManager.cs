using System;
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

        public int CurrentCharges => cooldownController?.CurrentCharges ?? 0;
        public int MaxCharges => cooldownController?.MaxCharges ?? 1;
        public float NormalProgress => cooldownController?.NormalProgress ?? 1f;
        public float SkillProgress => cooldownController?.SkillProgress ?? 1f;
        public bool IsSkillCharged => cooldownController?.IsSkillCharged ?? false;

        public void Initialize() {
            cooldownController = new CooldownController(
                CharacterManager.Instance.MyCharacter.maxBullets, 
                GameConfig.NormalAttackCoolDown, 
                CharacterManager.Instance.MyCharacter.skillAttackCoolDown
                );
        }

        private void Update() {
            if (cooldownController == null) return;

            cooldownController.Tick(Time.deltaTime);
        }

        public bool TryNormalAttack() => cooldownController.TryNormalAttack();
        public bool TrySkillAttack() => cooldownController.TrySkillAttack();
    }
}