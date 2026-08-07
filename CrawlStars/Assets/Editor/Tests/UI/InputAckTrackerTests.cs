using NUnit.Framework;

namespace Tests.EditMode.UI {
    public class InputAckTrackerTests {
        [Test]
        public void TryAcknowledge_ExactTick_ReturnsMeasuredLatency() {
            var tracker = new InputAckTracker();
            tracker.TryRecord(1, 10.0);

            var acknowledged = tracker.TryAcknowledge(1, 10.125, out var latencyMs);

            Assert.That(acknowledged, Is.True);
            Assert.That(latencyMs, Is.EqualTo(125.0).Within(0.0001));
            Assert.That(tracker.PendingCount, Is.Zero);
        }

        [Test]
        public void TryAcknowledge_JumpedTick_DoesNotUseSupersededInputAsLatencySample() {
            var tracker = new InputAckTracker();
            tracker.TryRecord(1, 10.0);
            tracker.TryRecord(2, 10.01);

            var acknowledged = tracker.TryAcknowledge(3, 10.1, out _);

            Assert.That(acknowledged, Is.False);
            Assert.That(tracker.PendingCount, Is.Zero);
            Assert.That(tracker.TimedOutCount, Is.Zero);
        }

        [Test]
        public void TryAcknowledge_JumpedToRecordedTick_UsesOnlyExactTickLatency() {
            var tracker = new InputAckTracker();
            tracker.TryRecord(1, 10.0);
            tracker.TryRecord(2, 10.02);

            var acknowledged = tracker.TryAcknowledge(2, 10.12, out var latencyMs);

            Assert.That(acknowledged, Is.True);
            Assert.That(latencyMs, Is.EqualTo(100.0).Within(0.0001));
            Assert.That(tracker.PendingCount, Is.Zero);
        }

        [Test]
        public void Expire_CountsOnlyInputsPastTimeout() {
            var tracker = new InputAckTracker();
            tracker.TryRecord(1, 10.0);
            tracker.TryRecord(2, 11.0);

            var expiredCount = tracker.CheckExpiration(13.0);

            Assert.That(expiredCount, Is.EqualTo(1));
            Assert.That(tracker.TimedOutCount, Is.EqualTo(1));
            Assert.That(tracker.PendingCount, Is.EqualTo(1));
            Assert.That(tracker.TimeoutRate, Is.EqualTo(0.5));
        }

        [Test]
        public void Record_RejectsNonPositiveAndNonIncreasingTicks() {
            var tracker = new InputAckTracker();

            Assert.That(tracker.TryRecord(0, 10.0), Is.False);
            Assert.That(tracker.TryRecord(1, 10.1), Is.True);
            Assert.That(tracker.TryRecord(1, 10.2), Is.False);

            Assert.That(tracker.SubmittedCount, Is.EqualTo(1));
            Assert.That(tracker.PendingCount, Is.EqualTo(1));
        }
    }
}
