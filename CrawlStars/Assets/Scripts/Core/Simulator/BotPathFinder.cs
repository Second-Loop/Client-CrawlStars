using System.Collections.Generic;
using Core.Map;
using UnityEngine;

namespace Core.Simulator {

    // A* 기반 길찾기
    public static class BotPathFinder {

        // 탐색할 수 있는 방향
        private static readonly Vector2Int[] Directions = {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        private const float MinDirectionSqrMagnitude = 0.0001f;

        public static Vector2 GetMoveDirection(Vector2 fromWorld, Vector2 toWorld) {
            if (MapLoader.CachedMapData == null) {
                return GetDirectDirection(fromWorld, toWorld);
            }

            Vector2Int start = MapHelper.GetMapIdx(fromWorld);
            Vector2Int goal = MapHelper.GetMapIdx(toWorld);

            if (start == goal || IsBlocked(start) || IsBlocked(goal)) {
                return GetDirectDirection(fromWorld, toWorld);
            }

            if (!TryFindNextTile(start, goal, out Vector2Int nextTile)) {
                return GetDirectDirection(fromWorld, toWorld);
            }

            Vector2 nextWorld = MapHelper.GetWorldPos(nextTile);
            return GetDirectDirection(fromWorld, nextWorld);
        }

        // A* 알고리즘
        private static bool TryFindNextTile(Vector2Int start, Vector2Int goal, out Vector2Int nextTile) {
            nextTile = start;

            var open = new List<Node> { new Node(start, null, 0, GetHeuristic(start, goal)) };
            var closed = new HashSet<Vector2Int>();
            var bestCosts = new Dictionary<Vector2Int, int> { {start, 0} };

            MapData mapData = MapLoader.CachedMapData;
            int maxIterations = mapData.width * mapData.height;
            int iterations = 0;

            while (open.Count > 0 && iterations < maxIterations) {
                ++iterations;

                Node current = PopBestNode(open);
                if (current.Pos == goal) {
                    nextTile = GetFirstStep(current, start);
                    return true;
                }

                if (!closed.Add(current.Pos)) continue;

                foreach (Vector2Int direction in Directions) {
                    Vector2Int next = current.Pos + direction;
                    if (IsBlocked(next) || closed.Contains(next)) continue;

                    int nextCost = current.G + 1;
                    if (bestCosts.TryGetValue(next, out int bestCost) && bestCost <= nextCost) continue;

                    bestCosts[next] = nextCost;
                    open.Add(new Node(next, current, nextCost, GetHeuristic(next, goal)));
                }
            }

            return false;
        }

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

        private static Vector2Int GetFirstStep(Node goalNode, Vector2Int start) {
            Node current = goalNode;

            while (current.Parent != null && current.Parent.Pos != start) {
                current = current.Parent;
            }

            return current.Pos;
        }

        private static int GetHeuristic(Vector2Int a, Vector2Int b) {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        private static bool IsBlocked(Vector2Int pos) {
            return MapHelper.IsWallTile(pos.x, pos.y);
        }

        // fallback용
        private static Vector2 GetDirectDirection(Vector2 from, Vector2 to) {
            Vector2 direction = to - from;
            return direction.sqrMagnitude <= MinDirectionSqrMagnitude ? Vector2.zero : direction.normalized;
        }

        private sealed class Node {
            public Vector2Int Pos { get; }
            public Node Parent { get; }
            public int G { get; }
            public int H { get; }
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
