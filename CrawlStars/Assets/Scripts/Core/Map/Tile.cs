using System;
using UnityEngine;
using Utility;

namespace Core.Map {
    public class Tile : MonoBehaviour {
        public enum TileType {
            Ground, Wall
        }
        
        [SerializeField] public SpriteRenderer spriteRenderer;

        public void Initialize(TileType tileType, float tileScale) {
            SetSprite(tileType);

            float spriteSize = spriteRenderer.sprite.bounds.size.x;
            float scale = tileScale / spriteSize;

            transform.localScale = new Vector3(scale, scale, 1);
        }

        private void SetSprite(TileType tileType) {
            string spriteName;

            switch (tileType) {
                case TileType.Ground:
                    spriteName = "Ground";
                    break;
                case TileType.Wall:
                    spriteName = "Wall";
                    break;
                default:
                    return;
            }

            var sprite = SpriteCacheHelper.Get(spriteName);
            if (sprite == null) {
                Debug.LogError($"Tile.SetSprite::Cannot find sprite {spriteName} / tileType: {tileType}");
                return;
            }
            
            spriteRenderer.sprite = sprite;
        }

        private void OnDisable() {
            spriteRenderer.sprite = null;
        }
    }
}