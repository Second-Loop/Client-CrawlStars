using Core.Map;
using Core.Player;
using Core.Simulator;
using UnityEngine;

public class GameManager : MonoBehaviour {
    [SerializeField] private MapRenderer mapRenderer;
    [SerializeField] private Simulator simulator;
    
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
        Initialize();
    }

    // 서버에서 정보 받아와서 세팅
    public void Initialize() {
        // 맵 인덱스
        mapRenderer.Render(0);
        
        // 시뮬레이터 가동
        simulator.Initialize();
        simulator.Activate();
    }
}
