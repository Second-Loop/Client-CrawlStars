using Core.Map;
using UnityEngine;

namespace Core.Prediction {
    public static class GamePhysics {
        public static Vector2 GetNextPosition(Vector2 curPos, Vector2 movDir) {
            Vector2 result = curPos;

            // 축을 하나씩 체크해서 한쪽 면 충돌했을 때 미끄러지게 처리
            Vector2 nextX = new Vector2(curPos.x + movDir.x, curPos.y);
            if (!CheckCollision(nextX)) {
                result.x = nextX.x;
            }

            Vector2 nextY = new Vector2(result.x, curPos.y + movDir.y);
            if (!CheckCollision(nextY)) {
                result.y = nextY.y;
            }
            return result;
        }

        private static bool CheckCollision(Vector2 position) {
            var mapData = MapHelper.CachedMapData;
            if (mapData?.map == null || mapData.width <= 0 || mapData.height <= 0) {
                Debug.LogError($"GamePhysics.CheckCollision::invalid CachedMapData");
                return false;
            }

            float tileSize = GameConfig.TileSize;
            float radius = GameConfig.PlayerRadius;
            float halfTileSize = MapHelper.HalfTileSize;
            Vector2 mapStart = MapHelper.GetMapStartPos(mapData);

            float minX = mapStart.x - halfTileSize;
            float maxX = mapStart.x + (mapData.width - 1) * tileSize + halfTileSize;
            float maxY = mapStart.y + halfTileSize;
            float minY = mapStart.y - (mapData.height - 1) * tileSize - halfTileSize;

            // 맵 끝 처리
            if (position.x - radius < minX || position.x + radius > maxX
                || position.y - radius < minY || position.y + radius > maxY) {
                return true;
            }

            int rowCount = Mathf.Min(mapData.height, mapData.map.Length);
            if (rowCount <= 0) return false;

            // 원과 겹칠 수 있는 후보군 추리기
            // 부동소수점 경계 오차로 후보를 놓치지 않도록 각 방향으로 한 칸 여유를 둠
            int minCandidateX = Mathf.Max(0,
                Mathf.FloorToInt((position.x - radius - minX) / tileSize) - 1);
            int maxCandidateX = Mathf.Min(mapData.width - 1,
                Mathf.FloorToInt((position.x + radius - minX) / tileSize) + 1);
            int minCandidateY = Mathf.Max(0,
                Mathf.FloorToInt((maxY - position.y - radius) / tileSize) - 1);
            int maxCandidateY = Mathf.Min(rowCount - 1,
                Mathf.FloorToInt((maxY - position.y + radius) / tileSize) + 1);

            // 후보군 모두 충돌 검증
            for (int y = minCandidateY; y <= maxCandidateY; y++) {
                int[] row = mapData.map[y];
                if (row == null) continue;

                int columnCount = Mathf.Min(mapData.width, row.Length);
                int lastCandidateX = Mathf.Min(maxCandidateX, columnCount - 1);
                for (int x = minCandidateX; x <= lastCandidateX; x++) {
                    if (!MapHelper.IsPathBlockedTileType(row[x])) continue;

                    Vector2 tileCenter = mapStart + new Vector2(x, -y) * tileSize;
                    
                    // 원의 중심에서 사각형에 가장 가까운 점 찾기
                    float nearestX = Mathf.Clamp(position.x, tileCenter.x - halfTileSize, tileCenter.x + halfTileSize);
                    float nearestY = Mathf.Clamp(position.y, tileCenter.y - halfTileSize, tileCenter.y + halfTileSize);

                    // 그 점과 원 중심 사이의 거리 구하기
                    float dx = position.x - nearestX;
                    float dy = position.y - nearestY;

                    // 그 거리가 원의 반지름보다 작거나 같으면 충돌
                    bool isOverlapped = dx * dx + dy * dy <= radius * radius;
                    if (isOverlapped) return true;
                }
            }

            return false;
        }
    }
}
