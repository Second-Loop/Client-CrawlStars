using UnityEngine;
using Utility;

namespace Core.Player {
    public class PlayerListener : MonoBehaviour {
        public int Id { get; set; }

        public void MoveTo(Vector3 position) {
            transform.position = position + Vector3.back;
        }
        
        public void RotateTo(Vector2 direction) {
            if (direction.sqrMagnitude <= 0.0001f) return;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        public void Attack(Vector2 direction) {
            
            // attack to dir
        }
    }
}