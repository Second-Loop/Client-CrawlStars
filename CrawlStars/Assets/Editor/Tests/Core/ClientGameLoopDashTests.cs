using System;
using System.Reflection;
using Core;
using Core.Inputs;
using Core.Player;
using Core.Prediction;
using Network;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.Core {
    public class ClientGameLoopDashTests {
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        private GameObject gameLoopRoot;
        private GameObject listenerRoot;
        private ClientGameLoop gameLoop;
        private AttackManager attackManager;
        private PlayerListener listener;

        [SetUp]
        public void SetUp() {
            PlayerManager.Instance.playerListeners.Clear();
            PlayerManager.Instance.MyId = "me";
            PlayerManager.Instance.MyTeam = "red";

            listenerRoot = new GameObject("ClientGameLoopDashTests.Listener");
            listener = listenerRoot.AddComponent<PlayerListener>();
            SetPrivate(listener, "bodyRoot", listener.transform);
            SetPrivate(listener, "spriteAnimator", listenerRoot.AddComponent<PlayerSpriteAnimator>());
            SetPrivate(listener, "meleeAttackEffect", listenerRoot.AddComponent<SpriteRenderer>());
            PlayerManager.Instance.playerListeners.Add("me", listener);
            SetPrivateProperty(PlayerManager.Instance, "MyListener", listener);

            gameLoopRoot = new GameObject("ClientGameLoopDashTests.GameLoop");
            var inputProvider = gameLoopRoot.AddComponent<InputProvider>();
            attackManager = gameLoopRoot.AddComponent<AttackManager>();
            SetPrivate(attackManager, "serverAuthoritative", true);
            SetPrivate(attackManager, "authoritativeCombatState", new AuthoritativeCombatState(3, 1f, 12f));

            gameLoop = gameLoopRoot.AddComponent<ClientGameLoop>();
            SetPrivate(gameLoop, "inputProvider", inputProvider);
            SetPrivate(gameLoop, "attackManager", attackManager);
            SetPrivate(gameLoop, "isInitialized", true);
            SetPrivate(gameLoop, "isActive", true);
        }

        [TearDown]
        public void TearDown() {
            PlayerManager.Instance.playerListeners.Clear();
            SetPrivateProperty(PlayerManager.Instance, "MyListener", null);
            PlayerManager.Instance.MyId = null;
            PlayerManager.Instance.MyTeam = null;
            if (gameLoopRoot != null) UnityEngine.Object.DestroyImmediate(gameLoopRoot);
            if (listenerRoot != null) UnityEngine.Object.DestroyImmediate(listenerRoot);
        }

        [Test]
        public void DashSnapshot_CancelsPredictionAtNewestServerPositionAndBlocksNewPrediction() {
            BeginPrediction();

            Observe(2, Player(position: new Vector2(7f, 4f), isDashing: true));

            Assert.That(attackManager.IsDashing, Is.True);
            Assert.That(Predictor.IsActive, Is.False);
            Assert.That(listener.transform.position.x, Is.EqualTo(7f));
            Assert.That(listener.transform.position.y, Is.EqualTo(4f));

            Submit(2, Vector2.left);

            Assert.That(Predictor.IsActive, Is.False);
        }

        [Test]
        public void UpdateLocalPrediction_WhenDashBecomesActive_CancelsOutstandingPrediction() {
            BeginPrediction();
            attackManager.ObserveSnapshot(2, Player(position: new Vector2(1f, 1f), isDashing: true));

            Invoke(gameLoop, "UpdateLocalPrediction");

            Assert.That(Predictor.IsActive, Is.False);
            Assert.That(listener.transform.position.x, Is.EqualTo(1f));
            Assert.That(listener.transform.position.y, Is.EqualTo(1f));
        }

        [Test]
        public void NonDashSnapshot_AfterDash_AllowsPredictionToResume() {
            BeginPrediction();
            Observe(2, Player(position: new Vector2(2f, 2f), isDashing: true));

            Observe(3, Player(position: new Vector2(3f, 3f), isDashing: false));
            Submit(3, Vector2.up);

            Assert.That(attackManager.IsDashing, Is.False);
            Assert.That(Predictor.IsActive, Is.True);
        }

        [Test]
        public void Clear_RemovesDashAndPredictionUntilNextLiveSnapshot() {
            Observe(1, Player(position: new Vector2(1f, 1f), isDashing: true));

            gameLoop.Clear();

            Assert.That(attackManager.IsDashing, Is.False);
            Assert.That(Predictor.HasServerState, Is.False);
            Assert.That(Predictor.IsActive, Is.False);

            SetPrivate(gameLoop, "isInitialized", true);
            SetPrivate(gameLoop, "isActive", true);
            Observe(2, Player(position: new Vector2(2f, 2f), isDashing: false));
            Submit(2, Vector2.right);

            Assert.That(Predictor.IsActive, Is.True);
        }

        [Test]
        public void DeadLocalPlayerSnapshot_CancelsOutstandingPrediction() {
            BeginPrediction();

            Observe(2, Player(position: new Vector2(2f, 2f), isDead: true));

            Assert.That(Predictor.IsActive, Is.False);
            Assert.That(attackManager.TryNormalAttack(), Is.False);
        }

        [Test]
        public void MissingLocalPlayer_CancelsPredictionAndBlocksCombatState() {
            BeginPrediction();

            Invoke(gameLoop, "ObserveLocalPlayerSnapshot", 2L, Array.Empty<PlayerData>());

            Assert.That(Predictor.IsActive, Is.False);
            Assert.That(attackManager.TryNormalAttack(), Is.False);
        }

        [Test]
        public void PositiveTickNullPlayers_CancelsPredictionAndClearsCombatState() {
            BeginPrediction();

            Invoke(gameLoop, "HandleSnapshot", new SnapshotDto {
                Status = "started",
                Tick = 2,
                Players = null
            });

            Assert.That(Predictor.IsActive, Is.False);
            Assert.That(attackManager.TryNormalAttack(), Is.False);
        }

        [Test]
        public void TickZeroNullPlayers_PreservesPrestartPredictionState() {
            BeginPrediction();

            Invoke(gameLoop, "HandleSnapshot", new SnapshotDto {
                Status = "started",
                Tick = 0,
                Players = null
            });

            Assert.That(Predictor.IsActive, Is.True);
            Assert.That(attackManager.TryNormalAttack(), Is.True);
        }

        private LocalMovementPredictor Predictor =>
            (LocalMovementPredictor)typeof(ClientGameLoop).GetField("localPredictor", PrivateInstance)?.GetValue(gameLoop);

        private void BeginPrediction() {
            listener.transform.position = new Vector3(1f, 1f, 0f);
            Observe(1, Player(position: new Vector2(1f, 1f)));
            Submit(1, Vector2.right);
            Assert.That(Predictor.IsActive, Is.True, "fixture must start an actual prediction");
        }

        private void Observe(long tick, PlayerData player) =>
            Invoke(gameLoop, "ObserveLocalPlayerSnapshot", tick, new[] { player });

        private void Submit(long tick, Vector2 direction) =>
            Invoke(gameLoop, "HandleInputSubmitted", new InputMessageDto {
                ClientTick = tick,
                MoveDir = new Vector2Dto(direction)
            });

        private static PlayerData Player(Vector2 position, bool isDashing = false, bool isDead = false) => new PlayerData {
            Id = "me",
            Pos = new Vector2Dto(position),
            MoveDir = new Vector2Dto(Vector2.zero),
            Speed = 6f,
            AttackCharges = 3,
            IsDashing = isDashing,
            IsDead = isDead
        };

        private static object Invoke(object target, string methodName, params object[] arguments) =>
            target.GetType().GetMethod(methodName, PrivateInstance)?.Invoke(target, arguments);

        private static void SetPrivate(object target, string fieldName, object value) =>
            target.GetType().GetField(fieldName, PrivateInstance)?.SetValue(target, value);

        private static void SetPrivateProperty(object target, string propertyName, object value) =>
            target.GetType().GetProperty(propertyName, PrivateInstance | BindingFlags.Public)?.SetValue(target, value);
    }
}
