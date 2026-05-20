using UnityEngine;
using Utility;

namespace Core.Player {
    public class PlayerListener : MonoBehaviour {
        public int Id { get; set; }

        public void MoveTo(Vector3 position) {
            transform.position = position + Vector3.back;
        }
        
        public void LookAt(Vector2 direction) {
            // transform.LookAtZAxisOnly(direction);
        }

        public void Attack() {
            // attack to dir
        }
    }
}