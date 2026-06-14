using Newtonsoft.Json;
using Core.Player;
using Core.Projectile;
using UnityEngine;

namespace Network {
    public class MatchDto {
        [JsonProperty("room")] public RoomDto Room { get; set; }
        [JsonProperty("player")] public PlayerDto Player { get; set; }
        [JsonProperty("webSocketPath")] public string WebSocketPath { get; set; }
    }

    public class RoomDto {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("status")] public string Status { get; set; }
        [JsonProperty("players")] public PlayerDto[] Players { get; set; }
        [JsonProperty("maxPlayers")] public int MaxPlayers { get; set; }
        [JsonProperty("latestSnapshot")] public SnapshotDto LatestSnapshot { get; set; }
    }

    public class SnapshotMessageDto {
        [JsonProperty("Type")] public string Type { get; set; }
        [JsonProperty("Snapshot")] public SnapshotDto Snapshot { get; set; }
    }

    public class ErrorMessageDto {
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

    public class PlayerDto {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("team")] public string Team { get; set; }
        [JsonProperty("slot")] public int Slot { get; set; }
    }

    public class InputMessageDto {
        [JsonProperty("MoveDir")] public Vector2Dto MoveDir { get; set; }
        [JsonProperty("AttackDir")] public Vector2Dto AttackDir { get; set; }
        [JsonProperty("PressedAttack")] public bool PressedAttack { get; set; }
    }

    public class Vector2Dto {
        [JsonProperty("x")] public float X { get; set; }
        [JsonProperty("y")] public float Y { get; set; }

        public Vector2Dto() { }

        public Vector2Dto(Vector2 vector) {
            X = vector.x;
            Y = vector.y;
        }

        public Vector2 ToVector2() => new Vector2(X, Y);
    }

#region Test

    public sealed class HealthDto {
        [JsonProperty("status")] public string Status { get; set; }
        [JsonProperty("service")] public string Service { get; set; }
    }

    public sealed class RoomListDto {
        [JsonProperty("rooms")] public RoomDto[] Rooms { get; set; }
    }

#endregion
}
