using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace Core.Projectile {
    public class ProjectileManager {
        private static ProjectileManager instance;
        public static ProjectileManager Instance => instance ??= new ProjectileManager();

        private Dictionary<string, ProjectileListener> projectiles = new Dictionary<string, ProjectileListener>();

        public void UpdateProjectiles(List<ProjectileData> dataList) {
            var willRemoveList = new List<ProjectileData>();

            foreach (var data in dataList) {
                if (!projectiles.TryGetValue(data.Id, out var projectile)!) {
                    Debug.LogError($"ProjectileManager.UpdateProjectiles::Can not find {data.Id} from projectiles");
                    continue;
                }

                if (data.IsDestroyed) {
                    ObjectPooling.Instance.TryAbandon("Projectile", projectile.gameObject);
                    willRemoveList.Add(data);
                } else {
                    projectile.MoveTo(data.Pos);
                }
            }

            foreach (var data in willRemoveList) {
                projectiles.Remove(data.Id);
            }
        }

        public void Create(ProjectileData data) {
            if (data == null) {
                Debug.LogError($"ProjectileManager.Create::data is null");
                return;
            }

            var projectile = ObjectPooling.Instance.Get<ProjectileListener>("Projectile");
            if (projectile == null) return;

            projectile.MoveTo(data.Pos);
            projectile.RotateTo(data.Dir);
            
            projectiles.Add(data.Id, projectile);
        }
    }
}