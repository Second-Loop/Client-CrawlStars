using UnityEngine;

namespace Core.Map {
    public static class MapHelper {
        public static MapData CachedMapData { get; set; }

        private static float HalfTileSize => GameConfig.TileSize * 0.5f;

        public static Vector2 GetMapStartPos(MapData mapData) => new Vector2(
            -HalfTileSize * (mapData.width - 1),
            HalfTileSize * (mapData.height - 1)
        );

        public static bool IsPathBlockedTile(int x, int y) {
            var mapData = CachedMapData;
            if (mapData == null) return true;

            if (x < 0 || x >= mapData.width) return true;
            if (y < 0 || y >= mapData.height) return true;

            int tileType = mapData.map[y][x];
            return (Tile.TileType)tileType is Tile.TileType.Wall or Tile.TileType.Water;
        }

        public static bool IsInBush(Vector2Int pos) => IsInBush(pos.x, pos.y);

        public static bool IsInBush(int x, int y) {
            var mapData = CachedMapData;
            if (mapData == null) return false;

            if (x < 0 || x >= mapData.width) return false;
            if (y < 0 || y >= mapData.height) return false;

            return mapData.map[y][x] == (int)Tile.TileType.Bush;
        }

        public static Vector2Int GetMapIdx(float x, float y) => GetMapIdx(new Vector2(x, y));

        public static Vector2Int GetMapIdx(Vector2 worldPos) {
            var mapData = CachedMapData;
            if (mapData == null) return Vector2Int.zero;

            Vector2 mapStartPos = GetMapStartPos(mapData);
            Vector2 localPos = worldPos - mapStartPos;

            int x = Mathf.RoundToInt(localPos.x / GameConfig.TileSize);
            int y = Mathf.RoundToInt(-localPos.y / GameConfig.TileSize);

            return new Vector2Int(x, y);
        }
        
        public static Vector2 GetWorldPos(int x, int y) => GetWorldPos(new Vector2Int(x, y));
        
        public static Vector2 GetWorldPos(Vector2Int mapIdx) {
            var mapData = CachedMapData;
            if (mapData == null) return Vector2Int.zero;

            Vector2 mapStartPos = GetMapStartPos(mapData);
            return mapStartPos + new Vector2(mapIdx.x, -mapIdx.y) * GameConfig.TileSize;
        }
    }
}
