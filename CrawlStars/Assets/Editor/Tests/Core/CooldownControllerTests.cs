using Core.Player;
using NUnit.Framework;

namespace Tests.EditMode.Core {
    public class CooldownControllerTests {
        [Test]
        public void Constructor_StartsWithFullNormalAndSkillCharges() {
            var cooldown = new CooldownController(3, 2f, 5f);

            Assert.That(cooldown.MaxCharges, Is.EqualTo(3));
            Assert.That(cooldown.CurrentCharges, Is.EqualTo(3));
            Assert.That(cooldown.NormalProgress, Is.EqualTo(1f));
            Assert.That(cooldown.IsSkillCharged, Is.True);
            Assert.That(cooldown.SkillProgress, Is.EqualTo(1f));
        }

        [Test]
        public void TryNormalAttack_ConsumesChargesAndStopsAtZero() {
            var cooldown = new CooldownController(2, 1f, 1f);

            Assert.That(cooldown.TryNormalAttack(), Is.True);
            Assert.That(cooldown.TryNormalAttack(), Is.True);
            Assert.That(cooldown.TryNormalAttack(), Is.False);
            Assert.That(cooldown.CurrentCharges, Is.Zero);
        }

        [Test]
        public void Tick_RecoversChargeAtCooldownBoundary() {
            var cooldown = new CooldownController(2, 2f, 1f);
            cooldown.TryNormalAttack();

            cooldown.Tick(1.99f);
            Assert.That(cooldown.CurrentCharges, Is.EqualTo(1));
            Assert.That(cooldown.NormalProgress, Is.EqualTo(0.995f).Within(0.0001f));

            cooldown.Tick(0.01f);
            Assert.That(cooldown.CurrentCharges, Is.EqualTo(2));
            Assert.That(cooldown.NormalProgress, Is.EqualTo(1f));
        }

        [Test]
        public void Tick_LargeDeltaTime_RecoversMultipleChargesWithoutExceedingMax() {
            var cooldown = new CooldownController(3, 1f, 1f);
            cooldown.TryNormalAttack();
            cooldown.TryNormalAttack();
            cooldown.TryNormalAttack();

            cooldown.Tick(10f);

            Assert.That(cooldown.CurrentCharges, Is.EqualTo(3));
            Assert.That(cooldown.NormalProgress, Is.EqualTo(1f));
        }

        [Test]
        public void TrySkillAttack_RemainsUnavailableUntilCooldownCompletes() {
            var cooldown = new CooldownController(1, 1f, 3f);

            Assert.That(cooldown.TrySkillAttack(), Is.True);
            Assert.That(cooldown.TrySkillAttack(), Is.False);

            cooldown.Tick(2f);
            Assert.That(cooldown.IsSkillCharged, Is.False);
            Assert.That(cooldown.SkillProgress, Is.EqualTo(2f / 3f).Within(0.0001f));

            cooldown.Tick(1f);
            Assert.That(cooldown.IsSkillCharged, Is.True);
            Assert.That(cooldown.TrySkillAttack(), Is.True);
        }

        [Test]
        public void Constructor_ClampsInvalidConfigurationToMinimums() {
            var cooldown = new CooldownController(0, 0f, -10f);
            cooldown.TryNormalAttack();
            cooldown.TrySkillAttack();

            cooldown.Tick(0.5f);

            Assert.That(cooldown.MaxCharges, Is.EqualTo(1));
            Assert.That(cooldown.CurrentCharges, Is.Zero);
            Assert.That(cooldown.NormalProgress, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(cooldown.SkillProgress, Is.EqualTo(0.5f).Within(0.0001f));
        }
    }
}
