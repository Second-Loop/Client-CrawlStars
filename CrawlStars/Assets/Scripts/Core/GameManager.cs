using System;
using Core.Controller;
using Core.Map;
using Core.Player;
using Cysharp.Threading.Tasks;
using Network;
using Core.Projectile;
using Core.Simulator;
using Managing;
using Popup;
using UnityEngine;

public class GameManager : SingletonMonoBehaviour<GameManager> {
    [SerializeField] private MapRenderer mapRenderer;
    [SerializeField] private ClientGameLoop clientGameLoop;
    private bool isEnding;

    public bool IsBotModeActivated = false;
    public IAttackCooldownSource AttackCooldownSource => clientGameLoop.AttackCooldownSource;

    public void Initialize(ReadyEventMessageDto readyEvent) {
        MapHelper.CachedMapData = readyEvent.Map; 
        mapRenderer.Render(readyEvent.Map);

        clientGameLoop.Initialize(readyEvent.Players);
        BushVisibilityController.Instance.Initialize();

        NetworkManager.Instance.GameEndReceived += HandleGameEnd;

        if (IsBotModeActivated) {
            BotController.Instance.Initialize();
        }
    }

    public void OnEnterPlayScene() {
        PlayerManager.Instance.FocusCamera();
        NetworkManager.Instance.SendReadyAckAsync().Forget();
    }

    public void Dispose() {
        clientGameLoop.Clear();
        mapRenderer.Clear();
        PlayerManager.Instance.ClearListeners();
        ProjectileManager.Instance.ClearListener();
        NetworkManager.Instance.GameEndReceived -= HandleGameEnd;
        NetworkManager.Instance.DisconnectSocketAsync().Forget();
        isEnding = false;
    }

    public void RegisterOnSendInput(Action<Vector2, Vector2> callback) => clientGameLoop.OnSendInput += callback;
    public void UnregisterOnSendInput(Action<Vector2, Vector2> callback) => clientGameLoop.OnSendInput -= callback;
    public void RegisterOnDetectInput(Action<Vector2, bool> callback) => clientGameLoop.OnDetectInput += callback;
    public void UnregisterOnDetectInput(Action<Vector2, bool> callback) => clientGameLoop.OnDetectInput -= callback;

    public async UniTask EndGameAsync(string result) {
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
