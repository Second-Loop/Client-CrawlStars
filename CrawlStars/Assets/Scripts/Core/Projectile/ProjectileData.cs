using UnityEngine;

namespace Core.Projectile {
    public class ProjectileData {
        public string Id { get; set; }
        public string OwnerId { get; set; }
        public Vector2 Pos { get; set; }
        public Vector2 Dir { get; set; }
        public float Speed { get; set; }
        public float Damage { get; set; }
        public float Radius { get; set; }
        public bool IsDestroyed { get; set; }
    }
}