using Newtonsoft.Json;
using UnityEngine;

namespace Network {
#region Test
    public sealed class HealthResponse {
        [JsonProperty("ok")] public bool Ok { get; set; }
        [JsonProperty("message")] public string Message { get; set; }
    }

    public sealed class LoginRequest {
        [JsonProperty("email")] public string Email { get; set; }
        [JsonProperty("password")] public string Password { get; set; }
    }

    public sealed class LoginResponse {
        [JsonProperty("accessToken")] public string AccessToken { get; set; }
        [JsonProperty("userId")] public int UserId { get; set; }
        [JsonProperty("nickname")] public string Nickname { get; set; }
    }

    public sealed class UserResponse {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("email")] public string Email { get; set; }
        [JsonProperty("nickname")] public string Nickname { get; set; }
        [JsonProperty("level")] public int Level { get; set; }
    }
#endregion
}