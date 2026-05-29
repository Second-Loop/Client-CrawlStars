using System.Collections.Generic;
using Core.Controller;
using Core.Player;
using Core.Projectile;
using UnityEngine;

namespace Core.Simulator {
    // 나중에 클라에서도 tick 을 계산해야 하기에 서버로 이주 후에는 이름을 TickManager로 바꾸기
    public class Simulator : MonoBehaviour {
        [SerializeField] private InputProvider inputProvider;

        public int TickCount { get; private set; }
        private float accumulator;
        private bool isActivated;

        private const int TickRate = 30;
        public const float TickThreshold = 1f / TickRate;

        private void Update() {
            if (!isActivated) return;

            accumulator += Time.deltaTime;

            if (accumulator >= TickThreshold) {
                Tick();
                ++TickCount;
                accumulator -= TickThreshold;
            }
        }

        public void Initialize() {
            for (int i = 0; i < 2; ++i) {
                var player = PlayerData.BasePlayerData;
                player.Pos = Vector2.up * (i + 1) * 2;
                players.Add(player);
            }
            PlayerManager.Instance.Initialize(players);

            accumulator = 0;
            TickCount = 0;
            isActivated = false;
        }

        public void Activate() {
            isActivated = true;
        }
        
        public void Dispose() {
            isActivated = false;
            players.Clear();
        }

        private List<PlayerData> players { get; set; } = new List<PlayerData>();
        private List<ProjectileData> projectiles = new List<ProjectileData>();
        private List<ProjectileData> willRemoveProjectiles = new List<ProjectileData>();
        private List<ProjectileData> willAddProjectiles = new List<ProjectileData>();

        private void Tick() {
            // ===== Receive client's input =====
            Vector2 moveDirection = inputProvider.GetMoveDirection();
            Vector2 attackDirection = inputProvider.CaptureAttackDirection();

            foreach (var player in players) {
                player.MoveDir = moveDirection;
                player.AttackDir = attackDirection;
            }

            // ===== Simulate =====
            // 1. 플레이어 움직임
            foreach (var player in players) {
                if (player.MoveDir == Vector2.zero) continue;
                Physics.MovePlayer(player, players);
            }

            // 2. 투사체 움직임
            foreach (var projectile in projectiles) {
                Physics.MoveProjectile(projectile, players);
                if (projectile.IsDestroyed) {
                    willRemoveProjectiles.Add(projectile);
                }
            }

            // 3. 만들어질 투사체 생성
            foreach (var player in players) {
                if (player.AttackDir == Vector2.zero) continue;

                var newProjectile = ProjectileData.BaseProjectile;
                newProjectile.OwnerId = player.Id;
                newProjectile.Pos = player.Pos;
                newProjectile.Dir = attackDirection;
                willAddProjectiles.Add(newProjectile);
            }

            // ===== Send to client =====
            // 1. 플레이어 처리
            PlayerManager.Instance.Move(players);
            PlayerManager.Instance.Attack(players);

            // 2. 투사체 처리
            ProjectileManager.Instance.UpdateProjectiles(projectiles);
            foreach (var newProjectile in willAddProjectiles) {
                ProjectileManager.Instance.Create(newProjectile);
            }
            
            // ===== Post Process =====
            // 클라에게 지워졌다고 정보를 전송해야 하기 때문
            foreach (var projectile in willRemoveProjectiles) {
                projectiles.Remove(projectile);
            }
            willRemoveProjectiles.Clear();

            // 생성이 움직임보다 나중에 되기 때문
            foreach (var projectile in willAddProjectiles) {
                projectiles.Add(projectile);
            }
            willAddProjectiles.Clear();
        }
    }
}