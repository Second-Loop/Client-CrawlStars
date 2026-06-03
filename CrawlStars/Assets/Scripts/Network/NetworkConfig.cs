using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Network {
    public class NetworkConfig {
        private const string ConfigFileName = "network_config.json";

        public string RestBaseUrl { get; private set; }
        public string WebSocketUrlTemplate { get; private set; }

        public static NetworkConfig Load() {
            string configPath = GetConfigPath();

            if (!File.Exists(configPath)) {
                Debug.LogError($"NetworkConfig.Load::config file not found. using local defaults. path={configPath}");
                return null;
            }

            try {
                string json = File.ReadAllText(configPath);
                var fileConfig = JsonConvert.DeserializeObject<NetworkConfigFile>(json);
                return FromFileConfig(fileConfig);
            } catch (Exception e) {
                Debug.LogError($"NetworkConfig.Load::failed to load config. using local defaults. error={e.Message}");
                return null;
            }
        }

        private static NetworkConfig FromFileConfig(NetworkConfigFile fileConfig) {
            var config = new NetworkConfig();

            if (!string.IsNullOrWhiteSpace(fileConfig?.RestBaseUrl)) {
                config.RestBaseUrl = fileConfig.RestBaseUrl.TrimEnd('/');
            }

            if (!string.IsNullOrWhiteSpace(fileConfig?.WebSocketUrlTemplate)) {
                config.WebSocketUrlTemplate = fileConfig.WebSocketUrlTemplate;
            } else if (!string.IsNullOrWhiteSpace(fileConfig?.WebSocketUrl)) {
                config.WebSocketUrlTemplate = fileConfig.WebSocketUrl;
            }

            return config;
        }

        public string GetWebSocketUrl(string roomID, string playerID) {
            return WebSocketUrlTemplate
                .Replace("{roomID}", Uri.EscapeDataString(roomID))
                .Replace("{playerID}", Uri.EscapeDataString(playerID));
        }

        private static string GetConfigPath() {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", ConfigFileName));
        }

        private sealed class NetworkConfigFile {
            [JsonProperty("restBaseUrl")]
            public string RestBaseUrl { get; set; }

            [JsonProperty("websocketUrl")]
            public string WebSocketUrl { get; set; }

            [JsonProperty("websocketUrlTemplate")]
            public string WebSocketUrlTemplate { get; set; }
        }
    }
}
