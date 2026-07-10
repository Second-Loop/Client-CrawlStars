using System.Collections.Generic;
using Core.Map;
using UnityEngine;

namespace Core.Simulator {

    // A* 기반 길찾기
    public static class BotPathFinder {

        // 탐색할 방향
        private static readonly Vector2Int[] Directions = {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        private const float MinDirectionSqrMagnitude = 0.0001f;

        public static Vector2 GetMoveDirection(Vector2 fromWorld, Vector2 toWorld) {
            if (MapHelper.CachedMapData == null) {
                return GetDirectDir(fromWorld, toWorld);
            }

            Vector2Int start = MapHelper.GetMapIdx(fromWorld);
            Vector2Int goal = MapHelper.GetMapIdx(toWorld);

            if (start == goal || MapHelper.IsPathBlockedTile(start.x, start.y)) {
                return GetDirectDir(fromWorld, toWorld);
            }

            if (MapHelper.IsPathBlockedTile(goal.x, goal.y)) {
                return GetDirectDir(fromWorld, toWorld);
            }

            if (!TryFindNextTile(start, goal, out Vector2Int nextTile)) {
                return GetDirectDir(fromWorld, toWorld);
            }

            Vector2 nextWorld = MapHelper.GetWorldPos(nextTile);
            return GetDirectDir(fromWorld, nextWorld);
        }

        // A* 알고리즘
        private static bool TryFindNextTile(Vector2Int start, Vector2Int goal, out Vector2Int nextTile) {
            nextTile = start;

            // heap으로 최적화 가능
            var open = new List<Node> { new Node(start, null, 0, GetHeuristic(start, goal)) };
            var closed = new HashSet<Vector2Int>();
            var bestCosts = new Dictionary<Vector2Int, int> { {start, 0} };

            MapData mapData = MapHelper.CachedMapData;
            int maxIterations = mapData.width * mapData.height;
            int iterations = 0;

            while (open.Count > 0 && iterations < maxIterations) {
                ++iterations;

                // 매번 전체 순회해서 찾기 때문에 장애물에 막혔더라도 최저 비용 노드부터 탐색
                Node current = PopBestNode(open);
                if (current.Pos == goal) {
                    nextTile = GetFirstStep(current, start);
                    return true;
                }

                if (!closed.Add(current.Pos)) continue;

                foreach (Vector2Int direction in Directions) {
                    Vector2Int next = current.Pos + direction;
                    if (MapHelper.IsPathBlockedTile(next.x, next.y) || closed.Contains(next)) continue;

                    // G는 누적 비용이기 때문에 현재 비용에 다음 거리인 1을 누적해서 더해줌
                    int nextCost = current.G + 1;

                    // 다음 노드가 이미 더 좋은 비용으로 탐색한 이력이 있다면 패스
                    if (bestCosts.TryGetValue(next, out int bestCost) && bestCost <= nextCost) continue;

                    // 이미 탐색한 노드지만 더 좋은 비용이라면 업데이트
                    bestCosts[next] = nextCost;
                    open.Add(new Node(next, current, nextCost, GetHeuristic(next, goal)));
                }
            }

            return false;
        }

        // 가장 F 값이 작은 노드 반환, F가 같으면 H를 비교
        private static Node PopBestNode(List<Node> open) {
            int bestIndex = 0;

            for (int i = 1; i < open.Count; ++i) {
                if (open[i].F > open[bestIndex].F) continue;
                if (open[i].F == open[bestIndex].F && open[i].H >= open[bestIndex].H) continue;

                bestIndex = i;
            }

            Node best = open[bestIndex];
            open.RemoveAt(bestIndex);
            return best;
        }

        // 시작점 바로 다음 노드를 반환
        private static Vector2Int GetFirstStep(Node goalNode, Vector2Int start) {
            Node current = goalNode;
            // 다음 parent가 start일 때까지 순회, parent가 이전 노드임
            while (current.Parent != null && current.Parent.Pos != start) {
                current = current.Parent;
            }
            return current.Pos;
        }

        private static int GetHeuristic(Vector2Int a, Vector2Int b) {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        // fallback용
        private static Vector2 GetDirectDir(Vector2 from, Vector2 to) {
            Vector2 direction = to - from;
            return direction.sqrMagnitude <= MinDirectionSqrMagnitude ? Vector2.zero : direction.normalized;
        }

        private class Node {
            public Vector2Int Pos { get; }
            public Node Parent { get; }
            public int G { get; }   // 출발 노드에서 현재 노드까지 도달하기 위한 누적 비용
            public int H { get; }   // 현재 노드에서 도착 노드까지의 예상 비용 (휴리스틱)
            public int F => G + H;

            public Node(Vector2Int pos, Node parent, int g, int h) {
                Pos = pos;
                Parent = parent;
                G = g;
                H = h;
            }
        }
    }
}
