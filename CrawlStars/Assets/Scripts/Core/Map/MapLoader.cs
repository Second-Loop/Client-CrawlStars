using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.Map {
    public static class MapLoader {
        private const string FilePath = "Maps";
        private const string FilePrefix = "Map_";
        private const string FileExtension = ".json";

        public static MapData LoadMapFile(int mapIndex) {
            string path = Path.Combine(Application.streamingAssetsPath, FilePath, $"{FilePrefix}{mapIndex}{FileExtension}");
            if (string.IsNullOrWhiteSpace(path)) {
                Debug.LogError($"MapGenerator.LoadMapFile::{mapIndex} 파일 경로 Combine 실패");
                return null;
            }
            
            string text = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(text)) {
                Debug.LogError($"MapGenerator.LoadMapFile::{mapIndex} 파일 경로 읽기 실패");
                return null;
            }

            try {
                return JsonConvert.DeserializeObject<MapData>(text);
            } catch (Exception e) { 
                Debug.LogError($"MapGenerator.LoadMapFile::{mapIndex} Json Deserialize 실패\n{e}");
                return null;
            }
        }
    }
}