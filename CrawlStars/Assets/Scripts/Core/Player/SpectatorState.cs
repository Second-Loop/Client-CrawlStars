using System.Collections.Generic;

namespace Core.Player {
    public sealed class SpectatorState {
        private string localTeam;

        public bool IsLocalPlayerDead { get; private set; }
        public bool IsSpectating { get; private set; }
        public string TargetPlayerId { get; private set; }

        public void Observe(IReadOnlyList<PlayerData> players, string myId) {
            PlayerData self = FindPlayer(players, myId);
            if (self != null && !string.IsNullOrEmpty(self.Team)) {
                localTeam = self.Team;
            }

            if (!IsLocalPlayerDead && self != null && !self.IsDead) {
                TargetPlayerId = self.Id;
                IsSpectating = false;
                return;
            }

            IsLocalPlayerDead = true;

            PlayerData selected = null;
            for (int i = 0; players != null && i < players.Count; ++i) {
                PlayerData candidate = players[i];
                if (!IsEligibleTeammate(candidate, myId)) continue;

                if (candidate.Id == TargetPlayerId) {
                    selected = candidate;
                    break;
                }

                if (selected == null || ComesBefore(candidate, selected)) {
                    selected = candidate;
                }
            }

            TargetPlayerId = selected?.Id;
            IsSpectating = TargetPlayerId != null;
        }

        public void Reset() {
            localTeam = null;
            IsLocalPlayerDead = false;
            IsSpectating = false;
            TargetPlayerId = null;
        }

        private bool IsEligibleTeammate(PlayerData player, string myId) {
            return player != null
                && !string.IsNullOrEmpty(player.Id)
                && player.Id != myId
                && !player.IsDead
                && !string.IsNullOrEmpty(localTeam)
                && string.Equals(player.Team, localTeam, System.StringComparison.Ordinal);
        }

        private static PlayerData FindPlayer(IReadOnlyList<PlayerData> players, string id) {
            if (players == null || string.IsNullOrEmpty(id)) return null;

            for (int i = 0; i < players.Count; ++i) {
                PlayerData player = players[i];
                if (player != null && string.Equals(player.Id, id, System.StringComparison.Ordinal)) {
                    return player;
                }
            }

            return null;
        }

        private static bool ComesBefore(PlayerData candidate, PlayerData current) {
            if (candidate.Slot != current.Slot) return candidate.Slot < current.Slot;
            return string.CompareOrdinal(candidate.Id, current.Id) < 0;
        }
    }
}
