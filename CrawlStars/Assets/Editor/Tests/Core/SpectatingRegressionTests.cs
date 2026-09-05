using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using Core.Inputs;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Utility;

namespace Tests.EditMode.Core {
    public class InputProviderDeactivationTests {
        private GameObject root;
        private InputProvider inputProvider;

        [SetUp]
        public void SetUp() {
            root = new GameObject("InputProviderDeactivationTests");
            inputProvider = root.AddComponent<InputProvider>();
        }

        [TearDown]
        public void TearDown() {
            if (root != null) UnityEngine.Object.DestroyImmediate(root);
        }

        [Test]
        public void Deactivate_DuringSkillAim_ClearsAllTransientInputForNextMatch() {
            inputProvider.IsActivated = true;
            SetAutoProperty("AimDirection", Vector2.right);
            SetField("attackDirection", Vector2.up);
            SetAutoProperty("UsedSkill", true);
            SetEnumField("currentAimButton", "Right");

            inputProvider.IsActivated = false;

            Assert.That(inputProvider.AimDirection, Is.EqualTo(Vector2.zero));
            Assert.That(inputProvider.CaptureAttackDirection(), Is.EqualTo(Vector2.zero));
            Assert.That(inputProvider.UsedSkill, Is.False);
            Assert.That(GetField("currentAimButton").ToString(), Is.EqualTo("None"));
        }

        private object GetField(string name) => typeof(InputProvider)
            .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(inputProvider);

        private void SetField(string name, object value) => typeof(InputProvider)
            .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(inputProvider, value);

        private void SetAutoProperty(string name, object value) => SetField($"<{name}>k__BackingField", value);

        private void SetEnumField(string name, string value) {
            FieldInfo field = typeof(InputProvider).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            field?.SetValue(inputProvider, Enum.Parse(field.FieldType, value));
        }
    }

    public class ObjectPoolingVisibilityTests {
        private static readonly FieldInfo ObjectPoolField = typeof(ObjectPooling).GetField(
            "objectPool",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        private GameObject root;
        private ObjectPooling pooling;

        [SetUp]
        public void SetUp() {
            ClearPool();
            root = new GameObject("ObjectPoolingVisibilityTests");
            pooling = root.AddComponent<ObjectPooling>();
        }

        [TearDown]
        public void TearDown() {
            ClearPool();
            if (root != null) UnityEngine.Object.DestroyImmediate(root);
        }

        [Test]
        public void TryAbandon_HiddenCheckedOutObject_ReturnsItToPoolAndGetReusesIt() {
            GameObject hidden = pooling.Get(Constants.Tile);
            hidden.SetActive(false);

            bool abandoned = pooling.TryAbandon(Constants.Tile, hidden);
            GameObject reused = pooling.Get(Constants.Tile);

            Assert.That(abandoned, Is.True);
            Assert.That(reused, Is.SameAs(hidden));
            Assert.That(reused.activeSelf, Is.True);
        }

        [Test]
        public void TryAbandon_SameObjectTwice_RejectsDuplicateAndQueuesOnlyOnce() {
            GameObject instance = pooling.Get(Constants.Tile);
            Assert.That(pooling.TryAbandon(Constants.Tile, instance), Is.True);
            LogAssert.Expect(LogType.Error, new Regex("object is already abandoned"));

            bool duplicate = pooling.TryAbandon(Constants.Tile, instance);
            GameObject reused = pooling.Get(Constants.Tile);
            GameObject next = pooling.Get(Constants.Tile);

            Assert.That(duplicate, Is.False);
            Assert.That(reused, Is.SameAs(instance));
            Assert.That(next, Is.Not.SameAs(instance));
        }

        private static void ClearPool() {
            var dictionary = ObjectPoolField?.GetValue(null) as IDictionary;
            dictionary?.Clear();
        }
    }
}
