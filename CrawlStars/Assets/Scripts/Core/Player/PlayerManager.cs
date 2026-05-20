using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utility;

namespace Core.Player {
    public class PlayerManager {
        private static PlayerManager instance;
        public static PlayerManager Instance => instance ??= new PlayerManager();
        
        private Dictionary<int, PlayerListener> playerListeners = new Dictionary<int, PlayerListener>();

        public void Initialize(/* Player infos from server (id, characterType, position) */) {
            var playerListener = ObjectPooling.Instance.Get<PlayerListener>("Player");
            if (playerListener == null) {
                Debug.LogError("PlayerManager.Initialize::Cannot find Player Object");
                return;
            }

            playerListener.Id = 1234;
            playerListener.gameObject.transform.position = Vector3.back;
            playerListeners.TryAdd(playerListener.Id, playerListener);
        }

        public void ClearListeners() {
            foreach (var playerListener in playerListeners) {
                ObjectPooling.Instance.TryAbandon("Player", playerListener.Value.gameObject);
            }
            playerListeners.Clear();
        }

        public void Move(Vector2 pos /* Player move infos from server (id, position) */) {
            // 서버 연동 이후 id로 찾아서 Action하는 것으로 변환
            var player = playerListeners.FirstOrDefault();
            if (player.Value == null) {
                Debug.LogError("PlayerManager.Move::Cannot find Player Object");
            }
            
            player.Value.MoveTo(pos);
        }
        
        public void Look(Vector2 dir /* Player look infos from server (id, direction) */) {
            // 서버 연동 이후 id로 찾아서 Action하는 것으로 변환
            var player = playerListeners.FirstOrDefault();
            if (player.Value == null) {
                Debug.LogError("PlayerManager.Look::Cannot find Player Object");
            }
            
            player.Value.LookAt(dir);
        }
        
        public void Attack(/* Player attack infos from server (id, true) */) {
            // 서버 연동 이후 id로 찾아서 Action하는 것으로 변환
            var player = playerListeners.FirstOrDefault();
            if (player.Value == null) {
                Debug.LogError("PlayerManager.Attack::Cannot find Player Object");
            }
            
            player.Value.Attack();
        }
    }
}