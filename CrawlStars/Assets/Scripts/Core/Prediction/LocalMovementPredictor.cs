using Core.Map;
using Core.Player;
using UnityEngine;

namespace Core.Prediction {
    public class LocalMovementPredictor {
        private const float PredictionDuration = 0.12f;

        private Vector2 lastSubmittedDir;
        private Vector2 predictionDir;
        private Vector2 authoritativePos;
        private float authoritativeSpeed;
        private float elapsed;
        private long pendingClientTick;

        public bool IsActive { get; private set; }
        public bool HasAuthoritativeState { get; private set; }
        public Vector2 Position { get; private set; }

        public bool HandleInput(long clientTick, Vector2 moveDirection, Vector2 currentPosition) {
            moveDirection = Vector2.ClampMagnitude(moveDirection, 1f);
            bool isChanged = (moveDirection - lastSubmittedDir).sqrMagnitude > Mathf.Epsilon;
            lastSubmittedDir = moveDirection;
            if (!isChanged) return false;

            if (moveDirection.sqrMagnitude <= Mathf.Epsilon) {
                Cancel();
                return true;
            }

            if (!IsActive) {
                Position = currentPosition;
            }
            if (!HasAuthoritativeState || clientTick <= 0 || authoritativeSpeed <= 0f) {
                IsActive = false;
                return true;
            }

            predictionDir = moveDirection;
            pendingClientTick = clientTick;
            elapsed = 0f;
            IsActive = true;
            return true;
        }

        public void ObserveSnapshot(PlayerData player) {
            if (player == null) return;

            Vector2 nextAuthoritativePosition = player.Pos.ToVector2();
            if (IsActive) {
                Position += nextAuthoritativePosition - authoritativePos;
            }
            authoritativePos = nextAuthoritativePosition;
            authoritativeSpeed = Mathf.Max(0f, player.Speed);
            HasAuthoritativeState = true;

            if (!IsActive) {
                EndAtAuthoritativePosition();
                return;
            }

            if (player.IsDead || player.LastProcessedClientTick >= pendingClientTick) {
                EndAtAuthoritativePosition();
            }
        }

        public bool TryUpdate(float deltaTime, out Vector2 position) {
            position = Position;
            if (!IsActive) return false;

            float frameDelta = Mathf.Max(0f, deltaTime);
            elapsed += frameDelta;
            if (elapsed >= PredictionDuration) {
                EndAtAuthoritativePosition();
                position = Position;
                return true;
            }

            float progress = elapsed / PredictionDuration;
            float speedRatio = progress * progress;
            Vector2 movement = predictionDir * (authoritativeSpeed * speedRatio * frameDelta);
            Position = GamePhysics.GetNextPosition(Position, movement);
            position = Position;
            return true;
        }

        public void Cancel() {
            if (HasAuthoritativeState) {
                Position = authoritativePos;
            }
            IsActive = false;
            elapsed = 0f;
            pendingClientTick = 0;
        }

        public void Reset() {
            lastSubmittedDir = Vector2.zero;
            predictionDir = Vector2.zero;
            authoritativePos = Vector2.zero;
            authoritativeSpeed = 0f;
            Position = Vector2.zero;
            IsActive = false;
            HasAuthoritativeState = false;
            elapsed = 0f;
            pendingClientTick = 0;
        }

        private void EndAtAuthoritativePosition() {
            Position = authoritativePos;
            IsActive = false;
            elapsed = 0f;
            pendingClientTick = 0;
        }
    }
}
