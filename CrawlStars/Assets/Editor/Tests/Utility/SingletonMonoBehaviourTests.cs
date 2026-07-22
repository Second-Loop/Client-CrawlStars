using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.EditMode.Utility {
    public class SingletonMonoBehaviourTests {
        private static readonly FieldInfo SingletonInstanceField = typeof(SingletonMonoBehaviour<TestSingleton>).GetField(
            "instance",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        private GameObject firstRoot;
        private GameObject secondRoot;

        [SetUp]
        public void SetUp() {
            SingletonInstanceField?.SetValue(null, null);
        }

        [TearDown]
        public void TearDown() {
            SingletonInstanceField?.SetValue(null, null);
            if (firstRoot != null) Object.DestroyImmediate(firstRoot);
            if (secondRoot != null) Object.DestroyImmediate(secondRoot);
        }

        [Test]
        public void Register_FirstInstance_BecomesSingleton() {
            TestSingleton first = CreateSingleton(ref firstRoot, "FirstSingleton");

            first.Register();

            Assert.That(TestSingleton.Instance, Is.SameAs(first));
        }

        [Test]
        public void Register_SecondInstance_KeepsFirstInstance() {
            TestSingleton first = CreateSingleton(ref firstRoot, "FirstSingleton");
            TestSingleton second = CreateSingleton(ref secondRoot, "SecondSingleton");
            first.Register();
            LogAssert.Expect(LogType.Error, new Regex("TestSingleton Instance.*SecondSingleton"));

            second.Register();

            Assert.That(TestSingleton.Instance, Is.SameAs(first));
        }

        private static TestSingleton CreateSingleton(ref GameObject root, string name) {
            root = new GameObject(name);
            return root.AddComponent<TestSingleton>();
        }

        public sealed class TestSingleton : SingletonMonoBehaviour<TestSingleton> {
            protected override void Awake() {
                // Registration is invoked explicitly so EditMode lifecycle timing does not affect the test.
            }

            public void Register() {
                base.Awake();
            }
        }
    }
}
