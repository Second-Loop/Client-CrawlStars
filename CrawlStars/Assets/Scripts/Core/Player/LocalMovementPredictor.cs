using Core.Map;
using UnityEngine;

namespace Core.Player {
    public class LocalMovementPredictor {
        private const float PredictionDuration = 0.12f;

        private Vector2 lastSubmittedDirection;
        private Vector2 predictionDirection;
        private Vector2 authoritativePosition;
        private float authoritativeSpeed;
        private float authoritativeRadius;
        private float elapsed;
        private long pendingClientTick;

        public bool IsActive { get; private set; }
        public bool HasAuthoritativeState { get; private set; }
        public Vector2 Position { get; private set; }

        public bool HandleInput(long clientTick, Vector2 moveDirection, Vector2 currentPosition) {
            moveDirection = Vector2.ClampMagnitude(moveDirection, 1f);
            bool isChanged = (moveDirection - lastSubmittedDirection).sqrMagnitude > Mathf.Epsilon;
            lastSubmittedDirection = moveDirection;
            if (!isChanged) return false;

            if (moveDirection.sqrMagnitude <= Mathf.Epsilon) {
                Cancel();
                return true;
            }

            Position = currentPosition;
            if (!HasAuthoritativeState || clientTick <= 0 || authoritativeSpeed <= 0f) {
                IsActive = false;
                return true;
            }

            predictionDirection = moveDirection;
            pendingClientTick = clientTick;
            elapsed = 0f;
            IsActive = true;
            return true;
        }

        public void ObserveSnapshot(PlayerData player) {
            if (player == null) return;

            authoritativePosition = player.Pos.ToVector2();
            authoritativeSpeed = Mathf.Max(0f, player.Speed);
            authoritativeRadius = Mathf.Max(0f, player.Radius);
            HasAuthoritativeState = true;

            if (!IsActive || player.IsDead || player.LastProcessedClientTick >= pendingClientTick) {
                EndAtAuthoritativePosition();
            }
        }

        public bool TryUpdate(float deltaTime, MapData mapData, out Vector2 position) {
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
            Vector2 movement = predictionDirection * (authoritativeSpeed * speedRatio * frameDelta);
            Position = PlayerMapMovement.Move(Position, movement, authoritativeRadius, mapData);
            position = Position;
            return true;
        }

        public void Cancel() {
            if (HasAuthoritativeState) {
                Position = authoritativePosition;
            }
            IsActive = false;
            elapsed = 0f;
            pendingClientTick = 0;
        }

        public void Reset() {
            lastSubmittedDirection = Vector2.zero;
            predictionDirection = Vector2.zero;
            authoritativePosition = Vector2.zero;
            authoritativeSpeed = 0f;
            authoritativeRadius = 0f;
            Position = Vector2.zero;
            IsActive = false;
            HasAuthoritativeState = false;
            elapsed = 0f;
            pendingClientTick = 0;
        }

        private void EndAtAuthoritativePosition() {
            Position = authoritativePosition;
            IsActive = false;
            elapsed = 0f;
            pendingClientTick = 0;
        }
    }
}
