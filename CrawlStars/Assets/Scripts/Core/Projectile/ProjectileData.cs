using System;
using UnityEngine;

namespace Core.Projectile {
    public class ProjectileData {
        public enum ProjectileType {
            
        }

        public string Id { get; set; }
        public string OwnerId { get; set; }
        public Vector2 Pos { get; set; }
        public Vector2 Dir { get; set; }
        public float Speed { get; set; }
        public float Radius { get; set; }   // 얘는 고정
        public int Damage { get; set; }
        public ProjectileType Type { get; set; }
        public bool IsDestroyed { get; set; }
        
        public static ProjectileData BaseProjectile => new ProjectileData {
            Id = Guid.NewGuid().ToString(),
            Speed = 13f,
            Damage = 100,
            Radius = 0.3f
        };
    }
}