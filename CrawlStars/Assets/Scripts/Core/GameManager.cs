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
        TestNetwork().Forget();

        // 맵 인덱스
        mapRenderer.Render(0);
        
        // 시뮬레이터 가동
        simulator.Initialize();
        UniTask.Delay(1000).ContinueWith(() => simulator.Activate());
    }

    public void Dispose() {
        simulator.Clear();
        mapRenderer.Clear();
        PlayerManager.Instance.ClearListeners();
        ProjectileManager.Instance.ClearListener();
    }

    public async UniTask EndGameAsync(bool didWin) {
        simulator.Deactivate();
        var desc = didWin ? "Win" : "Lose";
        var param = new OneButtonPopup.Param("Game End", desc);
        await PopupManager.Instance.ShowAsync("OneButtonPopup", param);
        SceneController.Instance.ChangeSceneAsync(SceneController.MainSceneName, Dispose).Forget();
    }

    private async UniTask TestNetwork() {
        try {
            // REST API 테스트
            NetworkTestSession session = await networkManager.TestRestApiAsync();

            // 웹소켓 테스트
            await networkManager.ConnectSocketAsync(session.RoomID, session.PlayerID);
            await networkManager.SendSocketJsonAsync(new InputMessage {
                MoveDir = new NetworkVector2 { X = 1f, Y = 0f },
                AttackDir = new NetworkVector2 { X = 1f, Y = 0f },
                PressedAttack = false
            });
        } catch (Exception exception) {
            Debug.LogError(exception);
        }
    }
}
