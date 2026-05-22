using Core.Map;
using UnityEngine;

namespace Core.Simulator {
    public static class Physics {
        public static Vector2 GetNextPosition(Vector2 origin, Vector2 movement, float radius = 0.5f) {
            Vector2 nextX = origin + new Vector2(movement.x, 0f);
            bool isHorizontalBlocked = CheckCollision(nextX, radius);
            if (!isHorizontalBlocked) {
                origin = nextX;
            }

            Vector2 nextY = origin + new Vector2(0f, movement.y);
            bool isVerticalBlocked = CheckCollision(nextY, radius);
            if (!isVerticalBlocked) {
                origin = nextY;
            }

            Debug.Log($"isHorizontalBlocked: {isHorizontalBlocked} /  isVerticalBlocked: {isVerticalBlocked}");
            return origin;
        }
        
        private static bool CheckCollision(Vector2 circleCenter, float radius) {
            // 원과 겹칠 수 있는 후보군 추리기
            Vector2Int circleLeftUpIdx = MapHelper.GetMapIdx(circleCenter.x - radius, circleCenter.y + radius);
            Vector2Int circleRightUpIdx = MapHelper.GetMapIdx(circleCenter.x + radius, circleCenter.y - radius);

            // 후보군 모두 충돌 검증
            for (int y = circleLeftUpIdx.y; y <= circleRightUpIdx.y; ++y) {
                for (int x = circleLeftUpIdx.x; x <= circleRightUpIdx.x; ++x) {
                    if (!MapHelper.IsWallTile(x, y)) continue;

                    Vector2 tileCenter = MapHelper.GetWorldPos(x, y);

                    // 원의 중심에서 사각형에 가장 가까운 점 찾기
                    float closestX = Mathf.Clamp(circleCenter.x, tileCenter.x - MapHelper.HalfTileSize, tileCenter.x + MapHelper.HalfTileSize);
                    float closestY = Mathf.Clamp(circleCenter.y, tileCenter.y - MapHelper.HalfTileSize, tileCenter.y + MapHelper.HalfTileSize);

                    // 그 점과 원 중심 사이의 거리 구하기
                    float dx = circleCenter.x - closestX;
                    float dy = circleCenter.y - closestY;

                    // 그 거리가 원의 반지름보다 작거나 같으면 충돌
                    bool isOverlapped = dx * dx + dy * dy <= radius * radius;
                    if (isOverlapped) return true;
                }
            }
            return false;
        }
    }
}