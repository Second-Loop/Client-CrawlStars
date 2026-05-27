using Core.Map;
using Core.Player;
using Core.Simulator;
using Cysharp.Threading.Tasks;
using Network;
using Newtonsoft.Json;
using System;
using UnityEngine;

public class GameManager : MonoBehaviour {
    [SerializeField] private MapRenderer mapRenderer;
    [SerializeField] private Simulator simulator;
    [SerializeField] private NetworkManager networkManager;

    private static GameManager instance;
    public static GameManager Instance => instance;

    private void Awake() {
        if (instance != null) {
            Debug.LogError($"{nameof(GameManager)} Instance가 {name}에서 중복 초기화 시도되었습니다.");
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        Initialize().Forget();
    }

    public async UniTask Initialize() {
        // 네트워크 테스트
        // await TestNetwork();

        // 맵 인덱스
        mapRenderer.Render(0);
        
        // 시뮬레이터 가동
        simulator.Initialize();
        simulator.Activate();
    }
    
    public async UniTask TestNetwork() {
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
