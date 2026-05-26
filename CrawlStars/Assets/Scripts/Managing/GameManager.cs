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
    [SerializeField] private NetworkBehaviour networkBehaviour;

    private static GameManager instance;
    public static GameManager Instance => instance;
    private bool socketLogHandlersRegistered;

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
        RegisterSocketLogHandlers();

        try {
            // REST API 테스트
            await TestRestApiAsync();

            // 웹소켓 테스트
            await networkBehaviour.Service.ConnectSocketAsync();
            await networkBehaviour.Service.Socket.SendJsonAsync(new { type = "PING" });
        } catch (Exception exception) {
            Debug.LogException(exception);
            return;
        }

        // 맵 인덱스
        mapRenderer.Render(0);
        
        // 시뮬레이터 가동
        simulator.Initialize();
        simulator.Activate();
    }

    private void RegisterSocketLogHandlers() {
        if (socketLogHandlersRegistered) {
            return;
        }

        var socket = networkBehaviour.Service.Socket;
        socket.Connected += () => Debug.Log("WebSocket OnOpen");
        socket.TextReceived += message => Debug.Log($"WebSocket Message: {message}");
        socket.ErrorReceived += error => Debug.LogError($"WebSocket Error: {error}");
        socket.Closed += closeCode => Debug.Log($"WebSocket Closed: {closeCode}");

        socketLogHandlersRegistered = true;
    }

    private async UniTask TestRestApiAsync() {
        HealthResponse health = await networkBehaviour.Service.Rest.GetAsync<HealthResponse>("health");
        Debug.Log($"REST Health: ok={health.Ok}, message={health.Message}");

        LoginResponse login = await networkBehaviour.Service.Rest.PostAsync<LoginRequest, LoginResponse>(
            "auth/login",
            new LoginRequest {
                Email = "test@example.com",
                Password = "password"
            }
        );

        networkBehaviour.Service.SetJwtToken(login.AccessToken);
        Debug.Log($"REST Login: userId={login.UserId}, nickname={login.Nickname}");

        UserResponse me = await networkBehaviour.Service.Rest.GetAsync<UserResponse>("users/me");
        Debug.Log($"REST UsersMe: id={me.Id}, email={me.Email}, nickname={me.Nickname}, level={me.Level}");
    }

    private sealed class HealthResponse {
        [JsonProperty("ok")] public bool Ok { get; set; }
        [JsonProperty("message")] public string Message { get; set; }
    }

    private sealed class LoginRequest {
        [JsonProperty("email")] public string Email { get; set; }
        [JsonProperty("password")] public string Password { get; set; }
    }

    private sealed class LoginResponse {
        [JsonProperty("accessToken")] public string AccessToken { get; set; }
        [JsonProperty("userId")] public int UserId { get; set; }
        [JsonProperty("nickname")] public string Nickname { get; set; }
    }

    private sealed class UserResponse {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("email")] public string Email { get; set; }
        [JsonProperty("nickname")] public string Nickname { get; set; }
        [JsonProperty("level")] public int Level { get; set; }
    }
}
