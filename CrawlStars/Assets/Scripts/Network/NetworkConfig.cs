using System;
using System.IO;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Network {
    public class NetworkConfig {
        private const string ConfigFileName = "network_config.json";

        public string RestBaseUrl { get; private set; }
        public string WebSocketUrl { get; private set; }

        public static async UniTask<NetworkConfig> LoadAsync() {
            string configUrl = GetConfigUrl();
            try {
                using var request = UnityWebRequest.Get(configUrl);
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success) {
                    Debug.LogError($"NetworkConfig.LoadAsync::failed to load config. url={configUrl}, error={request.error}");
                    return null;
                }

                string json = request.downloadHandler.text;
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
                Debug.LogError($"NetworkConfig.LoadAsync::failed to load config. {e.Message}");
                return null;
            }
        }

        public string GetWebSocketUrl(string path) => $"{WebSocketUrl}{path}";

        private static string GetConfigUrl() {
            string path = Path.Combine(Application.streamingAssetsPath, ConfigFileName);
            return path.Contains("://") ? path : new Uri(path).AbsoluteUri;
        }

        private sealed class NetworkConfigFile {
            [JsonProperty("restBaseUrl")]
            public string RestBaseUrl { get; set; }

            [JsonProperty("websocketUrl")]
            public string WebSocketUrl { get; set; }
        }
    }
}
