using System.Collections.Generic;
using UnityEngine;

namespace Utility {
    public static class SpriteCacheHelper {
        private static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

        public static Sprite Get(string name) {
            if (string.IsNullOrWhiteSpace(name)) {
                Debug.LogError("SpriteCacheHelper.Get::invalid sprite name.");
                return null;
            }

            if (sprites.TryGetValue(name, out var sprite)) return sprite;

            var loadedSprite = Resources.Load<Sprite>(name);
            if (loadedSprite == null) {
                Debug.LogError($"SpriteCacheHelper.Get::{name}을 찾을 수 없습니다.");
                return null;
            }

            sprites.Add(name, loadedSprite);
            return loadedSprite;
        }

        public static void Clear() {
            foreach (var sprite in sprites) {
                if (sprite.Value != null) {
                    Resources.UnloadAsset(sprite.Value);
                }
            }
            sprites.Clear();
        }
    }
}
