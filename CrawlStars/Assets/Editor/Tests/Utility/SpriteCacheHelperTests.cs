using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Utility;

namespace Tests.EditMode.Utility {
    public class SpriteCacheHelperTests {
        [TearDown]
        public void TearDown() {
            SpriteCacheHelper.Clear();
        }

        [Test]
        public void Get_ExistingSprite_ReturnsCachedInstance() {
            Sprite first = SpriteCacheHelper.Get("Ground");
            Sprite second = SpriteCacheHelper.Get("Ground");

            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.SameAs(first));
        }

        [Test]
        public void Get_MissingSprite_ReturnsNullAndLogsError() {
            LogAssert.Expect(LogType.Error, new Regex("SpriteCacheHelper.Get::missing-test-sprite"));

            Sprite sprite = SpriteCacheHelper.Get("missing-test-sprite");

            Assert.That(sprite, Is.Null);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Get_InvalidName_ReturnsNullAndLogsError(string name) {
            LogAssert.Expect(LogType.Error, "SpriteCacheHelper.Get::invalid sprite name.");

            Sprite sprite = SpriteCacheHelper.Get(name);

            Assert.That(sprite, Is.Null);
        }

        [Test]
        public void Clear_AllowsSpriteToBeLoadedAgain() {
            Assert.That(SpriteCacheHelper.Get("Ground"), Is.Not.Null);

            SpriteCacheHelper.Clear();

            Assert.That(SpriteCacheHelper.Get("Ground"), Is.Not.Null);
        }
    }
}
