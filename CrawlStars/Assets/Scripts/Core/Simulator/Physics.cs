using Core.Map;
using UnityEngine;

namespace Core.Simulator {
    public class Physics {
        public Vector2 MoveWithCollision(Vector2 from, Vector2 movement, float radius) {
            Vector2 pos = from;

            Vector2 nextX = pos + new Vector2(movement.x, 0f);
            if (!IsCircleBlocked(nextX, radius)) {
                pos = nextX;
            }

            Vector2 nextY = pos + new Vector2(0f, movement.y);
            if (!IsCircleBlocked(nextY, radius)) {
                pos = nextY;
            }

            return pos;
        }
        
        private bool IsCircleBlocked(Vector2 center, float radius) {
            return IsWallAt(center)
                   || IsWallAt(center + Vector2.up * radius)
                   || IsWallAt(center + Vector2.down * radius)
                   || IsWallAt(center + Vector2.left * radius)
                   || IsWallAt(center + Vector2.right * radius);
        }

        private bool IsWallAt(Vector2 worldPos) {
            var mapData = MapLoader.CachedMapData;
            if (mapData == null) return true;

            float tileScale = MapRenderer.TileScale;

            Vector2 mapStartPos = new Vector2(
                -tileScale * (mapData.width - 1) * 0.5f,
                tileScale * (mapData.height - 1) * 0.5f
            );

            Vector2 local = worldPos - mapStartPos;

            int x = Mathf.RoundToInt(local.x / tileScale);
            int y = Mathf.RoundToInt(-local.y / tileScale);

            if (x < 0 || x >= mapData.width) return true;
            if (y < 0 || y >= mapData.height) return true;

            return mapData.map[y][x] == 1;
        }
    }
}