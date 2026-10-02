using System;
using CameraControl;
using Core;
using Core.Map;
using Core.Player;
using Cysharp.Threading.Tasks;
using Network;
using Core.Projectile;
using Managing;
using Popup;
using UnityEngine;

public class GameManager : SingletonMonoBehaviour<GameManager> {
    [SerializeField] private MapRenderer mapRenderer;
    [SerializeField] private ClientGameLoop clientGameLoop;
    private bool isEnding;

    public IAttackCooldownSource AttackCooldownSource => clientGameLoop.AttackCooldownSource;

    public bool AmIDead => clientGameLoop.AmIDead;

    public void Initialize(ReadyEventMessageDto readyEvent) {
        if (readyEvent?.Map == null) {
            throw new ArgumentException("Ready event map is missing.", nameof(readyEvent));
        }

        MapHelper.CachedMapData = readyEvent.Map;
        mapRenderer.Render(MapHelper.CachedMapData);
        SpectateManager.Instance.Initialize();
        clientGameLoop.Initialize(readyEvent.Players);
        BushVisibilityController.Instance.Initialize();

        NetworkManager.Instance.GameEndReceived += HandleGameEnd;
        NetworkManager.Instance.SocketDisconnected += HandleSocketDisconnected;
    }

    public void OnEnterPlayScene() {
        SpectateManager.Instance.FocusCameraToMe();
        NetworkManager.Instance.SendReadyAckAsync().Forget();
    }

    public void Dispose() {
        MapHelper.CachedMapData = null;
        mapRenderer.Clear();
        clientGameLoop.Clear();
        isEnding = false;

        PlayerManager.Instance.ClearListeners();
        ProjectileManager.Instance.ClearListener();

        NetworkManager.Instance.GameEndReceived -= HandleGameEnd;
        NetworkManager.Instance.SocketDisconnected -= HandleSocketDisconnected;
        NetworkManager.Instance.DisconnectSocketAsync().Forget();
    }

    private async UniTask EndGameAsync(string result) {
        if (isEnding) return;
        isEnding = true;

        clientGameLoop.SetActive(false);
        var param = new OneButtonPopup.Param("Game End", result);
        await PopupManager.Instance.ShowAsync("OneButtonPopup", param);
        SceneController.Instance.ChangeSceneAsync(SceneController.MainSceneName, Dispose).Forget();
    }

    private void HandleGameEnd(GameEndMessageDto message) {
        if (message == null || message.PlayerId != PlayerManager.Instance.MyId) return;

        EndGameAsync(message.Result).Forget();
    }

    private void HandleSocketDisconnected() {
        HandleSocketDisconnectedInternal().Forget();
    }

    private async UniTask HandleSocketDisconnectedInternal() {
        await UniTask.WaitUntil(() => !SceneController.Instance.IsChanging);
        await EndGameAsync("The connection to the server was lost");
    }

    public void SetActiveInput(bool isActive) => clientGameLoop.SetActiveInput(isActive);

    public void RegisterOnDead(Action callback) => clientGameLoop.onDead += callback;
    public void UnregisterOnDead(Action callback) => clientGameLoop.onDead -= callback;

    public void RegisterOnDetectInput(Action<Vector2, bool> callback) => clientGameLoop.onDetectInput += callback;
    public void UnregisterOnDetectInput(Action<Vector2, bool> callback) => clientGameLoop.onDetectInput -= callback;
}
