using Newtonsoft.Json;
using Network;

namespace Core.Player {
    public class PlayerData {
        [JsonProperty("Id")] public string Id { get; set; }
        [JsonProperty("Team")] public string Team { get; set; }
        [JsonProperty("Slot")] public int Slot { get; set; }
        [JsonProperty("Pos")] public Vector2Dto Pos { get; set; }
        [JsonProperty("MoveDir")] public Vector2Dto MoveDir { get; set; }
        [JsonProperty("AttackDir")] public Vector2Dto AttackDir { get; set; }
        [JsonProperty("Speed")] public float Speed { get; set; }
        [JsonProperty("Radius")] public float Radius { get; set; }
        [JsonProperty("HP")] public float Hp { get; set; }
        [JsonProperty("PressedAttack")] public bool PressedAttack { get; set; }
        [JsonProperty("IsDead")] public bool IsDead { get; set; }
        [JsonProperty("CharacterType")] public int CharacterType { get; set; }
    }
}
