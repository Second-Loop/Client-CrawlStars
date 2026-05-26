using System.Collections.Generic;
using System.Linq;
using CameraControl;
using UnityEngine;
using Utility;

namespace Core.Player {
    public class PlayerManager {
        private static PlayerManager instance;
        public static PlayerManager Instance => instance ??= new PlayerManager();

        private Dictionary<string, PlayerListener> playerListeners = new Dictionary<string, PlayerListener>();
        public string MyId { get; private set; }

        public void Initialize(List<PlayerData> players) {
            for (int i = 0; i < players.Count; ++i) {
                var playerListener = ObjectPooling.Instance.Get<PlayerListener>("Player");
                if (playerListener == null) {
                    Debug.LogError("PlayerManager.Initialize::Cannot find Player Object");
                    return;
                }

                var id = players[i].Id;
                playerListener.transform.position = (Vector3)players[i].Pos + Vector3.back;
                playerListeners.TryAdd(id, playerListener);

                if (i == 0) {
                    MyId = id;
                    CommonCache.MainCamera.GetComponent<CameraController>().TargetPlayer = playerListener.transform;
                }
            }
        }

        public void ClearListeners() {
            foreach (var playerListener in playerListeners) {
                ObjectPooling.Instance.TryAbandon("Player", playerListener.Value.gameObject);
            }
            playerListeners.Clear();
            CommonCache.MainCamera.GetComponent<CameraController>().TargetPlayer = null;
            MyId = null;
        }

        public void Move(List<PlayerData> players) {
            foreach (var player in players) {
                if (!playerListeners.TryGetValue(player.Id, out var listener)) {
                    Debug.LogError("PlayerManager.Move::Cannot find Player Object");
                    continue;
                }
                Debug.Log(player.Pos);
                listener.MoveTo(player.Pos);
                listener.RotateTo(player.Dir);
            }
            Debug.Log("End");
        }
        
        public void Attack(List<PlayerData> players) {
            foreach (var player in players) {
                if (!playerListeners.TryGetValue(player.Id, out var listener)) {
                    Debug.LogError("PlayerManager.Attack::Cannot find Player Object");
                    continue;
                }
                listener.RotateTo(player.Dir);
                listener.Attack(player.Dir);
            }
        }
    }
}