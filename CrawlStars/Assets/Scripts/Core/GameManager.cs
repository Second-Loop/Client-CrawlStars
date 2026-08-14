using System;
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

    public void Initialize(ReadyEventMessageDto readyEvent) {
        if (readyEvent?.Map == null) {
            throw new ArgumentException("Ready event map is missing.", nameof(readyEvent));
        }

        MapHelper.CachedMapData = readyEvent.Map;
        mapRenderer.Render(MapHelper.CachedMapData);
        clientGameLoop.Initialize(readyEvent.Players);
        BushVisibilityController.Instance.Initialize();

        NetworkManager.Instance.GameEndReceived += HandleGameEnd;
    }

    public void OnEnterPlayScene() {
        PlayerManager.Instance.FocusCamera();
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
        NetworkManager.Instance.DisconnectSocketAsync().Forget();
    }

    public void RegisterOnDetectInput(Action<Vector2, bool> callback) => clientGameLoop.OnDetectInput += callback;
    public void UnregisterOnDetectInput(Action<Vector2, bool> callback) => clientGameLoop.OnDetectInput -= callback;

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

    public void SetActiveInput(bool isActive) => clientGameLoop.SetActiveInput(isActive);
}
