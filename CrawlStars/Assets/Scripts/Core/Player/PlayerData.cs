using System;
using UnityEngine;

namespace Core.Player {
    public class PlayerData {
        public enum PlayerType {
            
        }
        
        public string Id { get; set; }
        public Vector2 Pos { get; set; }
        public Vector2 Dir { get; set; }
        public float Speed { get; set; }
        public float Radius { get; set; }
        public PlayerType Type { get; set; }
        public bool PressedAttack { get; set; }
        public bool IsDead { get; set; }

        public static PlayerData BasePlayerData => new PlayerData {
            Id = Guid.NewGuid().ToString(),
            Speed = 2f,
            Radius = 0.5f,
            PressedAttack = false,
            IsDead = false,
        };
    }
}