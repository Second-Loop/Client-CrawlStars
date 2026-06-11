using Newtonsoft.Json;

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
    [JsonProperty("latestSnapshot")] public SnapshotResponse LatestSnapshot { get; set; }
}

public class SnapshotResponse {
    [JsonProperty("tick")] public int Tick { get; set; }
    [JsonProperty("playerCount")] public int PlayerCount { get; set; }
    [JsonProperty("projectileCount")] public int ProjectileCount { get; set; }
}

public class PlayerResponse {
    [JsonProperty("id")] public string Id { get; set; }
    [JsonProperty("team")] public string Team { get; set; }
    [JsonProperty("slot")] public int Slot { get; set; }
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

    public sealed class InputMessage {
        [JsonProperty("MoveDir")] public NetworkVector2 MoveDir { get; set; }
        [JsonProperty("AttackDir")] public NetworkVector2 AttackDir { get; set; }
        [JsonProperty("PressedAttack")] public bool PressedAttack { get; set; }
    }

    public sealed class NetworkVector2 {
        [JsonProperty("x")] public float X { get; set; }
        [JsonProperty("y")] public float Y { get; set; }
    }
#endregion
}
