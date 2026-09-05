using System.Collections.Generic;
using CameraControl;
using Network;
using UnityEngine;
using Utility;
using Cache = Utility.Cache;

namespace Core.Player {
    public class PlayerManager {
        private static PlayerManager instance;
        public static PlayerManager Instance => instance ??= new PlayerManager();

        // 임시 public
        public readonly Dictionary<string, PlayerListener> playerListeners = new Dictionary<string, PlayerListener>();

        private readonly SpectatorState spectatorState = new SpectatorState();

        public PlayerListener MyListener { get; private set; }
        public PlayerListener ViewListener { get; private set; }
        public string MyId { get; set; }
        public string MyTeam { get; set; }
        public bool IsLocalPlayerDead => spectatorState.IsLocalPlayerDead;
        public bool IsSpectating => spectatorState.IsSpectating;
        public string ViewTargetPlayerId => spectatorState.TargetPlayerId;

        public void Initialize(IReadOnlyList<ReadyPlayerDto> players) {
            ClearListeners();

            foreach (var player in players) {
                if (player == null || string.IsNullOrEmpty(player.Id)) {
                    Debug.LogError("PlayerManager.Initialize::invalid data from server");
                    continue;
                }

                var listener = ObjectPooling.Instance.Get<PlayerListener>(Constants.Player);
                if (listener == null) continue;

                bool isMe = player.Id == MyId;
                listener.Initialize(player, isMe);
                listener.gameObject.SetActive(true);
                playerListeners.Add(player.Id, listener);

                if (isMe) {
                    MyListener = listener;
                    ViewListener = listener;
                }
            }
        }

        public void ObserveSnapshot(IReadOnlyList<PlayerData> players) {
            spectatorState.Observe(players, MyId);
        }

        public void ApplySnapshot(IReadOnlyList<PlayerData> players, bool preserveLocalMovement = false) {
            // Resolve deaths first so a spectator target never points at an object returned to the pool.
            foreach (var player in players) {
                if (player == null || string.IsNullOrEmpty(player.Id)) continue;

                if (!playerListeners.TryGetValue(player.Id, out var listener)) {
                    continue;
                }

                if (player.IsDead) {
                    ObjectPooling.Instance.TryAbandon(Constants.Player, listener.gameObject);
                    playerListeners.Remove(player.Id);
                    if (ReferenceEquals(MyListener, listener)) MyListener = null;
                    if (ReferenceEquals(ViewListener, listener)) ViewListener = null;
                }
            }

            foreach (var player in players) {
                if (player == null || string.IsNullOrEmpty(player.Id) || player.IsDead) continue;

                if (!playerListeners.TryGetValue(player.Id, out var listener)) {
                    // 살아있는데 없으면 에러
                    Debug.LogError($"PlayerManager.ApplySnapshot::PlayerId not found:{player.Id}");
                    continue;
                }

                bool shouldPreserveMovement = preserveLocalMovement && player.Id == MyId;
                if (!shouldPreserveMovement) {
                    var moveDirection = player.MoveDir.ToVector2();
                    listener.MoveTo(player.Pos.ToVector2());
                    listener.RotateTo(moveDirection);
                    listener.SetMoving(moveDirection.sqrMagnitude > Mathf.Epsilon);
                }

                if (player.PressedAttack) {
                    listener.RotateToAttack(player.AttackDir.ToVector2());
                    listener.Attack(player.AttackDir.ToVector2());
                }
                
                listener.BeingHit(player.Hp);
            }

            ResolveViewListener();
        }

        public void FocusCamera() {
            Cache.CameraController.TargetPlayer = ViewListener?.transform;
        }

        public void ClearListeners() {
            foreach (var playerListener in playerListeners) {
                ObjectPooling.Instance.TryAbandon(Constants.Player, playerListener.Value.gameObject);
            }
            playerListeners.Clear();
            Cache.CameraController.TargetPlayer = null;
            MyListener = null;
            ViewListener = null;
            spectatorState.Reset();
        }

        public bool GetListener(string id, out PlayerListener listener) => playerListeners.TryGetValue(id, out listener);

        private void ResolveViewListener() {
            ViewListener = null;
            if (spectatorState.TargetPlayerId != null) {
                playerListeners.TryGetValue(spectatorState.TargetPlayerId, out var listener);
                ViewListener = listener;
            }

            FocusCamera();
        }
    }
}
