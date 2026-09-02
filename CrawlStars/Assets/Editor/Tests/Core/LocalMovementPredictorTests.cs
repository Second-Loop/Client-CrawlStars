using System.Reflection;
using Core;
using Core.Map;
using Core.Player;
using Core.Prediction;
using Network;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.Core {
    public class LocalMovementPredictorTests {
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
            MapHelper.CachedMapData = CreateOpenMap();
        }

        [TearDown]
        public void TearDown() {
            MapHelper.CachedMapData = null;
            TileSizeField.SetValue(null, previousTileSize);
            PlayerRadiusField.SetValue(null, previousPlayerRadius);
        }

        [Test]
        public void TryUpdatePosition_FirstFrame_UsesFullServerSpeed() {
            const float frameSeconds = 1f / 60f;
            const float serverSpeed = 2f;
            var predictor = new LocalMovementPredictor();
            predictor.ObserveSnapshot(new PlayerData {
                Pos = new Vector2Dto(Vector2.zero),
                Speed = serverSpeed,
                LastProcessedClientTick = 0
            });
            predictor.HandleInput(1, Vector2.right, Vector2.zero);

            var updated = predictor.TryUpdatePosition(frameSeconds, out var position);

            Assert.That(updated, Is.True);
            Assert.That(position.x, Is.EqualTo(serverSpeed * frameSeconds).Within(0.000001f));
            Assert.That(position.y, Is.Zero.Within(0.000001f));
        }

        [Test]
        public void TryUpdatePosition_NegativeDelta_DoesNotMoveBackward() {
            var predictor = ActivePredictor();

            predictor.TryUpdatePosition(-1f / 60f, out var position);

            Assert.That(position, Is.EqualTo(Vector2.zero));
            Assert.That(predictor.IsActive, Is.True);
        }

        [Test]
        public void ObserveSnapshot_AcknowledgedInput_ReconcilesToAuthoritativePosition() {
            var predictor = ActivePredictor();
            predictor.TryUpdatePosition(1f / 60f, out _);

            predictor.ObserveSnapshot(new PlayerData {
                Pos = new Vector2Dto(new Vector2(0.02f, 0f)),
                Speed = 2f,
                LastProcessedClientTick = 1
            });

            Assert.That(predictor.IsActive, Is.False);
            Assert.That(predictor.CurPosition.x, Is.EqualTo(0.02f).Within(0.000001f));
            Assert.That(predictor.CurPosition.y, Is.Zero.Within(0.000001f));
        }

        [Test]
        public void TryUpdatePosition_AtPredictionDuration_ReturnsToLatestServerPosition() {
            var predictor = ActivePredictor();

            predictor.TryUpdatePosition(0.119f, out _);

            Assert.That(predictor.IsActive, Is.True);

            predictor.TryUpdatePosition(0.001f, out _);

            Assert.That(predictor.IsActive, Is.False);
            Assert.That(predictor.CurPosition, Is.EqualTo(Vector2.zero));
        }

        private static LocalMovementPredictor ActivePredictor() {
            var predictor = new LocalMovementPredictor();
            predictor.ObserveSnapshot(new PlayerData {
                Pos = new Vector2Dto(Vector2.zero),
                Speed = 2f,
                LastProcessedClientTick = 0
            });
            predictor.HandleInput(1, Vector2.right, Vector2.zero);
            return predictor;
        }

        private static MapData CreateOpenMap() {
            const int size = 5;
            var tiles = new int[size][];
            for (var row = 0; row < size; row++) {
                tiles[row] = new int[size];
            }
            return new MapData {
                width = size,
                height = size,
                map = tiles
            };
        }
    }
}
