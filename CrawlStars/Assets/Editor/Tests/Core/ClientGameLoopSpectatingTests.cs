using System.Reflection;
using CameraControl;
using Core;
using Core.Inputs;
using Core.Player;
using NUnit.Framework;
using UnityEngine;
using Utility;

namespace Tests.EditMode.Core {
    public class ClientGameLoopSpectatingTests {
        private GameObject cameraRoot;
        private GameObject gameLoopRoot;
        private ClientGameLoop gameLoop;
        private InputProvider inputProvider;

        [SetUp]
        public void SetUp() {
            cameraRoot = new GameObject("SpectatingTestCamera");
            cameraRoot.tag = "MainCamera";
            cameraRoot.AddComponent<Camera>();
            cameraRoot.AddComponent<CameraController>();
            Cache.OnChangeScene();

            PlayerManager.Instance.ClearListeners();
            PlayerManager.Instance.MyId = "me";
            PlayerManager.Instance.MyTeam = "red";

            gameLoopRoot = new GameObject("ClientGameLoopSpectatingTests");
            inputProvider = gameLoopRoot.AddComponent<InputProvider>();
            gameLoop = gameLoopRoot.AddComponent<ClientGameLoop>();
            typeof(ClientGameLoop).GetField("inputProvider", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(gameLoop, inputProvider);
        }

        [TearDown]
        public void TearDown() {
            PlayerManager.Instance.ClearListeners();
            PlayerManager.Instance.MyId = null;
            PlayerManager.Instance.MyTeam = null;
            if (gameLoopRoot != null) Object.DestroyImmediate(gameLoopRoot);
            if (cameraRoot != null) Object.DestroyImmediate(cameraRoot);
            Cache.OnChangeScene();
        }

        [Test]
        public void SetActiveInput_AfterLocalDeath_LeaveCancellationCannotReactivateInput() {
            PlayerManager.Instance.ObserveSnapshot(new[] {
                new PlayerData { Id = "me", Team = "red", IsDead = true }
            });

            gameLoop.SetActiveInput(false);
            gameLoop.SetActiveInput(true);

            Assert.That(inputProvider.IsActivated, Is.False);
        }

        [Test]
        public void ClearListeners_AfterLocalDeath_ResetsGuardForNextMatch() {
            PlayerManager.Instance.ObserveSnapshot(new[] {
                new PlayerData { Id = "me", Team = "red", IsDead = true }
            });

            PlayerManager.Instance.ClearListeners();
            gameLoop.SetActiveInput(true);

            Assert.That(PlayerManager.Instance.IsLocalPlayerDead, Is.False);
            Assert.That(inputProvider.IsActivated, Is.True);
        }
    }
}
