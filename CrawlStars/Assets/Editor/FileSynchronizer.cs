using System;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public sealed class FileSynchronizer : IPreprocessBuildWithReport {
    private const string RepositoryUrl = "https://raw.githubusercontent.com/Second-Loop/Server-CrawlStars/main";

    private static readonly (string Url, string OutputPath)[] Files = {
        ($"{RepositoryUrl}/client-config/game-config.json", "Assets/StreamingAssets/game-config.json"),
        ($"{RepositoryUrl}/api/asyncapi.yaml", "Docs/References/API/asyncapi.yaml"),
        ($"{RepositoryUrl}/api/openapi.yaml", "Docs/References/API/openapi.yaml")
    };

    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report) {
        try {
            SynchronizeFiles();
        } catch (Exception e) {
            throw new BuildFailedException($"Failed to synchronize files before build. {e.Message}");
        }
    }

    [MenuItem("Tools/File Synchronizer/Download Latest Files")]
    private static void SynchronizeFilesFromMenu() {
        try {
            SynchronizeFiles();
            EditorUtility.DisplayDialog("File Synchronizer", $"Downloaded {Files.Length} files successfully.", "OK");
        } catch (Exception e) {
            Debug.LogException(e);
            EditorUtility.DisplayDialog("File Synchronizer", $"Failed to download files.\n{e.Message}", "OK");
        }
    }

    private static void SynchronizeFiles() {
        using var client = new HttpClient();

        foreach ((string url, string outputAssetPath) in Files) {
            string content = DownloadFile(client, url);

            if (Path.GetExtension(outputAssetPath).Equals(".json", StringComparison.OrdinalIgnoreCase)) {
                JToken.Parse(content);
            }

            string outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", outputAssetPath));
            File.WriteAllText(outputPath, content);

            if (outputAssetPath.StartsWith("Assets/", StringComparison.Ordinal)) {
                AssetDatabase.ImportAsset(outputAssetPath);
            }

            Debug.Log($"Fetched latest file: {url}");
        }
    }

    private static string DownloadFile(HttpClient client, string url) {
        string content = client.GetStringAsync(url).GetAwaiter().GetResult();

        if (string.IsNullOrWhiteSpace(content)) {
            throw new InvalidOperationException($"Downloaded file is empty: {url}");
        }

        return content;
    }
}
