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

            if (!TryApplyJson(request.downloadHandler.text, out string error)) {
                Debug.LogError($"GameConfig.LoadAsync::invalid config. {error}");
                return false;
            }

            return true;
        }

        private static bool TryApplyJson(string json, out string error) {
            if (!GameConfigParser.TryParse(json, out var parsed, out error)) {
                return false;
            }

            var playerConfigs = new PlayerConfig[parsed.Characters.Length];
            for (int index = 0; index < parsed.Characters.Length; ++index) {
                GameConfigParser.CharacterConfig character = parsed.Characters[index];
                playerConfigs[index] = new PlayerConfig {
                    type = character.Type,
                    normalAttackDistance = character.NormalAttackDistance,
                    skillAttackDistance = character.SkillAttackDistance,
                    skillAttackCoolDown = character.SkillAttackCoolDown,
                    maxBullets = character.MaxBullets
                };
            }

            Version = parsed.Version;
            TileSize = parsed.TileSize;
            PlayerRadius = parsed.PlayerRadius;
            PlayerConfigs = playerConfigs;
            NormalAttackCoolDown = parsed.NormalAttackCoolDown;
            ProjectileRadius = parsed.ProjectileRadius;
            return true;
        }
    }
}
