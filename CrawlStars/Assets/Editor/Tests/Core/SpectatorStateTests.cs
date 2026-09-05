using Core.Player;
using NUnit.Framework;

namespace Tests.EditMode.Core {
    public class SpectatorStateTests {
        [Test]
        public void Observe_LiveSelf_FollowsSelfWithoutSpectating() {
            var state = new SpectatorState();

            state.Observe(new[] {
                Player("me", "red", 1),
                Player("ally", "red", 2)
            }, "me");

            Assert.That(state.IsLocalPlayerDead, Is.False);
            Assert.That(state.IsSpectating, Is.False);
            Assert.That(state.TargetPlayerId, Is.EqualTo("me"));
        }

        [Test]
        public void Observe_DeadSelfWithReversedAllies_SelectsLowestSlot() {
            var state = new SpectatorState();

            state.Observe(new[] {
                Player("me", "red", 0, true),
                Player("ally-b", "red", 2),
                Player("ally-a", "red", 1)
            }, "me");

            Assert.That(state.IsLocalPlayerDead, Is.True);
            Assert.That(state.IsSpectating, Is.True);
            Assert.That(state.TargetPlayerId, Is.EqualTo("ally-a"));
        }

        [Test]
        public void Observe_CurrentTargetRemainsAlive_PreservesTargetWhenOrderChanges() {
            var state = new SpectatorState();
            state.Observe(new[] {
                Player("me", "red", 0, true),
                Player("ally-a", "red", 1),
                Player("ally-b", "red", 2)
            }, "me");

            state.Observe(new[] {
                Player("ally-b", "red", 0),
                Player("me", "red", 0, true),
                Player("ally-a", "red", 9)
            }, "me");

            Assert.That(state.TargetPlayerId, Is.EqualTo("ally-a"));
        }

        [Test]
        public void Observe_CurrentTargetDies_SelectsNextEligibleAlly() {
            var state = new SpectatorState();
            state.Observe(new[] {
                Player("me", "red", 0, true),
                Player("ally-a", "red", 1),
                Player("ally-b", "red", 2)
            }, "me");

            state.Observe(new[] {
                Player("me", "red", 0, true),
                Player("ally-a", "red", 1, true),
                Player("ally-b", "red", 2)
            }, "me");

            Assert.That(state.TargetPlayerId, Is.EqualTo("ally-b"));
            Assert.That(state.IsSpectating, Is.True);
        }

        [Test]
        public void Observe_NoSurvivingAllies_ClearsTargetAndKeepsDeathGuard() {
            var state = new SpectatorState();

            state.Observe(new[] {
                Player("me", "red", 0, true),
                Player("ally", "red", 1, true),
                Player("enemy", "blue", 0)
            }, "me");

            Assert.That(state.TargetPlayerId, Is.Null);
            Assert.That(state.IsSpectating, Is.False);
            Assert.That(state.IsLocalPlayerDead, Is.True);
        }

        [Test]
        public void Reset_AfterDeath_ClearsStateForNextMatch() {
            var state = new SpectatorState();
            state.Observe(new[] {
                Player("me", "red", 0, true),
                Player("ally", "red", 1)
            }, "me");

            state.Reset();

            Assert.That(state.IsLocalPlayerDead, Is.False);
            Assert.That(state.IsSpectating, Is.False);
            Assert.That(state.TargetPlayerId, Is.Null);

            state.Observe(new[] { Player("me", "blue", 3) }, "me");
            Assert.That(state.TargetPlayerId, Is.EqualTo("me"));
            Assert.That(state.IsLocalPlayerDead, Is.False);
        }

        [Test]
        public void Observe_NullEntries_AreIgnoredAndOrdinalIdBreaksSlotTie() {
            var state = new SpectatorState();

            state.Observe(new PlayerData[] {
                null,
                Player("me", "red", 0, true),
                Player(null, "red", 0),
                Player("ally-b", "red", 1),
                Player("ally-a", "red", 1)
            }, "me");

            Assert.That(state.TargetPlayerId, Is.EqualTo("ally-a"));
        }

        [Test]
        public void Observe_MissingSelf_EnablesDeathGuardWithoutGuessingTeam() {
            var state = new SpectatorState();

            state.Observe(new[] {
                Player("red-player", "red", 0),
                Player("blue-player", "blue", 0)
            }, "missing");

            Assert.That(state.IsLocalPlayerDead, Is.True);
            Assert.That(state.IsSpectating, Is.False);
            Assert.That(state.TargetPlayerId, Is.Null);
        }

        [Test]
        public void Observe_NullSnapshot_EnablesDeathGuardAndClearsTarget() {
            var state = new SpectatorState();

            state.Observe(null, "me");

            Assert.That(state.IsLocalPlayerDead, Is.True);
            Assert.That(state.IsSpectating, Is.False);
            Assert.That(state.TargetPlayerId, Is.Null);
        }

        [Test]
        public void Observe_EnemiesAreExcludedFromSpectating() {
            var state = new SpectatorState();

            state.Observe(new[] {
                Player("me", "red", 0, true),
                Player("enemy", "blue", 0),
                Player("ally", "red", 4)
            }, "me");

            Assert.That(state.TargetPlayerId, Is.EqualTo("ally"));
        }

        [Test]
        public void Observe_AfterLocalDeath_DoesNotReactivateFromStaleAliveSnapshot() {
            var state = new SpectatorState();
            state.Observe(new[] {
                Player("me", "red", 0, true),
                Player("ally", "red", 1)
            }, "me");

            state.Observe(new[] {
                Player("me", "red", 0),
                Player("ally", "red", 1)
            }, "me");

            Assert.That(state.IsLocalPlayerDead, Is.True);
            Assert.That(state.IsSpectating, Is.True);
            Assert.That(state.TargetPlayerId, Is.EqualTo("ally"));
        }

        private static PlayerData Player(string id, string team, int slot, bool isDead = false) => new PlayerData {
            Id = id,
            Team = team,
            Slot = slot,
            IsDead = isDead
        };
    }
}
