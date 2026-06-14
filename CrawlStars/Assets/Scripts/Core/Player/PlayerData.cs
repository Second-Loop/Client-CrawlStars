using Newtonsoft.Json;
using Network;

namespace Core.Player {
    public class PlayerData {
        public enum PlayerType {
            
        }
        
        [JsonProperty("Id")] public string Id { get; set; }
        [JsonProperty("Team")] public string Team { get; set; }
        [JsonProperty("Slot")] public int Slot { get; set; }
        [JsonProperty("Pos")] public Vector2Dto Pos { get; set; }
        [JsonProperty("MoveDir")] public Vector2Dto MoveDir { get; set; }
        [JsonProperty("AttackDir")] public Vector2Dto AttackDir { get; set; }
        [JsonProperty("Speed")] public float Speed { get; set; }
        [JsonProperty("Radius")] public float Radius { get; set; }
        [JsonProperty("PressedAttack")] public bool PressedAttack { get; set; }
        public int Hp { get; set; }
        public PlayerType Type { get; set; }

        [JsonIgnore] public bool IsDead => Hp <= 0;
    }
}
