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

    public void Initialize() {
        mapRenderer.Render(0);
        if (clientGameLoop.Initialize()) {
            clientGameLoop.SetActive(true);
        }
    }

    public void Dispose() {
        clientGameLoop.Clear();
        mapRenderer.Clear();
        PlayerManager.Instance.ClearListeners();
        ProjectileManager.Instance.ClearListener();
        NetworkManager.Instance.DisconnectSocketAsync().Forget();
    }

    public async UniTask EndGameAsync(bool didWin) {
        clientGameLoop.SetActive(false);
        var desc = didWin ? "Win" : "Lose";
        var param = new OneButtonPopup.Param("Game End", desc);
        await PopupManager.Instance.ShowAsync("OneButtonPopup", param);
        SceneController.Instance.ChangeSceneAsync(SceneController.MainSceneName, Dispose).Forget();
    }

    public void SetActiveInput(bool isActive) => clientGameLoop.SetActiveInput(isActive);
}
