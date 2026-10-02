using System.Collections.Generic;
using Core.Player;
using UnityEngine;
using Cache = Utility.Cache;

namespace CameraControl {
    public class SpectateManager {
        private static SpectateManager instance;
        public static SpectateManager Instance => instance ??= new SpectateManager();
        
        public List<string> MyTeammateIds { get; private set; } = new List<string>();

        private int curSpectateIdx = 0;
        
        public void Initialize() {
            MyTeammateIds.Clear();
            curSpectateIdx = 0;
        }

        public void FocusCameraToMe() => FocusCamera(PlayerManager.Instance.MyListener);

        private void FocusCamera(PlayerListener listener) {
            if (listener == null) {
                Debug.LogError("PlayerManager.FocusCamera::listener is null");
                return;
            }

            Cache.CameraController.TargetPlayer = listener.transform;
        }
        
        public void SpectateMyTeammate(IReadOnlyList<PlayerData> players) {
            var playerManager = PlayerManager.Instance;

            foreach (var player in players) {
                if (player.IsDead
                    || player.Id == playerManager.MyId 
                    || player.Team != playerManager.MyTeam 
                    || !playerManager.GetListener(player.Id, out var listener)) continue;

                FocusCamera(listener);

                for (int i = 0; i < MyTeammateIds.Count; ++i) {
                    if (MyTeammateIds[i] == player.Id) {
                        curSpectateIdx = i;
                    }
                }
                return;
            }
        }

        public void SwitchSpectatingPlayer() {
            for (int i = 1; i <= MyTeammateIds.Count; ++i) {
                int nextIdx = (curSpectateIdx + i) % MyTeammateIds.Count;
                var nextId = MyTeammateIds[nextIdx];
                if (PlayerManager.Instance.GetListener(nextId, out var listener) && listener != null) {
                    FocusCamera(listener);
                    curSpectateIdx = nextIdx;
                    return;
                }
            }
        }
    }
}
