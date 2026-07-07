using System;
using System.Collections.Generic;
using Core.Player;
using Core.Projectile;
using Core.Simulator;
using Cysharp.Threading.Tasks;
using Network;
using Popup;
using UnityEngine;

namespace Core.Controller {
    public class ClientGameLoop : MonoBehaviour {
        [SerializeField] private InputProvider inputProvider;

        public Action<Vector2, Vector2> OnReceivedInput;
        private float accumulator;
        private bool isActive;
        private bool isInitialized;

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
                if (GameManager.Instance.IsBotModeActivated) {
                    BotController.Instance.SendInputAsync().Forget();
                } else {
                    SendInputAsync().Forget();
                }
                accumulator -= InputInterval;
            }
        }

        public bool Initialize(IReadOnlyList<ReadyPlayerDto> players) {
            if (isInitialized) return false;

            if (players == null) {
                Debug.LogError("ClientGameLoop.Initialize::ready players are null.");
                return false;
            }

            PlayerManager.Instance.Initialize(players);
            ProjectileManager.Instance.Initialize();
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
        }

        private async UniTask SendInputAsync() {
            Vector2 moveDirection = inputProvider.GetMoveDirection();
            Vector2 attackDirection = inputProvider.CaptureAttackDirection();

            OnReceivedInput?.Invoke(moveDirection, attackDirection);

            await NetworkManager.Instance.SendSocketJsonAsync(new InputMessageDto {
                MoveDir = new Vector2Dto(moveDirection),
                AttackDir = new Vector2Dto(attackDirection),
                PressedAttack = attackDirection != Vector2.zero
            });
        }

        private void HandleSnapshot(SnapshotDto snapshot) {
            if (!isInitialized) {
                Debug.LogWarning("ClientGameLoop.HandleSnapshot::Not initialized.");
                return;
            }

            switch (snapshot.Status) {
                case "starting":
                    var param = new CountdownPopup.Param(snapshot.Countdown);
                    PopupManager.Instance.ShowAsync(nameof(CountdownPopup), param).Forget();
                    return;
                case "started":
                    break;
                default:
                    Debug.LogWarning($"ClientGameLoop.HandleSnapshot::unknown status/{snapshot.Status}");
                    return;
            }

            if (snapshot.Players == null) {
                Debug.LogWarning("ClientGameLoop.HandleSnapshot::snapshot players are null");
                return;
            }

            PlayerManager.Instance.ApplySnapshot(snapshot.Players);
            ProjectileManager.Instance.ApplySnapshot(snapshot.Projectiles ?? Array.Empty<ProjectileData>());

            if (!isActive) {
                SetActive(true);
            }
        }
    }
}
