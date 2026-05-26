using System;
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
        private bool isStarted;

        private const int TickRate = 30;
        private const float TickThreshold = 1f / TickRate;

        private void Update() {
            if (!isStarted) return;

            accumulator += Time.deltaTime;

            if (accumulator >= TickThreshold) {
                Tick();
                ++TickCount;
                accumulator -= TickThreshold;
            }
        }

        public void Initialize() {
            accumulator = 0;
            TickCount = 0;
            isStarted = false;
        }

        public void StartTime() {
            isStarted = true;
        }

        private List<ProjectileData> projectiles = new List<ProjectileData>();
        private List<ProjectileData> willRemoveProjectiles = new List<ProjectileData>();
        private List<ProjectileData> willAddProjectiles = new List<ProjectileData>();

        // temp data
        private Vector2 playerPos = Vector2.up;

        private ProjectileData BaseProjectile => new ProjectileData {
            Pos = playerPos,
            Speed = 13f,
            Damage = 10f,
            Radius = 0.3f
        };
        private const float PlayerMoveSpeed = 2f;
        private const float PlayerRadius = 0.5f;

        private void Tick() {
            // ===== Receive client's input =====
            Vector2 moveDirection = inputProvider.GetMoveDirection();
            Vector2 attackDirection = inputProvider.CaptureAttackDirection();

            // ===== Simulate =====
            // 1. 플레이어 움직임
            if (moveDirection != Vector2.zero) {
                Vector2 movement = PlayerMoveSpeed * TickThreshold * moveDirection;
                playerPos = Physics.GetNextPlayerPos(playerPos, movement, PlayerRadius);
            }

            // 2. 투사체 움직임
            foreach (var projectile in projectiles) {
                Vector2 movement = projectile.Speed * TickThreshold * projectile.Dir;
                (Vector2 nextPos, bool hitWall) res 
                    = Physics.SimulateProjectile(projectile.Pos, movement, projectile.Radius);

                projectile.Pos = res.nextPos;
                if (res.hitWall) {
                    projectile.IsDestroyed = true;
                    willRemoveProjectiles.Add(projectile);
                }
            }

            // 3. 만들어질 투사체 생성
            ProjectileData newProjectile = null; 
            if (attackDirection != Vector2.zero) {
                newProjectile = BaseProjectile;
                newProjectile.Dir = attackDirection;
                newProjectile.Id = Guid.NewGuid().ToString();
                willAddProjectiles.Add(newProjectile);
            }

            // ===== Send to client =====
            // 1. 플레이어 처리
            PlayerManager.Instance.Move(playerPos);
            PlayerManager.Instance.Rotate(moveDirection);
            if (attackDirection != Vector2.zero) {
                PlayerManager.Instance.Attack(playerPos, attackDirection);
            }

            // 2. 투사체 처리
            ProjectileManager.Instance.UpdateProjectiles(projectiles);
            if (newProjectile != null) {
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