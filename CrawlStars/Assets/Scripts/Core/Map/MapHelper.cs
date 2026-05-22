using UnityEngine;

namespace Core.Map {
    public static class MapHelper {
        public const float TileSize = 1.2f;
        public const float HalfTileSize = TileSize * 0.5f;

        public static Vector2 GetMapStartPos(MapData mapData) => new Vector2(
            -HalfTileSize * (mapData.width - 1),
            HalfTileSize * (mapData.height - 1)
        );

        public static bool IsWallTile(int x, int y) {
            var mapData = MapLoader.CachedMapData;
            if (mapData == null) return true;

            if (x < 0 || x >= mapData.width) return true;
            if (y < 0 || y >= mapData.height) return true;

            return mapData.map[y][x] == (int)MapData.TileType.Wall;
        }

        public static Vector2Int GetMapIdx(float x, float y) => GetMapIdx(new Vector2(x, y));

        public static Vector2Int GetMapIdx(Vector2 worldPos) {
            var mapData = MapLoader.CachedMapData;
            if (mapData == null) return Vector2Int.zero;

            Vector2 mapStartPos = GetMapStartPos(mapData);
            Vector2 localPos = worldPos - mapStartPos;

            int x = Mathf.RoundToInt(localPos.x / TileSize);
            int y = Mathf.RoundToInt(-localPos.y / TileSize);

            return new Vector2Int(x, y);
        }
        
        public static Vector2 GetWorldPos(int x, int y) => GetWorldPos(new Vector2Int(x, y));
        
        public static Vector2 GetWorldPos(Vector2Int mapIdx) {
            var mapData = MapLoader.CachedMapData;
            if (mapData == null) return Vector2Int.zero;

            Vector2 mapStartPos = GetMapStartPos(mapData);
            return mapStartPos + new Vector2(mapIdx.x, -mapIdx.y) * TileSize;
        }
    }
}