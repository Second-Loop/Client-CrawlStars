using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.Map {
    public static class MapLoader {
        public enum MapType {
            A, B, C
        }

        private const string FilePath = "Maps";
        private const string FilePrefix = "Map_";
        private const string FileExtension = ".json";
        
        public static MapData LoadMapFile(MapType mapType) {
            string path = Path.Combine(Application.streamingAssetsPath, FilePath, $"{FilePrefix}{mapType}{FileExtension}");
            if (string.IsNullOrWhiteSpace(path)) {
                Debug.LogError($"MapGenerator.LoadMapFile::{mapType} 파일 경로 Combine 실패");
                return null;
            }
            
            string text = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(text)) {
                Debug.LogError($"MapGenerator.LoadMapFile::{mapType} 파일 경로 읽기 실패");
                return null;
            }

            try {
                return JsonConvert.DeserializeObject<MapData>(text);
            } catch (Exception e) { 
                Debug.LogError($"MapGenerator.LoadMapFile::{mapType} Json Deserialize 실패\n{e}");
                return null;
            }
        }
    }
}