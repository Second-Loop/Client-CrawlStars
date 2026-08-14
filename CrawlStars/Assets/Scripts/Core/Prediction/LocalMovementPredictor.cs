using Core.Map;
using Core.Player;
using UnityEngine;

namespace Core.Prediction {
    /// <summary>
    /// 멈춰있다가 이동이 시작되거나, 방향이 변경된 시점부터 PredictionDuration 초 동안만 움직임 예측 적용
    /// 예측 이후 서버 위치로 보정하는 과정에서 이전 위치로 끌어당겨질 수 있는 경험은 부작용이라고 판단하여, 반응성이 중요한 시점에만 적용하기 위함
    /// </summary>
    public class LocalMovementPredictor {
        private const float PredictionDuration = 0.12f;

        private Vector2 lastSubmittedDir;
        private Vector2 predictionDir;
        private Vector2 serverPos;
        private float serverSpeed;
        private float elapsed;
        private long pendingClientTick;

        // serverPos와 serverSpeed를 안전하게 사용할 수 있는지 확인
        public bool HasServerState { get; private set; }

        // 현재 서버 응답을 기다리면서 위치를 예측 중인지 확인
        public bool IsActive { get; private set; }

        public Vector2 CurPosition { get; private set; }

        // 입력을 서버로 전송하기 직전
        public bool HandleInput(long clientTick, Vector2 moveDirection, Vector2 currentPosition) {
            moveDirection = Vector2.ClampMagnitude(moveDirection, 1f);
            bool isDirChanged = (moveDirection - lastSubmittedDir).sqrMagnitude > Mathf.Epsilon;
            lastSubmittedDir = moveDirection;
            if (!isDirChanged) return false;

            // 정지 입력으로 전환된 경우에 예측 취소
            if (moveDirection.sqrMagnitude <= Mathf.Epsilon) {
                Cancel();
                return true;
            }

            if (!IsActive) {
                CurPosition = currentPosition;
            }

            if (!HasServerState || clientTick <= 0 || serverSpeed <= 0f) {
                IsActive = false;
                return true;
            }

            predictionDir = moveDirection;
            pendingClientTick = clientTick;

            // 방향이 바뀐 상황이므로 elapsed 초기화하여 예측 활성화
            elapsed = 0f;
            IsActive = true;
            return true;
        }

        // 스냅샷을 서버로부터 받은 직후
        public void ObserveSnapshot(PlayerData player) {
            if (player == null) {
                Debug.LogError("LocalMovementPredictor.ObserveSnapshot::player is null");
                return;
            }

            Vector2 nextServerPos = player.Pos.ToVector2();
            if (IsActive) {
                // 서버 위치 변화량을 예측 위치에 반영하여 부드러운 움직임으로 보여지게 적용 
                CurPosition += nextServerPos - serverPos;
            }

            serverPos = nextServerPos;
            serverSpeed = Mathf.Max(0f, player.Speed);
            HasServerState = true;

            if (!IsActive) {
                EndAtServerPosition();
                return;
            }

            // 이미 서버에서 처리된 움직임은 예측할 필요 없음
            if (player.IsDead || player.LastProcessedClientTick >= pendingClientTick) {
                EndAtServerPosition();
            }
        }

        public bool TryUpdatePosition(out Vector2 position) {
            position = CurPosition;
            if (!IsActive) return false;

            float deltaTime = Time.deltaTime;
            elapsed += deltaTime;

            // PredictionDuration 이후에는 예측 적용x
            if (elapsed >= PredictionDuration) {
                EndAtServerPosition();
                position = CurPosition;
                return true;
            }

            // 점점 빠르게 이동
            float progress = elapsed / PredictionDuration;
            float speedRatio = progress * progress;
            Vector2 movement = predictionDir * (serverSpeed * speedRatio * deltaTime);

            CurPosition = GamePhysics.GetNextPosition(CurPosition, movement);
            position = CurPosition;
            return true;
        }

        public void Cancel() {
            if (HasServerState) {
                // 예측을 취소하고 서버 위치로 덮어씀
                CurPosition = serverPos;
            }

            IsActive = false;
            elapsed = 0f;
            pendingClientTick = 0;
        }

        public void Reset() {
            lastSubmittedDir = Vector2.zero;
            predictionDir = Vector2.zero;
            serverPos = Vector2.zero;
            serverSpeed = 0f;
            CurPosition = Vector2.zero;
            IsActive = false;
            HasServerState = false;
            elapsed = 0f;
            pendingClientTick = 0;
        }

        private void EndAtServerPosition() {
            CurPosition = serverPos;
            IsActive = false;
            elapsed = 0f;
            pendingClientTick = 0;
        }
    }
}