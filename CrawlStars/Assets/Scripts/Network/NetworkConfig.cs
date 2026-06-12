using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Network {
    public class NetworkConfig {
        private const string ConfigFileName = "network_config.json";

        public string RestBaseUrl { get; private set; }
        public string WebSocketUrl { get; private set; }

        public static NetworkConfig Load() {
            string configPath = GetConfigPath();

            if (!File.Exists(configPath)) {
                Debug.LogError($"NetworkConfig.Load::config file not found. path={configPath}");
                return null;
            }

            try {
                string json = File.ReadAllText(configPath);
                var fileConfig = JsonConvert.DeserializeObject<NetworkConfigFile>(json);
                
                var config = new NetworkConfig();
                if (!string.IsNullOrWhiteSpace(fileConfig?.RestBaseUrl)) {
                    config.RestBaseUrl = fileConfig.RestBaseUrl.TrimEnd('/');
                }
                if (!string.IsNullOrWhiteSpace(fileConfig?.WebSocketUrl)) {
                    config.WebSocketUrl = fileConfig.WebSocketUrl;
                }
                return config;
            } catch (Exception e) {
                Debug.LogError($"NetworkConfig.Load::failed to load config. {e.Message}");
                return null;
            }
        }

        public string GetWebSocketUrl(string path) => $"{WebSocketUrl}{path}";

        private static string GetConfigPath() {
            return Path.Combine(Application.streamingAssetsPath, ConfigFileName);
        }

        private sealed class NetworkConfigFile {
            [JsonProperty("restBaseUrl")]
            public string RestBaseUrl { get; set; }

            [JsonProperty("websocketUrl")]
            public string WebSocketUrl { get; set; }
        }
    }
}
