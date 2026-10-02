using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Core.Projectile;
using Network;
using NUnit.Framework;
using UnityEngine;
using Utility;

namespace Tests.EditMode.Core {
    public class ProjectileManagerTests {
        private static readonly FieldInfo ObjectPoolField = typeof(ObjectPooling).GetField(
            "objectPool",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        private static readonly FieldInfo SingletonInstanceField = typeof(SingletonMonoBehaviour<ObjectPooling>).GetField(
            "instance",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        private readonly List<GameObject> roots = new List<GameObject>();
        private ProjectileManager projectileManager;

        [SetUp]
        public void SetUp() {
            ClearObjectPoolingStaticState();
            var root = Track(new GameObject("ProjectileManagerTests"));
            var pooling = root.AddComponent<ObjectPooling>();
            SingletonInstanceField?.SetValue(null, pooling);
            projectileManager = new ProjectileManager();
        }

        [TearDown]
        public void TearDown() {
            ClearObjectPoolingStaticState();
            foreach (var root in roots) {
                if (root != null) UnityEngine.Object.DestroyImmediate(root);
            }
            roots.Clear();
        }

        private GameObject Track(GameObject gameObject) {
            roots.Add(gameObject);
            return gameObject;
        }

        private static ProjectileData CreateProjectile(string id, Vector2 position) => new ProjectileData {
            Id = id,
            Pos = new Vector2Dto(position),
            Dir = new Vector2Dto(Vector2.right)
        };

        private static void ClearObjectPoolingStaticState() {
            var dictionary = ObjectPoolField?.GetValue(null) as IDictionary;
            dictionary?.Clear();
            SingletonInstanceField?.SetValue(null, null);
        }
    }
}
