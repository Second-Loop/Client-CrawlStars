using System;
using Core.Controller;
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

    public void Initialize(ReadyEventMessageDto readyEvent) {
        mapRenderer.Render(readyEvent.Map);
        clientGameLoop.Initialize(readyEvent.Players);
        NetworkManager.Instance.GameEndReceived += HandleGameEnd;
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

    public void RegisterInputAction(Action<Vector2, Vector2> callback) => clientGameLoop.OnReceivedInput += callback;
    public void UnregisterInputAction(Action<Vector2, Vector2> callback) => clientGameLoop.OnReceivedInput -= callback;

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
