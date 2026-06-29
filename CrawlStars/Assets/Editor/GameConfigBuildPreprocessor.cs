using System;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public sealed class GameConfigBuildPreprocessor : IPreprocessBuildWithReport {
    private const string ConfigUrl =
        "https://raw.githubusercontent.com/Second-Loop/Server-CrawlStars/main/client-config/game-config.json";

    private const string OutputAssetPath = "Assets/StreamingAssets/game-config.json";

    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report) {
        try {
            string json = DownloadConfig();
            JToken.Parse(json);

            string outputPath = Path.Combine(Application.dataPath, "../", OutputAssetPath);
            File.WriteAllText(outputPath, json);
            AssetDatabase.ImportAsset(OutputAssetPath);

            Debug.Log($"Fetched latest game config: {ConfigUrl}");
        } catch (Exception e) {
            throw new BuildFailedException($"Failed to fetch game config before build. {e.Message}");
        }
    }

    private static string DownloadConfig() {
        using var client = new HttpClient();
        string json = client.GetStringAsync(ConfigUrl).GetAwaiter().GetResult();

        if (string.IsNullOrWhiteSpace(json)) {
            throw new InvalidOperationException("Downloaded game config is empty.");
        }

        return json;
    }
}
