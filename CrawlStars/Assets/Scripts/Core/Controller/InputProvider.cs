using UnityEngine;
using Utility;
using Cache = Utility.Cache;

namespace Core.Controller {
    public class InputProvider : MonoBehaviour {
        public Vector2 AimDirection { get; private set; }
        private Vector2 attackDirection;
        public bool IsActivated { get; set; }
        
        private void Update() {
            if (!IsActivated) {
                AimDirection = Vector2.zero;
                attackDirection = Vector2.zero;
                return;
            }

            // 조준
            if (Input.GetKey(KeyCode.Mouse0)) {
                var mouseWorldPos = GetMouseWorldPos();
                AimDirection = (mouseWorldPos - (Vector2)Cache.MainCamera.transform.position).normalized;
            }

            // 발사
            if (Input.GetKeyUp(KeyCode.Mouse0)) {
                attackDirection = AimDirection;
                AimDirection = Vector2.zero;
            }
        }

        // Update 에서 인풋을 감지하고 저장 -> Tick 에서 저장한 인풋을 가져옴
        // Tick보다 Update가 자주 돌아서 놓치는 인풋이 없게 하기 위함
        public Vector2 CaptureAttackDirection() {
            var ret = attackDirection;
            attackDirection = Vector2.zero;
            return ret;
        }

        public Vector2 GetMoveDirection() {
            if (!IsActivated) return Vector2.zero;

            float left = Input.GetKey(KeyCode.A) ? -1 : 0;
            float right = Input.GetKey(KeyCode.D) ? 1 : 0;
            float up = Input.GetKey(KeyCode.W) ? 1 : 0;
            float down = Input.GetKey(KeyCode.S) ? -1 : 0;

            var direction = new Vector2(left + right, up + down).normalized;
            return direction;
        }

        private Vector2 GetMouseWorldPos() {
            Vector3 mouseScreenPos = Input.mousePosition;
            mouseScreenPos.z = -Cache.MainCamera.transform.position.z;
            return Cache.MainCamera.ScreenToWorldPoint(mouseScreenPos);
        }
    }
}