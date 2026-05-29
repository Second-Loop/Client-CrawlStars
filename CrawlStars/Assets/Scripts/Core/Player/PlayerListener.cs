using UnityEngine;
using Utility;

namespace Core.Player {
    public class PlayerListener : MonoBehaviour {
        [SerializeField] private Transform body;
        [SerializeField] private StatusBar hpBar;
        
        public void MoveTo(Vector3 position) {
            transform.position = position + Vector3.back;
        }
        
        public void RotateTo(Vector2 direction) {
            if (direction == Vector2.zero) return;

            float angle = MathUtil.GetAngle(direction);
            body.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        public void Attack(Vector2 direction) {
            if (direction == Vector2.zero) return;

            // attack to dir
        }
    }
}