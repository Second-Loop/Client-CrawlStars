using System.Collections.Generic;
using CameraControl;
using NUnit.Framework;
using UnityEngine;
using CameraCache = Utility.Cache;

namespace Tests.EditMode.Utility {
    public class CacheTests {
        private readonly List<GameObject> createdObjects = new List<GameObject>();
        private readonly List<GameObject> existingMainCameraObjects = new List<GameObject>();

        [SetUp]
        public void SetUp() {
            CameraCache.OnChangeScene();
            existingMainCameraObjects.AddRange(GameObject.FindGameObjectsWithTag("MainCamera"));
            foreach (GameObject existing in existingMainCameraObjects) {
                existing.tag = "Untagged";
            }
        }

        [TearDown]
        public void TearDown() {
            CameraCache.OnChangeScene();
            foreach (GameObject created in createdObjects) {
                if (created != null) Object.DestroyImmediate(created);
            }
            createdObjects.Clear();

            foreach (GameObject existing in existingMainCameraObjects) {
                if (existing != null) existing.tag = "MainCamera";
            }
            existingMainCameraObjects.Clear();
        }

        [Test]
        public void MainCamera_ReturnsCachedCameraUntilSceneChanges() {
            Camera first = CreateMainCamera("FirstMainCamera");
            Assert.That(CameraCache.MainCamera, Is.SameAs(first));

            first.tag = "Untagged";
            Camera second = CreateMainCamera("SecondMainCamera");

            Assert.That(CameraCache.MainCamera, Is.SameAs(first));
            CameraCache.OnChangeScene();
            Assert.That(CameraCache.MainCamera, Is.SameAs(second));
        }

        [Test]
        public void CameraController_ReturnsComponentFromMainCamera() {
            Camera camera = CreateMainCamera("MainCameraWithController");
            CameraController controller = camera.gameObject.AddComponent<CameraController>();

            Assert.That(CameraCache.CameraController, Is.SameAs(controller));
            Assert.That(CameraCache.CameraController, Is.SameAs(controller));
        }

        [Test]
        public void OnChangeScene_RefreshesCameraController() {
            Camera first = CreateMainCamera("FirstMainCamera");
            CameraController firstController = first.gameObject.AddComponent<CameraController>();
            Assert.That(CameraCache.CameraController, Is.SameAs(firstController));

            first.tag = "Untagged";
            Camera second = CreateMainCamera("SecondMainCamera");
            CameraController secondController = second.gameObject.AddComponent<CameraController>();
            CameraCache.OnChangeScene();

            Assert.That(CameraCache.CameraController, Is.SameAs(secondController));
        }

        private Camera CreateMainCamera(string name) {
            var gameObject = new GameObject(name);
            createdObjects.Add(gameObject);
            gameObject.tag = "MainCamera";
            return gameObject.AddComponent<Camera>();
        }
    }
}
