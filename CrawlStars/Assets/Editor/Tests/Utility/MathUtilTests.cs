using NUnit.Framework;
using UnityEngine;
using Utility;

namespace Tests.EditMode.Utility {
    public class MathUtilTests {
        [TestCase(1f, 0f, 0f)]
        [TestCase(0f, 1f, 90f)]
        [TestCase(-1f, 0f, 180f)]
        [TestCase(0f, -1f, -90f)]
        [TestCase(1f, 1f, 45f)]
        [TestCase(-1f, -1f, -135f)]
        public void GetAngle_ReturnsExpectedDegrees(float x, float y, float expected) {
            float angle = MathUtil.GetAngle(new Vector2(x, y));

            Assert.That(angle, Is.EqualTo(expected).Within(0.0001f));
        }

        [Test]
        public void GetAngle_IsIndependentOfDirectionMagnitude() {
            float unitAngle = MathUtil.GetAngle(new Vector2(1f, 2f));
            float scaledAngle = MathUtil.GetAngle(new Vector2(10f, 20f));

            Assert.That(scaledAngle, Is.EqualTo(unitAngle).Within(0.0001f));
        }

        [Test]
        public void GetAngle_ZeroVector_ReturnsZero() {
            Assert.That(MathUtil.GetAngle(Vector2.zero), Is.Zero);
        }
    }
}
