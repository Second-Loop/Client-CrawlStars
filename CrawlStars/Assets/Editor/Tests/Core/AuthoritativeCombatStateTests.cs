using Core.Player;
using NUnit.Framework;

namespace Tests.EditMode.Core {
    public class AuthoritativeCombatStateTests {
        [Test]
        public void BeforeFirstSnapshot_BlocksAttacksAndShowsNoAvailableResources() {
            var state = CreateState();

            Assert.That(state.CurrentCharges, Is.Zero);
            Assert.That(state.CanNormalAttack, Is.False);
            Assert.That(state.CanSkillAttack, Is.False);
            Assert.That(state.IsSkillCharged, Is.False);
            Assert.That(state.NormalProgress, Is.Zero);
            Assert.That(state.SkillProgress, Is.Zero);
        }

        [Test]
        public void SkillApproval_UsesReadyTickWithoutSpendingNormalCharges() {
            var state = CreateState();

            state.Observe(1, Player(charges: 2, nextChargeTick: 31, skillReadyTick: 361, pressedSkill: true));

            Assert.That(state.CurrentCharges, Is.EqualTo(2));
            Assert.That(state.CanSkillAttack, Is.False);
            Assert.That(state.SkillProgress, Is.Zero);
            state.Tick(20f);
            Assert.That(state.CurrentCharges, Is.EqualTo(2));
            Assert.That(state.CanSkillAttack, Is.False);
            state.Observe(360, Player(charges: 3, skillReadyTick: 361));
            Assert.That(state.CanSkillAttack, Is.True);
            Assert.That(state.IsSkillCharged, Is.True);
        }

        [Test]
        public void RejectedEarlySkillRetry_DoesNotRestartDisplayProgress() {
            var state = CreateState();
            state.Observe(1, Player(charges: 3, skillReadyTick: 361, pressedSkill: true));
            state.Tick(2f);

            state.Observe(61, Player(charges: 3, skillReadyTick: 361));

            Assert.That(state.SkillProgress, Is.EqualTo(1f / 6f).Within(0.0001f));
            Assert.That(state.CanSkillAttack, Is.False);
        }

        [Test]
        public void NewerSnapshot_CorrectsChargesAndRechargeProgress() {
            var state = CreateState();
            state.Observe(10, Player(charges: 1, nextChargeTick: 40));
            state.Tick(0.75f);
            Assert.That(state.NormalProgress, Is.EqualTo(0.75f).Within(0.0001f));

            state.Observe(20, Player(charges: 2, nextChargeTick: 35));

            Assert.That(state.CurrentCharges, Is.EqualTo(2));
            Assert.That(state.NormalProgress, Is.EqualTo(0.5f).Within(0.0001f));
        }

        [Test]
        public void ReloadSkillSnapshot_ImmediatelyShowsServerGrantedMaximumCharges() {
            var state = CreateState();
            state.Observe(1, Player(charges: 0, nextChargeTick: 31));

            state.Observe(2, Player(charges: 3, pressedSkill: true, skillReadyTick: 362));

            Assert.That(state.CurrentCharges, Is.EqualTo(3));
            Assert.That(state.NormalProgress, Is.EqualTo(1f));
        }

        [Test]
        public void BurstLock_BlocksNormalAttackEvenWithPositiveCharges() {
            var state = CreateState();

            state.Observe(50, Player(charges: 2, attackReadyTick: 52));

            Assert.That(state.CanNormalAttack, Is.False);
        }

        [Test]
        public void NextInputTickAtAttackBoundary_AllowsNormalAttack() {
            var state = CreateState();

            state.Observe(51, Player(charges: 2, attackReadyTick: 52));

            Assert.That(state.CanNormalAttack, Is.True);
        }

        [Test]
        public void LongMaxSnapshot_UsesSaturatingNextTickForReadyChecks() {
            var state = CreateState();

            state.Observe(long.MaxValue, Player(charges: 1, attackReadyTick: long.MaxValue, skillReadyTick: long.MaxValue));

            Assert.That(state.CanNormalAttack, Is.True);
            Assert.That(state.CanSkillAttack, Is.True);
        }

        [Test]
        public void DeadPlayer_BlocksBothAttackKinds() {
            var state = CreateState();

            state.Observe(10, Player(charges: 3, isDead: true));

            Assert.That(state.CanNormalAttack, Is.False);
            Assert.That(state.CanSkillAttack, Is.False);
        }

        [Test]
        public void StaleAndDuplicateSnapshots_DoNotReplaceNewerState() {
            var state = CreateState();
            state.Observe(10, Player(charges: 2));

            state.Observe(10, Player(charges: 0, isDead: true));
            state.Observe(9, Player(charges: 0, isDead: true));

            Assert.That(state.CurrentCharges, Is.EqualTo(2));
            Assert.That(state.CanNormalAttack, Is.True);
        }

        [Test]
        public void Reset_DiscardsSnapshotAndBlocksAttacks() {
            var state = CreateState();
            state.Observe(10, Player(charges: 3));

            state.Reset();

            Assert.That(state.CurrentCharges, Is.Zero);
            Assert.That(state.CanNormalAttack, Is.False);
            Assert.That(state.CanSkillAttack, Is.False);
            Assert.That(state.NormalProgress, Is.Zero);
            Assert.That(state.SkillProgress, Is.Zero);
        }

        [Test]
        public void HugeDisplayDelta_NeverCreatesChargesOrAttackPermission() {
            var state = CreateState();
            state.Observe(1, Player(charges: 0, nextChargeTick: 31, skillReadyTick: 361));

            state.Tick(1000f);

            Assert.That(state.CurrentCharges, Is.Zero);
            Assert.That(state.NormalProgress, Is.EqualTo(1f));
            Assert.That(state.SkillProgress, Is.EqualTo(1f));
            Assert.That(state.CanNormalAttack, Is.False);
            Assert.That(state.CanSkillAttack, Is.False);
        }

        private static AuthoritativeCombatState CreateState() => new AuthoritativeCombatState(3, 1f, 12f);

        private static PlayerData Player(
            int charges,
            long nextChargeTick = 0,
            long skillReadyTick = 0,
            long attackReadyTick = 0,
            bool pressedSkill = false,
            bool isDead = false
        ) => new PlayerData {
            AttackCharges = charges,
            NextAttackChargeTick = nextChargeTick,
            SkillReadyTick = skillReadyTick,
            AttackReadyTick = attackReadyTick,
            PressedSkill = pressedSkill,
            IsDead = isDead
        };
    }
}
