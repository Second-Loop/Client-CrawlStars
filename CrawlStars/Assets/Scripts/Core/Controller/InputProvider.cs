using UnityEngine;

namespace Core.Controller {
    public class InputProvider : MonoBehaviour {
        public Vector2 GetMoveDirection() {
            float left = Input.GetKey(KeyCode.A) ? -1 : 0;
            float right = Input.GetKey(KeyCode.D) ? 1 : 0;
            float up = Input.GetKey(KeyCode.W) ? 1 : 0;
            float down = Input.GetKey(KeyCode.S) ? -1 : 0;

            var direction = new Vector2(left + right, up + down).normalized;
            return direction;
        }

        public Vector2 GetLookingSide() {
            return Vector2.zero;
        }

        public bool GetAttack() {
            return Input.GetKey(KeyCode.Mouse0);
        }
    }
}