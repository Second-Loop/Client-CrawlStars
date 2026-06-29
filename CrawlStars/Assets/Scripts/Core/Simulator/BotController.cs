using System.Collections.Generic;
using Core.Player;
using Core.Projectile;
using Cysharp.Threading.Tasks;
using Network;
using UnityEngine;

namespace Core.Simulator {
    /*
     * Explore → 적, 아이템, 목표물을 찾으며 이동
     * Chase → 적 추격
     * Attack → 적 공격
     * Dodge → 공격 회피
     * Retreat -> 도망
     */

    public class BotController {
        private static BotController instance;
        public static BotController Instance => instance ??= new BotController();
        
        private enum State { None, Explore, Chase, Attack, Dodge, Retreat }

        private State state;

        private readonly Dictionary<string, Vector2> prevProjectilePositions = new Dictionary<string, Vector2>();

        private const float AttackRange = 5f;
        private const float RetreatHpThreshold = 0.2f;
        private const float RetreatDistance = 6f;
        private const float ProjectileLookAheadDistance = 8f;
        private const float DodgeMargin = 0.35f;
        private readonly float DangerRadius = GameConfig.PlayerRadius + GameConfig.ProjectileRadius + DodgeMargin;
        private const float MinDirectionSqrMagnitude = 0.0001f;

        public void Initialize() {
            state = State.None;
            prevProjectilePositions.Clear();
        }

        public async UniTask SendInputAsync() {
            (Vector2 moveDirection, Vector2 attackDirection) = Update();

            await NetworkManager.Instance.SendSocketJsonAsync(new InputMessageDto {
                MoveDir = new Vector2Dto(moveDirection),
                AttackDir = new Vector2Dto(attackDirection),
                PressedAttack = attackDirection != Vector2.zero
            });
        }

        private (Vector2 moveDirection, Vector2 attackDirection) Update() {
            var curPlayers = PlayerManager.Instance.playerListeners;
            var curProjectiles = ProjectileManager.Instance.projectileListeners;
            var curMe = PlayerManager.Instance.myListener;

            if (curMe == null) {
                StoreProjectilePositions(curProjectiles);
                state = State.Explore;
                return (Vector2.zero, Vector2.zero);
            }

            PlayerListener target = FindNearestTarget(curPlayers, curMe);
            Vector2 targetDirection = target == null ? Vector2.zero 
                : (target.transform.position - curMe.transform.position).normalized;

            // 회피 체크
            if (TryGetDodgeDirection(curProjectiles, curMe, out Vector2 dodgeDirection)) {
                state = State.Dodge;
                StoreProjectilePositions(curProjectiles);
                return (dodgeDirection, Vector2.zero);
            }

            if (target == null) {
                state = State.Explore;
                StoreProjectilePositions(curProjectiles);
                return (Vector2.zero, Vector2.zero);
            }

            // 체력이 낮으면 가장 가까운 적 반대 방향으로 도망
            if (ShouldRetreat(curMe)) {
                state = State.Retreat;
                StoreProjectilePositions(curProjectiles);
                var retreatTarget = (Vector2)curMe.transform.position - targetDirection * RetreatDistance;
                var retreatDirection = BotPathFinder.GetMoveDirection(curMe.transform.position, retreatTarget);
                return (retreatDirection, Vector2.zero);
            }

            // 공격 범위 체크
            if (IsInAttackRange(curMe, target)) {
                state = State.Attack;
                StoreProjectilePositions(curProjectiles);
                return (Vector2.zero, targetDirection);
            }

            // 추격
            state = State.Chase;
            StoreProjectilePositions(curProjectiles);
            var moveDirection = BotPathFinder.GetMoveDirection(curMe.transform.position, target.transform.position);
            return (moveDirection, Vector2.zero);
        }

        private PlayerListener FindNearestTarget(Dictionary<string, PlayerListener> players, PlayerListener curMe) {
            PlayerListener nearest = null;
            float nearestSqrDistance = float.MaxValue;
            Vector2 myPos = curMe.transform.position;

            foreach (var player in players) {
                PlayerListener candidate = player.Value;
                if (candidate == null || candidate == curMe) continue;

                float sqrDistance = ((Vector2)candidate.transform.position - myPos).sqrMagnitude;
                if (sqrDistance >= nearestSqrDistance) continue;

                nearest = candidate;
                nearestSqrDistance = sqrDistance;
            }

            return nearest;
        }

        private bool IsInAttackRange(PlayerListener curMe, PlayerListener target) {
            float sqrDistance = ((Vector2)(target.transform.position - curMe.transform.position)).sqrMagnitude;
            return sqrDistance <= AttackRange * AttackRange;
        }

        private bool ShouldRetreat(PlayerListener curMe) {
            return curMe.Hp > 0f && curMe.Hp <= RetreatHpThreshold;
        }

        // 투사체 검사 후 회피해야 하면 회피 방향 전달
        private bool TryGetDodgeDirection(Dictionary<string, ProjectileListener> curProjectiles, PlayerListener curMe, out Vector2 dodgeDirection) {
            dodgeDirection = Vector2.zero;
            if (prevProjectilePositions.Count == 0) return false;

            Vector2 myPos = curMe.transform.position;

            foreach (var projectile in curProjectiles) {
                ProjectileListener curProjectile = projectile.Value;
                if (curProjectile == null) continue;
                if (!prevProjectilePositions.TryGetValue(projectile.Key, out Vector2 prevPos)) continue;

                Vector2 curPos = curProjectile.transform.position;
                Vector2 movement = curPos - prevPos;

                // 벡터가 0에 가까우면 움직이지 않았는데도 진짜 방향으로 체크할 수 있으므로, 오차값을 두어 방어 
                if (movement.sqrMagnitude <= MinDirectionSqrMagnitude) continue;

                Vector2 projectileDir = movement.normalized;
                Vector2 toMe = myPos - curPos;

                // 투사체 진행 방향 기준으로, 내가 앞쪽에 얼마나 떨어져 있는지 체크 (toMe를 투사체 진행 방향으로 투영한 길이)
                float forwardDistance = Vector2.Dot(toMe, projectileDir);

                // 내가 투사체 뒤에 있거나, 투사체가 너무 멀리있으면 패스
                if (forwardDistance <= 0f || forwardDistance > ProjectileLookAheadDistance) continue;

                // 투사체가 내 위치와 가장 가까워지는 경로상의 점
                Vector2 closestPoint = curPos + projectileDir * forwardDistance;

                // 투사체 경로에서 내 위치로 향하는 벡터 -> 회피 방향
                Vector2 awayFromPath = myPos - closestPoint;
                if (awayFromPath.sqrMagnitude > DangerRadius * DangerRadius) continue;

                // 내 위치가 투사체 경로와 거의 정확히 겹쳐 있다면 회피 방향으로 쓸 수 없으므로, 투사체 방향의 수직으로 결정
                if (awayFromPath.sqrMagnitude <= MinDirectionSqrMagnitude) {
                    awayFromPath = new Vector2(-projectileDir.y, projectileDir.x);
                }

                dodgeDirection += awayFromPath.normalized;
            }

            if (dodgeDirection.sqrMagnitude <= MinDirectionSqrMagnitude) return false;

            dodgeDirection.Normalize();
            return true;
        }

        // 값 복사용
        private void StoreProjectilePositions(Dictionary<string, ProjectileListener> curProjectiles) {
            prevProjectilePositions.Clear();
            foreach (var projectile in curProjectiles) {
                if (projectile.Value == null) continue;

                prevProjectilePositions[projectile.Key] = projectile.Value.transform.position;
            }
        }
    }
}