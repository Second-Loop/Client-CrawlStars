using Network;
using Newtonsoft.Json;

namespace Core.Projectile {
    public class ProjectileData {
        public enum ProjectileType {
            
        }

        [JsonProperty("Id")] public string Id { get; set; }
        [JsonProperty("OwnerId")] public string OwnerId { get; set; }
        [JsonProperty("Pos")] public Vector2Dto Pos { get; set; }
        [JsonProperty("Dir")] public Vector2Dto Dir { get; set; }
        [JsonProperty("Speed")] public float Speed { get; set; }
        [JsonProperty("Damage")] public float Damage { get; set; }
        [JsonProperty("Radius")] public float Radius { get; set; }
        [JsonProperty("Type")] public string Type { get; set; }
        [JsonProperty("IsDestroyed")] public bool IsDestroyed { get; set; }
    }
}
