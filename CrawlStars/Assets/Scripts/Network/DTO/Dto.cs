using Newtonsoft.Json;
using Core.Player;
using Core.Projectile;
using UnityEngine;

namespace Network {
    public class MatchResponse {
        [JsonProperty("room")] public RoomResponse Room { get; set; }
        [JsonProperty("player")] public PlayerResponse Player { get; set; }
        [JsonProperty("webSocketPath")] public string WebSocketPath { get; set; }
    }

    public class RoomResponse {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("status")] public string Status { get; set; }
        [JsonProperty("players")] public PlayerResponse[] Players { get; set; }
        [JsonProperty("maxPlayers")] public int MaxPlayers { get; set; }
        [JsonProperty("latestSnapshot")] public SnapshotDto LatestSnapshot { get; set; }
    }

    public class SnapshotMessage {
        [JsonProperty("Type")] public string Type { get; set; }
        [JsonProperty("Snapshot")] public SnapshotDto Snapshot { get; set; }
    }

    public class ErrorMessage {
        [JsonProperty("Type")] public string Type { get; set; }
        [JsonProperty("Error")] public ApiErrorDto Error { get; set; }
    }

    public class ApiErrorDto {
        [JsonProperty("code")] public string Code { get; set; }
        [JsonProperty("message")] public string Message { get; set; }
    }

    public class SnapshotDto {
        [JsonProperty("Tick")] public int Tick { get; set; }
        [JsonProperty("Players")] public PlayerData[] Players { get; set; }
        [JsonProperty("Projectiles")] public ProjectileData[] Projectiles { get; set; }
    }

    public class PlayerResponse {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("team")] public string Team { get; set; }
        [JsonProperty("slot")] public int Slot { get; set; }
    }

    public class InputMessage {
        [JsonProperty("MoveDir")] public Vector2Dto MoveDir { get; set; }
        [JsonProperty("AttackDir")] public Vector2Dto AttackDir { get; set; }
        [JsonProperty("PressedAttack")] public bool PressedAttack { get; set; }
    }

    public class Vector2Dto {
        [JsonProperty("x")] public float X { get; set; }
        [JsonProperty("y")] public float Y { get; set; }

        public Vector2Dto() {
        }

        public Vector2Dto(UnityEngine.Vector2 vector) {
            X = vector.x;
            Y = vector.y;
        }

        public Vector2 ToVector2() => new(X, Y);
    }

#region Test

    public readonly struct NetworkTestSession {
        public readonly string RoomID;
        public readonly string PlayerID;

        public NetworkTestSession(string roomID, string playerID) {
            RoomID = roomID;
            PlayerID = playerID;
        }
    }

    public sealed class HealthResponse {
        [JsonProperty("status")] public string Status { get; set; }
        [JsonProperty("service")] public string Service { get; set; }
    }

    public sealed class RoomListResponse {
        [JsonProperty("rooms")] public RoomResponse[] Rooms { get; set; }
    }

#endregion
}
