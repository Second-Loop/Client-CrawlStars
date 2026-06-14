using UnityEngine;
using Utility;

namespace Core.Player {
    public class PlayerListener : MonoBehaviour {
        [SerializeField] private Transform body;
        [SerializeField] private StatusBar hpBar;

        public void Initialize(PlayerData playerData) {
            // temp
            if (playerData.Hp == 0) playerData.Hp = 2000;
            hpBar.Initialize(playerData.Hp);
            MoveTo(playerData.Pos.ToVector2());
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

        public void BeingHit(int hp) {
            hpBar.MoveValue(hp);
        }
    }
}