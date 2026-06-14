using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace Core.Projectile {
    public class ProjectileManager {
        private static ProjectileManager instance;
        public static ProjectileManager Instance => instance ??= new ProjectileManager();

        private Dictionary<string, ProjectileListener> projectileListeners = new Dictionary<string, ProjectileListener>();

        public void Initialize(IReadOnlyList<ProjectileData> dataList) {
            ClearListener();
        }

        public void ApplySnapshot(IReadOnlyList<ProjectileData> projectiles) {
            foreach (var projectile in projectiles) {
                if (projectile == null || string.IsNullOrEmpty(projectile.Id)) continue;

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
        }

        public void ClearListener() {
            foreach (var projectile in projectileListeners) {
                ObjectPooling.Instance.TryAbandon(Constants.Projectile, projectile.Value.gameObject);
            }
            projectileListeners.Clear();
        }
    }
}
