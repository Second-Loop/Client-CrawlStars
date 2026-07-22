using System.Reflection;
using Core;
using Core.Map;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.Core {
    public class MapHelperTests {
        private static readonly FieldInfo TileSizeField = typeof(GameConfig).GetField(
            "<TileSize>k__BackingField",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        private float previousTileSize;

        [SetUp]
        public void SetUp() {
            previousTileSize = (float)TileSizeField.GetValue(null);
            TileSizeField.SetValue(null, 2f);
            MapHelper.CachedMapData = CreateMap();
        }

        [TearDown]
        public void TearDown() {
            MapHelper.CachedMapData = null;
            TileSizeField.SetValue(null, previousTileSize);
        }

        [Test]
        public void GetMapStartPos_CentersMapAroundWorldOrigin() {
            Vector2 start = MapHelper.GetMapStartPos(MapHelper.CachedMapData);

            Assert.That(start, Is.EqualTo(new Vector2(-2f, 1f)));
        }

        [Test]
        public void GetWorldPosAndGetMapIdx_RoundTripEveryTile() {
            MapData map = MapHelper.CachedMapData;

            for (int y = 0; y < map.height; ++y) {
                for (int x = 0; x < map.width; ++x) {
                    Vector2 world = MapHelper.GetWorldPos(x, y);
                    Assert.That(MapHelper.GetMapIdx(world), Is.EqualTo(new Vector2Int(x, y)));
                }
            }
        }

        [TestCase(0, 0, false)]
        [TestCase(1, 0, true)]
        [TestCase(2, 0, false)]
        [TestCase(0, 1, true)]
        [TestCase(1, 1, false)]
        [TestCase(-1, 0, true)]
        [TestCase(3, 0, true)]
        [TestCase(0, 2, true)]
        public void IsPathBlockedTile_ReturnsExpectedResult(int x, int y, bool expected) {
            Assert.That(MapHelper.IsPathBlockedTile(x, y), Is.EqualTo(expected));
        }

        [TestCase(2, 0, true)]
        [TestCase(0, 0, false)]
        [TestCase(1, 0, false)]
        [TestCase(-1, 0, false)]
        [TestCase(3, 0, false)]
        public void IsInBush_ReturnsTrueOnlyForInBoundsBushTile(int x, int y, bool expected) {
            Assert.That(MapHelper.IsInBush(x, y), Is.EqualTo(expected));
        }

        [Test]
        public void Helpers_WithoutCachedMap_ReturnSafeDefaults() {
            MapHelper.CachedMapData = null;

            Assert.That(MapHelper.IsPathBlockedTile(0, 0), Is.True);
            Assert.That(MapHelper.IsInBush(0, 0), Is.False);
            Assert.That(MapHelper.GetMapIdx(Vector2.one), Is.EqualTo(Vector2Int.zero));
            Assert.That(MapHelper.GetWorldPos(1, 1), Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void GetMapStartPos_UsesCurrentTileSize() {
            Assert.That(MapHelper.GetMapStartPos(MapHelper.CachedMapData), Is.EqualTo(new Vector2(-2f, 1f)));

            TileSizeField.SetValue(null, 4f);

            Assert.That(MapHelper.GetMapStartPos(MapHelper.CachedMapData), Is.EqualTo(new Vector2(-4f, 2f)));
        }

        private static MapData CreateMap() => new MapData {
            width = 3,
            height = 2,
            map = new[] {
                new[] {
                    (int)Tile.TileType.Ground,
                    (int)Tile.TileType.Wall,
                    (int)Tile.TileType.Bush
                },
                new[] {
                    (int)Tile.TileType.Water,
                    (int)Tile.TileType.SpawnPoint,
                    (int)Tile.TileType.Ground
                }
            }
        };
    }
}
