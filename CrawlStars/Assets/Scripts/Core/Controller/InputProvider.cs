using UnityEngine;
using Utility;
using Cache = Utility.Cache;

namespace Core.Controller {
    [DefaultExecutionOrder(-100)] // ClientGameLoop.Update 보다 먼저 실행되도록 보장
    public class InputProvider : MonoBehaviour {
        private enum AimButton { None, Left, Right }

        public Vector2 AimDirection { get; private set; }
        private Vector2 attackDirection;
        public bool IsActivated { get; set; }
        public bool UsedSkill { get; private set; }
        private AimButton currentAimButton = AimButton.None;

        private void Update() {
            if (!IsActivated) {
                AimDirection = Vector2.zero;
                attackDirection = Vector2.zero;
                return;
            }

            // 조준 시작
            if (currentAimButton == AimButton.None) {
                bool leftDown = Input.GetKeyDown(KeyCode.Mouse0);
                bool rightDown = Input.GetKeyDown(KeyCode.Mouse1);

                // 같은 프레임에 둘 다 눌리면 무시
                if (leftDown && !rightDown) {
                    currentAimButton = AimButton.Left;
                    UsedSkill = false;
                } else if (rightDown && !leftDown) {
                    currentAimButton = AimButton.Right;
                    UsedSkill = true;
                }
            }

            // 조준 중
            bool isAiming = (currentAimButton == AimButton.Left && Input.GetKey(KeyCode.Mouse0)) ||
                            (currentAimButton == AimButton.Right && Input.GetKey(KeyCode.Mouse1));
            if (isAiming) {
                var mouseWorldPos = GetMouseWorldPos();
                AimDirection = (mouseWorldPos - (Vector2)Cache.MainCamera.transform.position).normalized;
            }

            // 발사
            bool released = (currentAimButton == AimButton.Left && Input.GetKeyUp(KeyCode.Mouse0)) ||
                            (currentAimButton == AimButton.Right && Input.GetKeyUp(KeyCode.Mouse1));
            if (released) {
                attackDirection = AimDirection;
                AimDirection = Vector2.zero;
                currentAimButton = AimButton.None;
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