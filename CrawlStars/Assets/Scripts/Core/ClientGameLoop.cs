using System;
using System.Collections.Generic;
using Core.Inputs;
using Core.Map;
using Core.Player;
using Core.Prediction;
using Core.Projectile;
using Cysharp.Threading.Tasks;
using Network;
using Popup;
using UnityEngine;

namespace Core {
    public class ClientGameLoop : MonoBehaviour {
        [SerializeField] private InputProvider inputProvider;
        [SerializeField] private AttackManager attackManager;

        // 데이이터에만 접근 가능하도록 한정적으로 열어둠
        public IAttackCooldownSource AttackCooldownSource => attackManager;
        public bool IsLocalPlayerDead => PlayerManager.Instance.IsLocalPlayerDead;
        public bool IsSpectating => PlayerManager.Instance.IsSpectating;
        public string ViewTargetPlayerId => PlayerManager.Instance.ViewTargetPlayerId;

        public Action<Vector2, bool> OnDetectInput;

        private IReadOnlyList<ReadyPlayerDto> curPlayers;
        private readonly LocalMovementPredictor localPredictor = new LocalMovementPredictor();

        private float accumulator;
        private Vector2 previousMoveDirection;
        private bool isActive;
        private bool isInitialized;

        private const int InputRate = 30;
        private const float InputInterval = 1f / InputRate;

        private void Start() {
            NetworkManager.Instance.SnapshotReceived += HandleSnapshot;
            NetworkManager.Instance.InputSubmitted += HandleInputSubmitted;
        }

        private void OnDestroy() {
            NetworkManager.Instance.SnapshotReceived -= HandleSnapshot;
            NetworkManager.Instance.InputSubmitted -= HandleInputSubmitted;
        }

        private void Update() {
            if (!isActive || IsLocalPlayerDead) return;

            accumulator += Time.deltaTime;
            SendInputAsync().Forget();
            UpdateLocalPrediction();

            OnDetectInput?.Invoke(inputProvider.AimDirection, inputProvider.UsedSkill);
        }

        public void Initialize(IReadOnlyList<ReadyPlayerDto> players) {
            if (isInitialized) return;

            if (players == null) {
                Debug.LogError("ClientGameLoop.Initialize::ready players are null.");
                return;
            }

            localPredictor.Reset();
            curPlayers = players;
            PlayerManager.Instance.Initialize(players);
            ProjectileManager.Instance.Initialize();
            attackManager.Initialize(serverAuthoritative: true);
            isInitialized = true;
        }

        public void SetActive(bool isActive) {
            if (isActive && !isInitialized) {
                Debug.LogError("ClientGameLoop.SetActive::Not initialized.");
                return;
            }

            this.isActive = isActive;
            if (!isActive) {
                CancelLocalPrediction();
            }
            SetActiveInput(isActive);
        }

        public void SetActiveInput(bool isActive) {
            inputProvider.IsActivated = isActive && !IsLocalPlayerDead;
        }
        
        public void Clear() {
            SetActive(false);
            accumulator = 0;
            previousMoveDirection = Vector2.zero;
            localPredictor.Reset();
            attackManager.Reset();
            isInitialized = false;
        }

        private async UniTask SendInputAsync() {
            if (IsLocalPlayerDead) return;

            Vector2 moveDirection = inputProvider.GetMoveDirection();
            Vector2 attackDirection = inputProvider.CaptureAttackDirection();

            // 쿨타임 체크
            if (attackDirection != Vector2.zero) {
                if (inputProvider.UsedSkill && !attackManager.TrySkillAttack()) {
                    attackDirection = Vector2.zero;
                } else if (!inputProvider.UsedSkill && !attackManager.TryNormalAttack()) {
                    attackDirection = Vector2.zero;
                }
            }

            // 입력 방향이 바뀌었거나 공격 키가 들어오면 interval를 무시하고 즉시 보내서 지연률 최소화
            bool isMoveChanged = (moveDirection - previousMoveDirection).sqrMagnitude > Mathf.Epsilon;
            previousMoveDirection = moveDirection;
            bool shouldSendImmediately = isMoveChanged || attackDirection != Vector2.zero;
            if (!shouldSendImmediately && accumulator < InputInterval) return;

            accumulator = shouldSendImmediately ? 0f : accumulator % InputInterval;

            await NetworkManager.Instance.SendInputAsync(moveDirection, attackDirection, inputProvider.UsedSkill);
        }

