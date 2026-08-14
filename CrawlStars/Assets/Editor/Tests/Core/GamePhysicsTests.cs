using System.Reflection;
using Core;
using Core.Map;
using Core.Prediction;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.Core {
    public class GamePhysicsTests {
        private static readonly FieldInfo TileSizeField = typeof(GameConfig).GetField(
            "<TileSize>k__BackingField",
            BindingFlags.Static | BindingFlags.NonPublic
        );
        private static readonly FieldInfo PlayerRadiusField = typeof(GameConfig).GetField(
            "<PlayerRadius>k__BackingField",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        private float previousTileSize;
        private float previousPlayerRadius;

        [SetUp]
        public void SetUp() {
            previousTileSize = (float)TileSizeField.GetValue(null);
            previousPlayerRadius = (float)PlayerRadiusField.GetValue(null);
            TileSizeField.SetValue(null, 2f);
            PlayerRadiusField.SetValue(null, 0.5f);
            MapHelper.CachedMapData = CreateMap();
        }

        [TearDown]
        public void TearDown() {
            MapHelper.CachedMapData = null;
            TileSizeField.SetValue(null, previousTileSize);
            PlayerRadiusField.SetValue(null, previousPlayerRadius);
        }

        [Test]
        public void GetNextPosition_WithoutCollision_MovesAlongBothAxes() {
            Vector2 result = GamePhysics.GetNextPosition(Vector2.zero, new Vector2(-0.4f, 0.4f));

            Assert.That(result, Is.EqualTo(new Vector2(-0.4f, 0.4f)));
        }

        [TestCase(Tile.TileType.Wall)]
        [TestCase(Tile.TileType.Water)]
        public void GetNextPosition_WhenHorizontalMovementIsBlocked_SlidesAlongWall(Tile.TileType tileType) {
            MapHelper.CachedMapData.map[2][3] = (int)tileType;

            Vector2 result = GamePhysics.GetNextPosition(Vector2.zero, new Vector2(0.6f, 0.4f));

            Assert.That(result, Is.EqualTo(new Vector2(0f, 0.4f)));
        }

        [Test]
        public void GetNextPosition_WhenCircleTouchesWall_TreatsItAsCollision() {
            MapHelper.CachedMapData.map[2][3] = (int)Tile.TileType.Wall;

            Vector2 result = GamePhysics.GetNextPosition(Vector2.zero, new Vector2(0.5f, 0f));

            Assert.That(result, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void GetNextPosition_WhenCircleWouldLeaveMap_BlocksMovement() {
            Vector2 currentPosition = new Vector2(4.4f, 0f);

            Vector2 result = GamePhysics.GetNextPosition(currentPosition, new Vector2(0.2f, 0f));

            Assert.That(result, Is.EqualTo(currentPosition));
        }

        private static MapData CreateMap() {
            const int size = 5;
            var tiles = new int[size][];
            for (int y = 0; y < size; y++) {
                tiles[y] = new int[size];
            }

            return new MapData {
                width = size,
                height = size,
                map = tiles
            };
        }
    }
}
