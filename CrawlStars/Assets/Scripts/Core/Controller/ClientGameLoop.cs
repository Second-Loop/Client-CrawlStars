using System;
using Core.Player;
using Core.Projectile;
using Cysharp.Threading.Tasks;
using Network;
using UnityEngine;

namespace Core.Controller {
    public class ClientGameLoop : MonoBehaviour {
        [SerializeField] private InputProvider inputProvider;

        private float accumulator;
        private bool isActive;
        private bool isInitialized;
        private SnapshotDto latestSnapshot;

        private const int InputRate = 30;
        private const float InputInterval = 1f / InputRate;

        private void Start() {
            NetworkManager.Instance.SnapshotReceived += HandleSnapshot;
        }

        private void OnDestroy() {
            NetworkManager.Instance.SnapshotReceived -= HandleSnapshot;
        }

        private void Update() {
            if (!isActive) return;

            accumulator += Time.deltaTime;
            if (accumulator >= InputInterval) {
                SendInputAsync().Forget();
                accumulator -= InputInterval;
            }
        }

        public bool Initialize() {
            if (isInitialized) return false;

            if (latestSnapshot == null || latestSnapshot.Players == null) {
                Debug.LogError("ClientGameLoop.Initialize::snapshot is not received.");
                return false;
            }

            PlayerManager.Instance.Initialize(latestSnapshot.Players);
            ProjectileManager.Instance.Initialize(latestSnapshot.Projectiles);
            isInitialized = true;
            return true;
        }

        public void SetActive(bool isActive) {
            if (isActive && !isInitialized) {
                Debug.LogError("ClientGameLoop.SetActive::Not initialized.");
                return;
            }

            this.isActive = isActive;
            SetActiveInput(isActive);
        }

        public void SetActiveInput(bool isActive) {
            inputProvider.IsActivated = isActive;
        }
        
        public void Clear() {
            SetActive(false);
            accumulator = 0;
            isInitialized = false;
            latestSnapshot = null;
        }

        private UniTask SendInputAsync() {
            Vector2 moveDirection = inputProvider.GetMoveDirection();
            Vector2 attackDirection = inputProvider.CaptureAttackDirection();

            return NetworkManager.Instance.SendSocketJsonAsync(new InputMessageDto {
                MoveDir = new Vector2Dto(moveDirection),
                AttackDir = new Vector2Dto(attackDirection),
                PressedAttack = attackDirection != Vector2.zero
            });
        }

        private void HandleSnapshot(SnapshotDto snapshot) {
            latestSnapshot = snapshot;

            if (!isInitialized || !isActive) {
                Debug.LogWarning("ClientGameLoop.HandleSnapshot::Not initialized.");
                return;
            }

            if (snapshot.Players == null || snapshot.Projectiles == null) {
                Debug.LogWarning($"ClientGameLoop.HandleSnapshot::Data of snapshot is null/{snapshot.Players}/{snapshot.Projectiles}");
                return;
            }

            PlayerManager.Instance.ApplySnapshot(snapshot.Players);
            ProjectileManager.Instance.ApplySnapshot(snapshot.Projectiles);
        }
    }
}
