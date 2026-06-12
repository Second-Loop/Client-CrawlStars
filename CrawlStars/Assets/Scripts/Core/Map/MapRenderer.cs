using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace Core.Map {
    public class MapRenderer : MonoBehaviour {
        [SerializeField] private Transform mapRoot;

        private List<Tile> loadedTiles = new List<Tile>();

        public void Render(int mapIndex) {
            var mapData = MapLoader.GetMapData(mapIndex);
            if (mapData == null) return;

            Vector2 startPos = MapHelper.GetMapStartPos(mapData);

            for (int y = 0; y < mapData.height; ++y) {
                for (int x = 0; x < mapData.width; ++x) {
                    var obj = ObjectPooling.Instance.Get<Tile>(Constants.Tile, mapRoot);
                    if (obj == null) return;

                    var tileType = (Tile.TileType)mapData.map[y][x];
                    obj.Initialize(tileType, MapHelper.TileSize);
                    obj.transform.localPosition = startPos + new Vector2(x, -y) * MapHelper.TileSize;
                    loadedTiles.Add(obj);
                }
            }
        }

        public void Clear() {
            foreach (var tile in loadedTiles) {
                ObjectPooling.Instance.TryAbandon(Constants.Tile, tile.gameObject);
            }
            loadedTiles.Clear();
        }
    }
}