using System;
using System.IO;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Core {
    public static class GameConfig {
        private const string ConfigFileName = "game-config.json";

        public static int Version { get; private set; }
        public static float TileSize { get; private set; }
        public static float PlayerRadius { get; private set; }
        public static string[] PlayerTypes { get; private set; } = Array.Empty<string>();
        public static float ProjectileRadius { get; private set; }
        public static string[] ProjectileTypes { get; private set; } = Array.Empty<string>();

        public static async UniTask LoadAsync() {
            string path = Path.Combine(Application.streamingAssetsPath, ConfigFileName);
            string configUrl = path.Contains("://") ? path : new Uri(path).AbsoluteUri;

            try {
                using var request = UnityWebRequest.Get(configUrl);
                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success) {
                    Debug.LogError($"GameConfig.LoadAsync::failed to load config. url={configUrl}, error={request.error}");
                    return;
                }

                var config = JsonConvert.DeserializeObject<GameConfigFile>(request.downloadHandler.text);
                if (config == null) {
                    Debug.LogError("GameConfig.LoadAsync::config file is empty or invalid.");
                    return;
                }

                Version = config.Version;
                TileSize = config.TileSize;
                PlayerRadius = config.PlayerRadius;
                PlayerTypes = config.PlayerTypes ?? Array.Empty<string>();
                ProjectileRadius = config.ProjectileRadius;
                ProjectileTypes = config.ProjectileTypes ?? Array.Empty<string>();
            } catch (Exception e) {
                Debug.LogError($"GameConfig.LoadAsync::failed to load config. {e.Message}");
            }
        }

        private sealed class GameConfigFile {
            [JsonProperty("version")] public int Version { get; set; }
            [JsonProperty("tileSize")] public float TileSize { get; set; }
            [JsonProperty("playerRadius")] public float PlayerRadius { get; set; }
            [JsonProperty("playerTypes")] public string[] PlayerTypes { get; set; }
            [JsonProperty("projectileRadius")] public float ProjectileRadius { get; set; }
            [JsonProperty("projectileTypes")] public string[] ProjectileTypes { get; set; }
        }
    }
}
