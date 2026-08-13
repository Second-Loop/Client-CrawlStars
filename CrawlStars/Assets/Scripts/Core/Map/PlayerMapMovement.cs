using UnityEngine;

namespace Core.Map {
    public static class PlayerMapMovement {
        public static Vector2 Move(Vector2 position, Vector2 movement, float radius, MapData mapData) {
            Vector2 result = position;

            Vector2 nextX = new Vector2(position.x + movement.x, position.y);
            if (!CollidesWithMap(nextX, radius, mapData)) {
                result.x = nextX.x;
            }

            Vector2 nextY = new Vector2(result.x, position.y + movement.y);
            if (!CollidesWithMap(nextY, radius, mapData)) {
                result.y = nextY.y;
            }

            return result;
        }

        private static bool CollidesWithMap(Vector2 position, float radius, MapData mapData) {
            if (mapData?.map == null || mapData.width <= 0 || mapData.height <= 0) return false;

            float tileSize = GameConfig.TileSize;
            if (tileSize <= 0f) return false;

            radius = Mathf.Max(0f, radius);
            float halfTileSize = tileSize * 0.5f;
            Vector2 mapStart = new Vector2(
                -halfTileSize * (mapData.width - 1),
                halfTileSize * (mapData.height - 1)
            );
            float minX = mapStart.x - halfTileSize;
            float maxX = mapStart.x + (mapData.width - 1) * tileSize + halfTileSize;
            float maxY = mapStart.y + halfTileSize;
            float minY = mapStart.y - (mapData.height - 1) * tileSize - halfTileSize;

            if (position.x - radius < minX || position.x + radius > maxX ||
                position.y - radius < minY || position.y + radius > maxY) {
                return true;
            }

            int rowCount = Mathf.Min(mapData.height, mapData.map.Length);
            for (int y = 0; y < rowCount; y++) {
                int[] row = mapData.map[y];
                if (row == null) continue;

                int columnCount = Mathf.Min(mapData.width, row.Length);
                for (int x = 0; x < columnCount; x++) {
                    var tileType = (Tile.TileType)row[x];
                    if (tileType is not (Tile.TileType.Wall or Tile.TileType.Water)) continue;

                    Vector2 tileCenter = mapStart + new Vector2(x, -y) * tileSize;
                    float nearestX = Mathf.Clamp(position.x, tileCenter.x - halfTileSize, tileCenter.x + halfTileSize);
                    float nearestY = Mathf.Clamp(position.y, tileCenter.y - halfTileSize, tileCenter.y + halfTileSize);
                    float dx = position.x - nearestX;
                    float dy = position.y - nearestY;
                    if (dx * dx + dy * dy <= radius * radius) return true;
                }
            }

            return false;
        }
    }
}