        private void HandleInputSubmitted(InputMessageDto input) {
            var listener = PlayerManager.Instance.MyListener;
            if (!isActive || IsLocalPlayerDead || attackManager.IsDashing || input == null || listener == null) return;

            Vector2 moveDirection = input.MoveDir.ToVector2();
            if (!localPredictor.HandleInput(input.ClientTick, moveDirection, listener.transform.position)) return;

            bool hasDirection = moveDirection.sqrMagnitude > Mathf.Epsilon;
            listener.SetMoving(hasDirection);
            if (hasDirection) {
                // 캐릭터 바로 회전
                listener.RotateTo(moveDirection);
            } else if (localPredictor.HasServerState) {
                // 정지 입력으로 전환된 경우에 서버 위치로 보정 (Cancel에서 대입함)
                listener.MoveTo(localPredictor.CurPosition);
            }
        }

        private void UpdateLocalPrediction() {
            if (IsLocalPlayerDead) return;

            if (attackManager.IsDashing) {
                CancelLocalPrediction();
                return;
            }

            var listener = PlayerManager.Instance.MyListener;
            if (listener == null) return;

            if (localPredictor.TryUpdatePosition(out var position)) {
                listener.MoveTo(position);
            }
        }

        private void CancelLocalPrediction() {
            localPredictor.Cancel();
            var listener = PlayerManager.Instance.MyListener;
            if (listener != null && localPredictor.HasServerState) {
                listener.MoveTo(localPredictor.CurPosition);
            }
        }

        private void HandleSnapshot(SnapshotDto snapshot) {
            if (!isInitialized) {
                Debug.LogWarning("ClientGameLoop.HandleSnapshot::Not initialized.");
                return;
            }

            switch (snapshot.Status) {
                case "starting":
                    var param = new CountdownPopup.Param(curPlayers, snapshot.Countdown);
                    PopupManager.Instance.ShowAsync(nameof(CountdownPopup), param).Forget();
                    return;
                case "started":
                    break;
                default:
                    Debug.LogWarning($"ClientGameLoop.HandleSnapshot::unknown status/{snapshot.Status}");
                    return;
            }

            if (snapshot.Players == null) {
                if (snapshot.Tick != 0) {
                    Debug.LogWarning("ClientGameLoop.HandleSnapshot::gameplay snapshot players are null");
                    attackManager.ObserveSnapshot(snapshot.Tick, null);
                    CancelLocalPrediction();
                }
                return;
            }

            PlayerManager.Instance.ObserveSnapshot(snapshot.Players);
            ObserveLocalPlayerSnapshot(snapshot.Tick, snapshot.Players);
            if (IsLocalPlayerDead) {
                CancelLocalPrediction();
                SetActiveInput(false);
            }

            PlayerManager.Instance.ApplySnapshot(
                snapshot.Players,
                !IsLocalPlayerDead && !attackManager.IsDashing && localPredictor.IsActive
            );
            BushVisibilityController.Instance.SetVisibility(snapshot.Players);
            ProjectileManager.Instance.ApplySnapshot(snapshot.Projectiles ?? Array.Empty<ProjectileData>());

            if (!isActive) {
                SetActive(true);
            }
        }

        private void ObserveLocalPlayerSnapshot(long snapshotTick, IReadOnlyList<PlayerData> players) {
            foreach (var player in players) {
                if (player != null && player.Id == PlayerManager.Instance.MyId) {
                    attackManager.ObserveSnapshot(snapshotTick, player);
                    if (!player.IsDead) {
                        localPredictor.ObserveSnapshot(player);
                    }
                    if (player.IsDead || attackManager.IsDashing) {
                        CancelLocalPrediction();
                    }
                    return;
                }
            }

            attackManager.ObserveSnapshot(snapshotTick, null);
            CancelLocalPrediction();
        }
    }
}
