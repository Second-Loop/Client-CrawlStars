using Core.Map;
using Core.Player;
using Core.Simulator;
using Cysharp.Threading.Tasks;
using Network;
using System;
using System.Threading;
using Core.Projectile;
using Managing;
using Popup;
using UnityEngine;

public class GameManager : SingletonMonoBehaviour<GameManager> {
    [SerializeField] private MapRenderer mapRenderer;
    [SerializeField] private Simulator simulator;
    [SerializeField] private NetworkManager networkManager;

    public void Initialize() {
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
        CancelMatch();
    }

    public async UniTask EndGameAsync(bool didWin) {
        simulator.SetActive(false);
        var desc = didWin ? "Win" : "Lose";
        var param = new OneButtonPopup.Param("Game End", desc);
        await PopupManager.Instance.ShowAsync("OneButtonPopup", param);
        SceneController.Instance.ChangeSceneAsync(SceneController.MainSceneName, Dispose).Forget();
    }

    public void SetActiveInput(bool isActive) => simulator.SetActiveInput(isActive);

    public async UniTask MatchAsync(CancellationToken ct) {
        // REST API 테스트
        NetworkTestSession session = await networkManager.TestRestApiAsync(ct);
        ct.ThrowIfCancellationRequested();

        // 웹소켓 테스트
        networkManager.ConnectSocket(session.RoomID, session.PlayerID);
        await networkManager.SendSocketJsonAsync(new InputMessage {
            MoveDir = new NetworkVector2 { X = 1f, Y = 0f },
            AttackDir = new NetworkVector2 { X = 1f, Y = 0f },
            PressedAttack = false
        });
        ct.ThrowIfCancellationRequested();
    }

    public void CancelMatch() {
        networkManager.DisconnectSocket();
    }
}
