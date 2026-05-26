using System;
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

        public void Initialize(/* Player infos from server (id, characterType, position) */) {
            var playerListener = ObjectPooling.Instance.Get<PlayerListener>("Player");
            if (playerListener == null) {
                Debug.LogError("PlayerManager.Initialize::Cannot find Player Object");
                return;
            }

            playerListener.Id = Guid.NewGuid().ToString();
            playerListener.transform.position = Vector3.back;
            playerListeners.TryAdd(playerListener.Id, playerListener);
            CommonCache.MainCamera.GetComponent<CameraController>().TargetPlayer = playerListener.transform;
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
        
        public void Rotate(Vector2 dir /* Player look infos from server (id, direction) */) {
            // 서버 연동 이후 id로 찾아서 Action하는 것으로 변환
            var player = playerListeners.FirstOrDefault();
            if (player.Value == null) {
                Debug.LogError("PlayerManager.Look::Cannot find Player Object");
            }
            
            player.Value.RotateTo(dir);
        }
        
        public void Attack(Vector2 pos, Vector2 dir /* Player attack infos from server */) {
            // 서버 연동 이후 id로 찾아서 Action하는 것으로 변환
            var player = playerListeners.FirstOrDefault();
            if (player.Value == null) {
                Debug.LogError("PlayerManager.Attack::Cannot find Player Object");
            }

            player.Value.RotateTo(dir);
            player.Value.Attack(dir);
        }
    }
}