using System;

namespace Core.Player {
    public sealed class AuthoritativeCombatState : IAttackCooldownSource {
        private const float ServerTickRate = 30f;

        private readonly float rechargeSeconds;
        private readonly float skillCooldownSeconds;
        private bool hasSnapshot;
        private long lastSnapshotTick;

        public int CurrentCharges { get; private set; }
        public int MaxCharges { get; }
        public float NormalProgress { get; private set; }
        public float SkillProgress { get; private set; }
        public bool IsSkillCharged => CanSkillAttack;
        public bool CanNormalAttack { get; private set; }
        public bool CanSkillAttack { get; private set; }

        public AuthoritativeCombatState(int maxCharges, float rechargeSeconds, float skillCooldownSeconds) {
            MaxCharges = Math.Max(1, maxCharges);
            this.rechargeSeconds = Math.Max(1f, rechargeSeconds);
            this.skillCooldownSeconds = Math.Max(1f, skillCooldownSeconds);
        }

        public void Observe(long snapshotTick, PlayerData player) {
            if (hasSnapshot && snapshotTick <= lastSnapshotTick) return;

            hasSnapshot = true;
            lastSnapshotTick = snapshotTick;

            if (player == null) {
                BlockUnavailableState();
                return;
            }

            CurrentCharges = Clamp(player.AttackCharges, 0, MaxCharges);
            long nextInputTick = snapshotTick == long.MaxValue ? long.MaxValue : snapshotTick + 1;
            CanNormalAttack = !player.IsDead
                && CurrentCharges > 0
                && (player.AttackReadyTick == 0 || nextInputTick >= player.AttackReadyTick);
            CanSkillAttack = !player.IsDead
                && (player.SkillReadyTick == 0 || nextInputTick >= player.SkillReadyTick);

            NormalProgress = CalculateProgress(
                snapshotTick,
                player.NextAttackChargeTick,
                rechargeSeconds,
                CurrentCharges >= MaxCharges
            );
            SkillProgress = CanSkillAttack
                ? 1f
                : CalculateProgress(snapshotTick, player.SkillReadyTick, skillCooldownSeconds, false);
        }

        public void Tick(float deltaSeconds) {
            if (!hasSnapshot || deltaSeconds <= 0f) return;

            if (CurrentCharges < MaxCharges) {
                NormalProgress = Clamp01(NormalProgress + deltaSeconds / rechargeSeconds);
            }

            if (!CanSkillAttack) {
                SkillProgress = Clamp01(SkillProgress + deltaSeconds / skillCooldownSeconds);
            }
        }

        public void Reset() {
            hasSnapshot = false;
            lastSnapshotTick = 0;
            BlockUnavailableState();
        }

        private void BlockUnavailableState() {
            CurrentCharges = 0;
            NormalProgress = 0f;
            SkillProgress = 0f;
            CanNormalAttack = false;
            CanSkillAttack = false;
        }

        private static float CalculateProgress(long snapshotTick, long readyTick, float durationSeconds, bool full) {
            if (full || readyTick == 0 || readyTick <= snapshotTick) return 1f;

            double durationTicks = durationSeconds * ServerTickRate;
            double remainingTicks = (double)readyTick - snapshotTick;
            return Clamp01((float)(1d - remainingTicks / durationTicks));
        }

        private static int Clamp(int value, int minimum, int maximum) {
            if (value < minimum) return minimum;
            return value > maximum ? maximum : value;
        }

        private static float Clamp01(float value) {
            if (value < 0f) return 0f;
            return value > 1f ? 1f : value;
        }
    }
}
