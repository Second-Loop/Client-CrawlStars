using Core.Map;
using Newtonsoft.Json;
using Core.Player;
using Core.Projectile;
using UnityEngine;

namespace Network {
    public class MatchDto {
        [JsonProperty("gameMode")] public string GameMode { get; set; }
        [JsonProperty("room")] public RoomDto Room { get; set; }
        [JsonProperty("player")] public PlayerDto Player { get; set; }
        [JsonProperty("sessionToken")] public string SessionToken { get; set; }
        [JsonProperty("webSocketPath")] public string WebSocketPath { get; set; }
    }

    public class MatchmakingJoinRequestDto {
        [JsonProperty("gameMode")] public string GameMode { get; set; }
    }

    public class RoomDto {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("gameMode")] public string GameMode { get; set; }
        [JsonProperty("status")] public string Status { get; set; }
        [JsonProperty("players")] public PlayerDto[] Players { get; set; }
        [JsonProperty("maxPlayers")] public int MaxPlayers { get; set; }
    }

    // GC Alloc 감소를 위해 서버에서 수신하는 WebSocket 메시지를 한 번의 역직렬화로 분기하기 위한 통합 DTO
    public sealed class SocketMessageDto {
        [JsonProperty("Type")] public string Type { get; set; }

        // Ready
        [JsonProperty("Map")] public MapData Map { get; set; }
        [JsonProperty("Players")] public ReadyPlayerDto[] ReadyPlayers { get; set; }

        // snapshot
        [JsonProperty("Snapshot")] public SnapshotDto Snapshot { get; set; }

        // GameEnd
        [JsonProperty("PlayerId")] public string PlayerId { get; set; }
        [JsonProperty("Result")] public string Result { get; set; }

        // error
        [JsonProperty("Error")] public ApiErrorDto Error { get; set; }
    }

    public class ReadyEventMessageDto {
        [JsonProperty("Type")] public string Type { get; set; }
        [JsonProperty("Map")] public MapData Map { get; set; }
        [JsonProperty("Players")] public ReadyPlayerDto[] Players { get; set; }
    }

    public class ReadyPlayerDto {
        [JsonProperty("Id")] public string Id { get; set; }
        [JsonProperty("Team")] public string Team { get; set; }
        [JsonProperty("Slot")] public int Slot { get; set; }
        [JsonProperty("IsBot")] public bool IsBot { get; set; }
        [JsonProperty("SpawnPosition")] public Vector2Dto SpawnPosition { get; set; }
        [JsonProperty("CharacterType")] public int CharacterType { get; set; }
    }

    public class ReadyAckMessageDto {
        [JsonProperty("Type")] public string Type => "ready";
    }

    public class SnapshotMessageDto {
        [JsonProperty("Type")] public string Type { get; set; }
        [JsonProperty("Snapshot")] public SnapshotDto Snapshot { get; set; }
    }

    public class GameEndMessageDto {
        [JsonProperty("Type")] public string Type { get; set; }
        [JsonProperty("PlayerId")] public string PlayerId { get; set; }
        [JsonProperty("Result")] public string Result { get; set; }
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
        [JsonProperty("status")] public string Status { get; set; }
        [JsonProperty("countdown")] public int? Countdown { get; set; }
        [JsonProperty("Tick")] public int Tick { get; set; }
        [JsonProperty("Players")] public PlayerData[] Players { get; set; }
        [JsonProperty("Projectiles")] public ProjectileData[] Projectiles { get; set; }
    }

    public class PlayerDto {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("team")] public string Team { get; set; }
        [JsonProperty("isBot")] public bool IsBot { get; set; }
        [JsonProperty("slot")] public int Slot { get; set; } // 순서
    }

    public class InputMessageDto {
        [JsonProperty("ClientTick")] public long ClientTick { get; set; }
        [JsonProperty("MoveDir")] public Vector2Dto MoveDir { get; set; }
        [JsonProperty("AttackDir")] public Vector2Dto AttackDir { get; set; }
        [JsonProperty("PressedAttack")] public bool PressedAttack { get; set; }
    }

    public struct Vector2Dto {
        [JsonProperty("x")] public float X { get; set; }
        [JsonProperty("y")] public float Y { get; set; }

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
