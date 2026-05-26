using CameraControl;
using UnityEngine;

namespace Utility {
    public static class CommonCache {
        private static Camera mainCamera;
        public static Camera MainCamera => mainCamera ??= Camera.main;

        private static CameraController cameraController;
        public static CameraController CameraController => cameraController ??= MainCamera.GetComponent<CameraController>();

        public static void OnChangeScene() {
            mainCamera = null;
            cameraController = null;
        }
    }
}