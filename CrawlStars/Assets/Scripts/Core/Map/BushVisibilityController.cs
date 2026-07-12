using System.Collections.Generic;
using Core.Player;
using UnityEngine;

namespace Core.Map {
    public class BushVisibilityController {
        private static BushVisibilityController instance;
        public static BushVisibilityController Instance => instance ??= new BushVisibilityController();

        private readonly Dictionary<Vector2Int, int> bushNumbers = new Dictionary<Vector2Int, int>();

        public void Initialize() {
            var mapData = MapHelper.CachedMapData;
            if (mapData == null) {
                Debug.LogError("BushVisibilityController.Initialize::mapData is null");
                return;
            }

            SetBushNumbers(mapData);
        }

        // BFS
        private void SetBushNumbers(MapData mapData) {
            bushNumbers.Clear();

            Vector2Int[] dir = new Vector2Int[] { new Vector2Int(1, 0),  new Vector2Int(0, -1), new Vector2Int(0, 1) , new Vector2Int(-1, 0) };
            bool[][] isVisited = new bool[mapData.height][];
            for (int y = 0; y < mapData.height; ++y) {
                isVisited[y] = new bool[mapData.width];
            }

            int bushNum = 0;

            for (int y = 0; y < mapData.height; ++y) {
                for (int x = 0; x < mapData.width; ++x) {
                    if (mapData.map[y][x] == (int)Tile.TileType.Bush && !isVisited[y][x]) {
                        var queue = new Queue<Vector2Int>();
                        queue.Enqueue(new Vector2Int(x, y));
                        isVisited[y][x] = true;

                        while (queue.Count > 0) {
                            Vector2Int cur = queue.Dequeue();
                            bushNumbers.TryAdd(cur, bushNum);

                            for (int k = 0; k < dir.Length; ++k) {
                                var next = cur + dir[k];
                                if (next.x < 0 || next.x >= mapData.width) continue;
                                if (next.y < 0 || next.y >= mapData.height) continue;
                                if (isVisited[next.y][next.x]) continue;
                                if (mapData.map[next.y][next.x] != (int)Tile.TileType.Bush) continue;

                                queue.Enqueue(next);
                                isVisited[next.y][next.x] = true;
                            }
                        }
                        ++bushNum;
                    }
                }
            }
        }

        public void SetVisibility(IReadOnlyList<PlayerData> players) {
            var myPos = MapHelper.GetMapIdx(PlayerManager.Instance.MyListener.transform.position);
            bool amIInBush = bushNumbers.TryGetValue(myPos, out var myBushNum);

            foreach (var player in players) {
                if (player.Id == PlayerManager.Instance.MyId) continue;
                if (!PlayerManager.Instance.GetListener(player.Id, out PlayerListener listener)) continue;
                if (player.IsDead) continue;
                if (player.Team == PlayerManager.Instance.MyTeam) continue;

                var idx = MapHelper.GetMapIdx(listener.transform.position);
                if (!MapHelper.IsInBush(idx)) {
                    listener.gameObject.SetActive(true);
                    continue;
                }

                if (!bushNumbers.TryGetValue(idx, out var bushNum)) {
                    Debug.LogError($"BushVisibilityController::SetVisibility::({idx.x},{idx.y}) is bush tile but, bushNumbers not found");
                    continue;
                }

                bool canSee = amIInBush && myBushNum == bushNum;
                listener.gameObject.SetActive(canSee);
            }
        }
    }
}
