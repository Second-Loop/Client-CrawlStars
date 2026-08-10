using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace Core.Projectile {
    public class ProjectileManager {
        private static ProjectileManager instance;
        public static ProjectileManager Instance => instance ??= new ProjectileManager();

        // 임시 public
        public readonly Dictionary<string, ProjectileListener> projectileListeners = new Dictionary<string, ProjectileListener>();

        public void Initialize() {
            ClearListener();
        }

        public void ApplySnapshot(IReadOnlyList<ProjectileData> projectiles) {
            var curProjectileIds = new HashSet<string>();

            foreach (var projectile in projectiles) {
                if (projectile == null || string.IsNullOrEmpty(projectile.Id)) continue;

                curProjectileIds.Add(projectile.Id);
                if (!projectileListeners.TryGetValue(projectile.Id, out var listener)) {
                    if (projectile.IsDestroyed) continue;

                    // 살아있는데 없으면 새로 생겨난 것
                    listener = ObjectPooling.Instance.Get<ProjectileListener>(Constants.Projectile);
                    if (listener == null) continue;

                    projectileListeners.Add(projectile.Id, listener);
                }

                if (projectile.IsDestroyed) {
                    ObjectPooling.Instance.TryAbandon(Constants.Projectile, listener.gameObject);
                    projectileListeners.Remove(projectile.Id);
                    continue;
                }

                listener.MoveTo(projectile.Pos.ToVector2());
                listener.RotateTo(projectile.Dir.ToVector2());
            }

            DestroyAbsentProjectiles(curProjectileIds);
        }

        public void ClearListener() {
            foreach (var projectile in projectileListeners) {
                ObjectPooling.Instance.TryAbandon(Constants.Projectile, projectile.Value.gameObject);
            }
            projectileListeners.Clear();
        }

        // 파괴 스냅샷을 놓쳐서 지우지 못 한 투사체 제거 처리
        private void DestroyAbsentProjectiles(HashSet<string> curProjectileIds) {
            var absentProjectileIds = new List<string>();
            foreach (var projectile in projectileListeners) {
                if (curProjectileIds.Contains(projectile.Key)) continue;

                ObjectPooling.Instance.TryAbandon(Constants.Projectile, projectile.Value.gameObject);
                absentProjectileIds.Add(projectile.Key);
            }

            foreach (var projectileId in absentProjectileIds) {
                projectileListeners.Remove(projectileId);
            }
        }
    }
}
