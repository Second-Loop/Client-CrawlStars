using Network;
using UnityEngine;
using Utility;

namespace Core.Player {
    public class PlayerListener : MonoBehaviour {
        [SerializeField] private Transform body;
        [SerializeField] private StatusBar hpBar;
        [SerializeField] private SpriteRenderer aura;

        private bool isStatusInitialized;

        private static readonly Color32 MyAuraColor = new Color32(23, 212, 29, 150);
        private static readonly Color32 MySideAuraColor = new Color32(0, 198, 255, 150);
        private static readonly Color32 OtherSideAuraColor = new Color32(255, 0, 0, 150);
        
        // 임시
        public float Hp => hpBar.Value;

        public void Initialize(ReadyPlayerDto playerData, bool isMe) {
            isStatusInitialized = false;

            bool isMySide = playerData.Team == PlayerManager.Instance.MyTeam;

            aura.color = isMe ? MyAuraColor : (isMySide ? MySideAuraColor : OtherSideAuraColor);
            hpBar.gameObject.SetActive(false);
            hpBar.SetColor(isMe, isMySide);

            MoveTo(playerData.SpawnPosition.ToVector2());
        }

        public void ApplyStatus(float hp) {
            int roundedHp = Mathf.RoundToInt(hp);
            if (!isStatusInitialized) {
                hpBar.Initialize(roundedHp);
                isStatusInitialized = true;
                return;
            }

            hpBar.MoveValue(roundedHp);
        }
        
        public void MoveTo(Vector3 position) {
            transform.position = position + Vector3.back;
        }
        
        public void RotateTo(Vector2 direction) {
            if (direction == Vector2.zero) return;

            float angle = MathUtil.GetAngle(direction);
            body.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        // 공격 모션임
        public void Attack(Vector2 direction) {
            if (direction == Vector2.zero) return;

            // attack to dir
        }

        public void BeingHit(float hp) {
            ApplyStatus(hp);
        }
    }
}