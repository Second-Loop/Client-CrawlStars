using System;
using System.IO;
using Core;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public sealed class GameConfigBuildValidator : IPreprocessBuildWithReport {
    private const string ConfigFileName = "game-config.json";

    public int callbackOrder => 100;

    public void OnPreprocessBuild(BuildReport report) {
        string path = Path.Combine(Application.streamingAssetsPath, ConfigFileName);
        if (!File.Exists(path)) {
            throw new BuildFailedException($"Game config file is missing: {path}");
        }

        string json;
        try {
            json = File.ReadAllText(path);
        } catch (Exception exception) when (
            exception is IOException || exception is UnauthorizedAccessException
        ) {
            throw new BuildFailedException($"Failed to read game config: {exception.Message}");
        }

        if (!GameConfigParser.TryParse(json, out _, out string error)) {
            throw new BuildFailedException($"Invalid game config: {error}");
        }
    }
}
