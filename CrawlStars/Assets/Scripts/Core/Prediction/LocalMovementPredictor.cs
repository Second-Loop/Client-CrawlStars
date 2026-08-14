using Core.Map;
using Core.Player;
using UnityEngine;

namespace Core.Prediction {
    public class LocalMovementPredictor {
        private const float PredictionDuration = 0.12f;

        private Vector2 lastSubmittedDir;
        private Vector2 predictionDir;
        private Vector2 serverPos;
        private float serverSpeed;
        private float elapsed;
        private long pendingClientTick;

        public bool IsActive { get; private set; }
        public bool HasServerState { get; private set; }
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
            if (!HasServerState || clientTick <= 0 || serverSpeed <= 0f) {
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

            Vector2 nextServerPos = player.Pos.ToVector2();
            if (IsActive) {
                Position += nextServerPos - serverPos;
            }
            serverPos = nextServerPos;
            serverSpeed = Mathf.Max(0f, player.Speed);
            HasServerState = true;

            if (!IsActive) {
                EndAtServerPosition();
                return;
            }

            if (player.IsDead || player.LastProcessedClientTick >= pendingClientTick) {
                EndAtServerPosition();
            }
        }

        public bool TryUpdate(float deltaTime, out Vector2 position) {
            position = Position;
            if (!IsActive) return false;

            float frameDelta = Mathf.Max(0f, deltaTime);
            elapsed += frameDelta;
            if (elapsed >= PredictionDuration) {
                EndAtServerPosition();
                position = Position;
                return true;
            }

            float progress = elapsed / PredictionDuration;
            float speedRatio = progress * progress;
            Vector2 movement = predictionDir * (serverSpeed * speedRatio * frameDelta);
            Position = GamePhysics.GetNextPosition(Position, movement);
            position = Position;
            return true;
        }

        public void Cancel() {
            if (HasServerState) {
                Position = serverPos;
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
            Position = Vector2.zero;
            IsActive = false;
            HasServerState = false;
            elapsed = 0f;
            pendingClientTick = 0;
        }

        private void EndAtServerPosition() {
            Position = serverPos;
            IsActive = false;
            elapsed = 0f;
            pendingClientTick = 0;
        }
    }
}
