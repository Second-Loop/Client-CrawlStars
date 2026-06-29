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

        private Dictionary<string, PlayerListener> playerListeners = new Dictionary<string, PlayerListener>();
        private PlayerListener myListener;
        
        public string MyId { get; set; }

        public void Initialize(IReadOnlyList<ReadyPlayerDto> players) {
            ClearListeners();

            foreach (var player in players) {
                if (player == null || string.IsNullOrEmpty(player.Id)) {
                    Debug.LogError("PlayerManager.Initialize::invalid data from server");
                    continue;
                }

                var listener = ObjectPooling.Instance.Get<PlayerListener>(Constants.Player);
                if (listener == null) continue;

                listener.Initialize(player);
                playerListeners.Add(player.Id, listener);

                if (player.Id == MyId) {
                    myListener = listener;
                }
            }
        }

        public void ApplySnapshot(IReadOnlyList<PlayerData> players) {
            foreach (var player in players) {
                if (player == null || string.IsNullOrEmpty(player.Id)) continue;

                if (!playerListeners.TryGetValue(player.Id, out var listener)) {
                    if (player.IsDead) continue;

                    // 살아있는데 없으면 에러
                    Debug.LogError($"PlayerManager.ApplySnapshot::PlayerId not found:{player.Id}");
                    continue;
                }

                if (player.IsDead) {
                    ObjectPooling.Instance.TryAbandon(Constants.Player, listener.gameObject);
                    playerListeners.Remove(player.Id);
                    continue;
                }

                listener.MoveTo(player.Pos.ToVector2());
                listener.RotateTo(player.MoveDir.ToVector2());

                if (player.PressedAttack) {
                    listener.RotateTo(player.AttackDir.ToVector2());
                    listener.Attack(player.AttackDir.ToVector2());
                }
                
                listener.BeingHit(player.Hp);
            }
        }

        public void FocusCamera() {
            if (myListener == null) {
                Debug.LogError("PlayerManager.FocusCamera::Cannot find my listener object");
                return;
            }

            Cache.CameraController.TargetPlayer = myListener.transform;
        }

        public void ClearListeners() {
            foreach (var playerListener in playerListeners) {
                ObjectPooling.Instance.TryAbandon(Constants.Player, playerListener.Value.gameObject);
            }
            playerListeners.Clear();
            Cache.CameraController.TargetPlayer = null;
            myListener = null;
        }
    }
}
