using System.Collections.Generic;
using Core.Map;
using Core.Player;
using Core.Projectile;
using Cysharp.Threading.Tasks;
using Network;
using UnityEngine;

namespace Core.Simulator {
    public class BotController {
        private static BotController instance;
        public static BotController Instance => instance ??= new BotController();

        private readonly Dictionary<string, Vector2> prevProjectilePositions = new Dictionary<string, Vector2>();
        private static readonly Vector2Int InvalidPathTile = new Vector2Int(int.MinValue, int.MinValue);
        private Vector2Int cachedPathStart = InvalidPathTile;
        private Vector2Int cachedPathGoal = InvalidPathTile;
        private Vector2 cachedMoveDirection;
        private Vector2 exploreDestination;
        private bool hasExploreDestination;
        private float lastAttackTime;

        private const float DetectionRange = 15f;
        private const float AttackInterval = 0.3f;
        private const float ExploreArrivalDistance = 0.25f;
        private const int ExploreDestinationAttempts = 20;
        private const float RetreatHpThreshold = 0.2f;
        private const float RetreatDistance = 6f;
        private const float ProjectileLookAheadDistance = 8f;
        private const float DodgeMargin = 0.35f;
        private readonly float DangerRadius = GameConfig.PlayerRadius + GameConfig.ProjectileRadius + DodgeMargin;
        private const float MinDirectionSqrMagnitude = 0.0001f;

        public void Initialize() {
            prevProjectilePositions.Clear();
            ClearCachedPath();
            hasExploreDestination = false;
            lastAttackTime = -AttackInterval;
        }

        public async UniTask SendInputAsync(AttackManager attackManager, System.Action<Vector2, Vector2> onSendInput) {
            (Vector2 moveDirection, Vector2 attackDirection) = Update(attackManager);
            bool usedSkill = false;

            // 쿨타임 체크
            if (attackDirection != Vector2.zero) {
                if (attackManager.TrySkillAttack()) {
                    usedSkill = true;
                } else if (!attackManager.TryNormalAttack()) {
                    attackDirection = Vector2.zero;
                }
            }

            onSendInput?.Invoke(moveDirection, attackDirection);

            // 추후 usedSkill 보내기
            await NetworkManager.Instance.SendSocketJsonAsync(new InputMessageDto {
                MoveDir = new Vector2Dto(moveDirection),
                AttackDir = new Vector2Dto(attackDirection),
                PressedAttack = attackDirection != Vector2.zero
            });
        }

        private (Vector2 moveDirection, Vector2 attackDirection) Update(AttackManager attackManager) {
            var curPlayers = PlayerManager.Instance.playerListeners;
            var curProjectiles = ProjectileManager.Instance.projectileListeners;
            var curMe = PlayerManager.Instance.MyListener;

            if (curMe == null) {
                StoreProjectilePositions(curProjectiles);
                return (Vector2.zero, Vector2.zero);
            }

            PlayerListener target = FindNearestTarget(curPlayers, curMe);
            Vector2 targetDirection = target == null ? Vector2.zero 
                : (target.transform.position - curMe.transform.position).normalized;
            if (target != null) hasExploreDestination = false;

            Vector2 moveDirection = Vector2.zero;
            Vector2 attackDirection = Vector2.zero;

            // 회피 체크
            if (TryGetDodgeDirection(curProjectiles, curMe, out Vector2 dodgeDirection)) {
                moveDirection = dodgeDirection;
            }
            // 주변에 적이 없으면 탐색
            else if (target == null) {
                moveDirection = GetExploreDirection(curMe.transform.position);
            }
            // 체력이 낮으면 가장 가까운 적 반대 방향으로 도망
            else if (ShouldRetreat(curMe)) {
                var retreatTarget = (Vector2)curMe.transform.position - targetDirection * RetreatDistance;
                var retreatDirection = GetCachedMoveDirection(curMe.transform.position, retreatTarget);
                moveDirection = retreatDirection;
            }
            // 적이 근처에 있으면 추격
            else {
                var chaseDirection = GetCachedMoveDirection(curMe.transform.position, target.transform.position);
                moveDirection =  chaseDirection;
            }

            // 공격 범위 체크
            if (target != null && IsInAttackRange(curMe, target, attackManager) && CanAttack()) {
                attackDirection = targetDirection;
                lastAttackTime = Time.time;
            }

            StoreProjectilePositions(curProjectiles);
            return (moveDirection, attackDirection);
        }

        private Vector2 GetExploreDirection(Vector2 fromWorld) {
            MapData mapData = MapHelper.CachedMapData;
            if (mapData == null) return Vector2.zero;

            if (!hasExploreDestination ||
                (exploreDestination - fromWorld).sqrMagnitude <= ExploreArrivalDistance * ExploreArrivalDistance) {
                hasExploreDestination = TrySetExploreDestination(mapData);
            }

            return hasExploreDestination
                ? GetCachedMoveDirection(fromWorld, exploreDestination)
                : Vector2.zero;
        }

        private bool TrySetExploreDestination(MapData mapData) {
            for (int i = 0; i < ExploreDestinationAttempts; ++i) {
                int x = Random.Range(0, mapData.width);
                int y = Random.Range(0, mapData.height);
                if (MapHelper.IsPathBlockedTile(x, y)) continue;

                exploreDestination = MapHelper.GetWorldPos(x, y);
                return true;
            }

            return false;
        }

        private Vector2 GetCachedMoveDirection(Vector2 fromWorld, Vector2 toWorld) {
            if (MapHelper.CachedMapData == null) {
                return BotPathFinder.GetMoveDirection(fromWorld, toWorld);
            }

            Vector2Int start = MapHelper.GetMapIdx(fromWorld);
            Vector2Int goal = MapHelper.GetMapIdx(toWorld);
            if (start == goal) {
                return BotPathFinder.GetMoveDirection(fromWorld, toWorld);
            }

            if (start == cachedPathStart && goal == cachedPathGoal) {
                return cachedMoveDirection;
            }

            cachedPathStart = start;
            cachedPathGoal = goal;
            cachedMoveDirection = BotPathFinder.GetMoveDirection(fromWorld, toWorld);
            return cachedMoveDirection;
        }

        private void ClearCachedPath() {
            cachedPathStart = InvalidPathTile;
            cachedPathGoal = InvalidPathTile;
            cachedMoveDirection = Vector2.zero;
        }

        private PlayerListener FindNearestTarget(Dictionary<string, PlayerListener> players, PlayerListener curMe) {
            PlayerListener nearest = null;
            float nearestSqrDistance = DetectionRange * DetectionRange;
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

        private bool IsInAttackRange(PlayerListener curMe, PlayerListener target, AttackManager attackManager) {
            CharacterInfo.Definition character = CharacterManager.Instance.MyCharacter;
            if (character == null) return false;

            float attackDistance = attackManager.IsSkillCharged
                ? character.skillAttackDistance
                : character.normalAttackDistance;
            float sqrDistance = ((Vector2)(target.transform.position - curMe.transform.position)).sqrMagnitude;
            return sqrDistance <= attackDistance * attackDistance;
        }

        private bool ShouldRetreat(PlayerListener curMe) {
            return curMe.Hp > 0f && curMe.Hp <= RetreatHpThreshold;
        }

        private bool CanAttack() {
            return Time.time - lastAttackTime >= AttackInterval;
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
