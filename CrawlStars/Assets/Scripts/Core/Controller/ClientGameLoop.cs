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
            if (NetworkManager.Instance != null) {
                NetworkManager.Instance.SnapshotReceived -= HandleSnapshot;
            }
        }

        private void Update() {
            if (!isActive) return;

            accumulator += Time.deltaTime;
            if (accumulator >= InputInterval) {
                SendInputAsync().Forget(e => Debug.LogError($"ClientGameLoop.SendInputAsync::{e.Message}"));
                accumulator -= InputInterval;
            }
        }

        public bool Initialize() {
            if (latestSnapshot == null) {
                Debug.LogError("ClientGameLoop.Initialize::snapshot is not received.");
                return false;
            }

            PlayerManager.Instance.Initialize(
                latestSnapshot.Players ?? Array.Empty<PlayerData>()
            );
            ProjectileManager.Instance.Initialize(
                latestSnapshot.Projectiles ?? Array.Empty<ProjectileData>()
            );
            isInitialized = true;
            return true;
        }

        public void SetActive(bool isActive) {
            if (isActive && !isInitialized) {
                Debug.LogError("ClientGameLoop.SetActive::not initialized.");
                return;
            }

            this.isActive = isActive;
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
            return NetworkManager.Instance.SendSocketJsonAsync(new InputMessage {
                MoveDir = new Vector2Dto(moveDirection),
                AttackDir = new Vector2Dto(attackDirection),
                PressedAttack = attackDirection != Vector2.zero
            });
        }

        private void HandleSnapshot(SnapshotDto snapshot) {
            latestSnapshot = snapshot;

            if (!isInitialized || !isActive) return;
            if (snapshot.Players == null) {
                Debug.LogError("ClientGameLoop.HandleSnapshot::Players is not received.");
                return;
            }
            if (snapshot.Projectiles == null) {
                Debug.LogError("ClientGameLoop.HandleSnapshot::Projectiles is not received.");
                return;
            }

            PlayerManager.Instance.ApplySnapshot(snapshot.Players);
            ProjectileManager.Instance.ApplySnapshot(snapshot.Projectiles);
        }
    }
}
