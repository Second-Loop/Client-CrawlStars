using UnityEngine;
using Utility;

namespace Core.Controller {
    public class InputProvider : MonoBehaviour {
        public Vector2 AimDirection { get; private set; }
        private Vector2 attackDirection;
        
        private void Update() {
            // 조준
            if (Input.GetKey(KeyCode.Mouse0)) {
                AimDirection = GetMouseWorldPos();
            }

            // 발사
            if (Input.GetKeyUp(KeyCode.Mouse0)) {
                attackDirection = AimDirection;
                AimDirection = Vector2.zero;
            }
        }

        public Vector2 CaptureAttackDirection() {
            var ret = attackDirection;
            attackDirection = Vector2.zero;
            return ret;
        }

        public Vector2 GetMoveDirection() {
            float left = Input.GetKey(KeyCode.A) ? -1 : 0;
            float right = Input.GetKey(KeyCode.D) ? 1 : 0;
            float up = Input.GetKey(KeyCode.W) ? 1 : 0;
            float down = Input.GetKey(KeyCode.S) ? -1 : 0;

            var direction = new Vector2(left + right, up + down).normalized;
            return direction;
        }

        private Vector2 GetMouseWorldPos() {
            Vector3 mouseScreenPos = Input.mousePosition;
            mouseScreenPos.z = -CommonCache.MainCamera.transform.position.z;

            Vector3 worldPos = CommonCache.MainCamera.ScreenToWorldPoint(mouseScreenPos);
            return new Vector2(worldPos.x, worldPos.y);
        }
    }
}