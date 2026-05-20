using Core.Map;
using UnityEngine;

namespace Core.Simulator {
    public static class Physics {
        public static Vector2 GetNextPosition(Vector2 origin, Vector2 movement, float radius = 0.5f) {
            Vector2 nextX = origin + new Vector2(movement.x, 0f);
            bool isHorizontalBlocked = IsCircleBlocked(nextX, radius);
            if (!isHorizontalBlocked) {
                origin = nextX;
            }

            Vector2 nextY = origin + new Vector2(0f, movement.y);
            bool isVerticalBlocked = IsCircleBlocked(nextY, radius);
            if (!isVerticalBlocked) {
                origin = nextY;
            }

            Debug.Log($"isHorizontalBlocked: {isHorizontalBlocked} /  isVerticalBlocked: {isVerticalBlocked}");
            return origin;
        }
        
        private static bool IsCircleBlocked(Vector2 center, float radius) 
            => IsWallAt(center)
               || IsWallAt(center + Vector2.up * radius) 
               || IsWallAt(center + Vector2.down * radius) 
               || IsWallAt(center + Vector2.left * radius) 
               || IsWallAt(center + Vector2.right * radius);

        private static bool IsWallAt(Vector2 worldPos) {
            var mapData = MapLoader.CachedMapData;
            if (mapData == null) return true;

            Vector2 mapStartPos = MapRenderer.GetMapStartPos(mapData);
            Vector2 local = worldPos - mapStartPos;

            int x = Mathf.RoundToInt(local.x / MapRenderer.TileScale);
            int y = Mathf.RoundToInt(-local.y / MapRenderer.TileScale);

            if (x < 0 || x >= mapData.width) return true;
            if (y < 0 || y >= mapData.height) return true;

            return mapData.map[y][x] == 1;
        }
    }
}