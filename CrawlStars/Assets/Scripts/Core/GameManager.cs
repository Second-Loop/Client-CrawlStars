using Core.Map;
using Core.Player;
using Core.Simulator;
using Cysharp.Threading.Tasks;
using Network;
using System;
using Core.Projectile;
using Managing;
using Popup;
using UnityEngine;

public class GameManager : SingletonMonoBehaviour<GameManager> {
    [SerializeField] private MapRenderer mapRenderer;
    [SerializeField] private Simulator simulator;
    [SerializeField] private NetworkManager networkManager;

    public void Initialize() {
        // 네트워크 테스트
        // await TestNetwork();

        // 맵 인덱스
        mapRenderer.Render(0);
        
        // 시뮬레이터 가동
        simulator.Initialize();
        UniTask.Delay(1000).ContinueWith(() => simulator.SetActive(true));
    }

    public void Dispose() {
        simulator.Clear();
        mapRenderer.Clear();
        PlayerManager.Instance.ClearListeners();
        ProjectileManager.Instance.ClearListener();
    }

    public async UniTask EndGameAsync(bool didWin) {
        simulator.SetActive(false);
        var desc = didWin ? "Win" : "Lose";
        var param = new OneButtonPopup.Param("Game End", desc);
        await PopupManager.Instance.ShowAsync("OneButtonPopup", param);
        SceneController.Instance.ChangeSceneAsync(SceneController.MainSceneName, Dispose).Forget();
    }

    public void SetActiveInput(bool isActive) => simulator.SetActiveInput(isActive);

    private async UniTask TestNetwork() {
        try {
            // REST API 테스트
            await networkManager.TestRestApiAsync();

            // 웹소켓 테스트
            await networkManager.ConnectSocketAsync();
            await networkManager.SendSocketJsonAsync(new { type = "PING" });
        } catch (Exception exception) {
            Debug.LogError(exception);
        }
    }
}
