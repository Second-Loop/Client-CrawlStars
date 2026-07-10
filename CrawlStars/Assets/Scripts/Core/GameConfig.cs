using System;
using System.IO;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Core {
    public static class GameConfig {
        public class PlayerConfig {
            [JsonProperty("type")] public int type;
            [JsonProperty("normalAttackDistance")] public float normalAttackDistance;
            [JsonProperty("skillAttackDistance")] public float skillAttackDistance;
            [JsonProperty("skillAttackCoolDown")] public int skillAttackCoolDown;
            [JsonProperty("maxBullets")] public int maxBullets;
        }

        private const string ConfigFileName = "game-config.json";

        public static int Version { get; private set; }
        public static float TileSize { get; private set; }
        public static float PlayerRadius { get; private set; }
        public static PlayerConfig[] PlayerConfigs { get; set; }
        public static int NormalAttackCoolDown { get; private set; }
        public static float ProjectileRadius { get; private set; }

        public static async UniTask<bool> LoadAsync() {
            string path = Path.Combine(Application.streamingAssetsPath, ConfigFileName);
            string configUrl = path.Contains("://") ? path : new Uri(path).AbsoluteUri;

            using var request = UnityWebRequest.Get(configUrl);
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success) {
                Debug.LogError($"GameConfig.LoadAsync::failed to load config. url={configUrl}, error={request.error}");
                return false;
            }

            var config = JsonConvert.DeserializeObject<GameConfigFile>(request.downloadHandler.text);
            if (config == null) {
                Debug.LogError("GameConfig.LoadAsync::config file is empty or invalid.");
                return false;
            }

            Version = config.Version;
            TileSize = config.TileSize;
            PlayerRadius = config.PlayerRadius;
            PlayerConfigs = config.characters ?? Array.Empty<PlayerConfig>();
            NormalAttackCoolDown = config.normalAttackCoolDown;
            ProjectileRadius = config.ProjectileRadius;
            return true;
        }

        private sealed class GameConfigFile {
            [JsonProperty("version")] public int Version { get; set; }
            [JsonProperty("tileSize")] public float TileSize { get; set; }
            [JsonProperty("playerRadius")] public float PlayerRadius { get; set; }
            [JsonProperty("characters")] public PlayerConfig[] characters { get; set; }
            [JsonProperty("normalAttackCoolDown")] public int normalAttackCoolDown { get; set; }
            [JsonProperty("projectileRadius")] public float ProjectileRadius { get; set; }
        }
    }
}
