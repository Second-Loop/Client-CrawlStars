using System.Collections.Generic;
using Core.Controller;
using Core.Player;
using Core.Projectile;
using Cysharp.Threading.Tasks;
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
            Clear();

            var player = PlayerData.BasePlayerData;
            player.Pos = Vector2.up;
            players.Add(player);
            
            player = PlayerData.BasePlayerData;
            player.Pos = Vector2.up * 2;
            players.Add(player);
            
            player = PlayerData.BasePlayerData;
            player.Pos = Vector2.up + Vector2.right;
            players.Add(player);

            PlayerManager.Instance.Initialize(players);
        }

        public void SetActive(bool isActive) {
            isActivated = isActive;
            SetActiveInput(isActive);
        }

        public void SetActiveInput(bool isActive) {
            inputProvider.IsActivated = isActive;
        }
        
        public void Clear() {
            SetActive(false);
            accumulator = 0;
            TickCount = 0;

            players.Clear();
            projectiles.Clear();
            willRemovePlayers.Clear();
            willRemoveProjectiles.Clear();
            willAddProjectiles.Clear();
        }

        private List<PlayerData> players = new List<PlayerData>();
        private List<PlayerData> willRemovePlayers = new List<PlayerData>();

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
                var hitPlayer = Physics.MoveProjectile(projectile, players);

                // 플레이어 사망 처리
                if (hitPlayer != null && hitPlayer.IsDead) {
                    willRemovePlayers.Add(hitPlayer);
                }

                // 투사체 제거 처리
                if (projectile.IsDestroyed) {
                    willRemoveProjectiles.Add(projectile);
                }
            }

            // 4. 만들어질 투사체 생성
            foreach (var player in players) {
                if (player.IsDead || player.AttackDir == Vector2.zero) continue;

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
            PlayerManager.Instance.BeingHit(players);

            // 2. 투사체 처리
            ProjectileManager.Instance.UpdateProjectiles(projectiles);
            foreach (var newProjectile in willAddProjectiles) {
                ProjectileManager.Instance.Create(newProjectile);
            }

            // ====== Judgement Game Ending ======
            int leftPlayerCount = willRemovePlayers.Count - players.Count;

            // 모두 한 틱에 사망하여 공동 승리한 경우
            if (leftPlayerCount == 0) {
                isActivated = false;
                foreach (var winner in players) {
                    if (winner.Id == PlayerManager.Instance.MyId) {
                        GameManager.Instance.EndGameAsync(winner.Id == PlayerManager.Instance.MyId).Forget();
                    }
                }
                return;
            }

            // 사망한 플레이어 패배 처리
            foreach (var player in willRemovePlayers) {
                players.Remove(player);
                if (player.Id == PlayerManager.Instance.MyId) {
                    GameManager.Instance.EndGameAsync(false).Forget();
                    return;
                }
            }
            willRemovePlayers.Clear();

            // 최후 1인 승리 처리
            if (players.Count == 1) {
                isActivated = false;
                GameManager.Instance.EndGameAsync(players[0].Id == PlayerManager.Instance.MyId).Forget();
                return;
            }

            // ===== Post Process =====
            // 클라에게 지워졌다고 정보를 전송해야 하기 때문에 전송 이후 틱 마지막에 지움
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