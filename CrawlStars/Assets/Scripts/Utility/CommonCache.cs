using UnityEngine;

namespace Utility {
    public static class CommonCache {
        private static Camera mainCamera;
        public static Camera MainCamera => mainCamera ??= Camera.main;
    }
}