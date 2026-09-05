using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Core {
    public static class GameConfigParser {
        public const int SupportedVersion = 3;

        public sealed class ParsedConfig {
            public int Version { get; internal set; }
            public float TileSize { get; internal set; }
            public float PlayerRadius { get; internal set; }
            public CharacterConfig[] Characters { get; internal set; }
            public int NormalAttackCoolDown { get; internal set; }
            public float ProjectileRadius { get; internal set; }
        }

        public sealed class CharacterConfig {
            public int Type { get; internal set; }
            public float NormalAttackDistance { get; internal set; }
            public float SkillAttackDistance { get; internal set; }
            public int SkillAttackCoolDown { get; internal set; }
            public int MaxBullets { get; internal set; }
        }

        public static bool TryParse(string json, out ParsedConfig config, out string error) {
            config = null;

            if (string.IsNullOrWhiteSpace(json)) {
                error = "config JSON is empty";
                return false;
            }

            JObject root;
            try {
                JToken token = JToken.Parse(json);
                root = token as JObject;
            } catch (JsonReaderException exception) {
                error = $"config JSON is invalid: {exception.Message}";
                return false;
            }

            if (root == null) {
                error = "config JSON must be an object";
                return false;
            }

            if (!TryReadInteger(root, "version", "version", out int version, out error)) {
                return false;
            }
            if (version != SupportedVersion) {
                error = $"version must be {SupportedVersion}, got {version}";
                return false;
            }
            if (!TryReadPositiveFloat(root, "tileSize", "tileSize", out float tileSize, out error)) {
                return false;
            }
            if (!TryReadPositiveFloat(root, "playerRadius", "playerRadius", out float playerRadius, out error)) {
                return false;
            }
            if (!TryReadPositiveInteger(
                    root,
                    "normalAttackCoolDown",
                    "normalAttackCoolDown",
                    out int normalAttackCoolDown,
                    out error
                )) {
                return false;
            }
            if (!TryReadPositiveFloat(
                    root,
                    "projectileRadius",
                    "projectileRadius",
                    out float projectileRadius,
                    out error
                )) {
                return false;
            }
            if (!TryReadCharacters(root, out CharacterConfig[] characters, out error)) {
                return false;
            }

            config = new ParsedConfig {
                Version = version,
                TileSize = tileSize,
                PlayerRadius = playerRadius,
                Characters = characters,
                NormalAttackCoolDown = normalAttackCoolDown,
                ProjectileRadius = projectileRadius
            };
            error = null;
            return true;
        }

        private static bool TryReadCharacters(
            JObject root,
            out CharacterConfig[] characters,
            out string error
        ) {
            characters = null;

            if (!TryGetRequiredToken(root, "characters", "characters", out JToken token, out error)) {
                return false;
            }
            if (token.Type != JTokenType.Array) {
                error = "characters must be an array";
                return false;
            }

            var array = (JArray)token;
            if (array.Count != 3) {
                error = $"characters must contain exactly 3 entries, got {array.Count}";
                return false;
            }

            var parsed = new CharacterConfig[array.Count];
            var seenTypes = new bool[3];

            for (int index = 0; index < array.Count; ++index) {
                string path = $"characters[{index}]";
                if (!(array[index] is JObject character)) {
                    error = $"{path} must be an object";
                    return false;
                }

                if (!TryReadInteger(character, "type", $"{path}.type", out int type, out error)) {
                    return false;
                }
                if (type < 0 || type > 2) {
                    error = $"{path}.type must be one of 0, 1, 2, got {type}";
                    return false;
                }
                if (seenTypes[type]) {
                    error = $"{path}.type duplicates {type}";
                    return false;
                }
                if (!TryReadPositiveFloat(
                        character,
                        "normalAttackDistance",
                        $"{path}.normalAttackDistance",
                        out float normalAttackDistance,
                        out error
                    )) {
                    return false;
                }
                if (!TryReadPositiveFloat(
                        character,
                        "skillAttackDistance",
                        $"{path}.skillAttackDistance",
                        out float skillAttackDistance,
                        out error
                    )) {
                    return false;
                }
                if (!TryReadPositiveInteger(
                        character,
                        "skillAttackCoolDown",
                        $"{path}.skillAttackCoolDown",
                        out int skillAttackCoolDown,
                        out error
                    )) {
                    return false;
                }
                if (!TryReadPositiveInteger(
                        character,
                        "maxBullets",
                        $"{path}.maxBullets",
                        out int maxBullets,
                        out error
                    )) {
                    return false;
                }

                seenTypes[type] = true;
                parsed[index] = new CharacterConfig {
                    Type = type,
                    NormalAttackDistance = normalAttackDistance,
                    SkillAttackDistance = skillAttackDistance,
                    SkillAttackCoolDown = skillAttackCoolDown,
                    MaxBullets = maxBullets
                };
            }

            characters = parsed;
            error = null;
            return true;
        }

        private static bool TryReadPositiveInteger(
            JObject parent,
            string propertyName,
            string path,
            out int value,
            out string error
        ) {
            if (!TryReadInteger(parent, propertyName, path, out value, out error)) {
                return false;
            }
            if (value <= 0) {
                error = $"{path} must be a positive integer";
                return false;
            }

            return true;
        }

        private static bool TryReadInteger(
            JObject parent,
            string propertyName,
            string path,
            out int value,
            out string error
        ) {
            value = default;

            if (!TryGetRequiredToken(parent, propertyName, path, out JToken token, out error)) {
                return false;
            }
            if (token.Type != JTokenType.Integer) {
                error = $"{path} must be an integer";
                return false;
            }

            try {
                value = token.Value<int>();
            } catch (Exception exception) when (
                exception is OverflowException || exception is FormatException || exception is InvalidCastException
            ) {
                error = $"{path} must be a 32-bit integer";
                return false;
            }

            return true;
        }

        private static bool TryReadPositiveFloat(
            JObject parent,
            string propertyName,
            string path,
            out float value,
            out string error
        ) {
            value = default;

            if (!TryGetRequiredToken(parent, propertyName, path, out JToken token, out error)) {
                return false;
            }
            if (token.Type != JTokenType.Integer && token.Type != JTokenType.Float) {
                error = $"{path} must be a number";
                return false;
            }

            double parsed;
            try {
                parsed = token.Value<double>();
            } catch (Exception exception) when (
                exception is OverflowException || exception is FormatException || exception is InvalidCastException
            ) {
                error = $"{path} must be finite and positive";
                return false;
            }

            value = (float)parsed;
            if (double.IsNaN(parsed) || double.IsInfinity(parsed) ||
                float.IsNaN(value) || float.IsInfinity(value) || value <= 0f) {
                error = $"{path} must be finite and positive";
                return false;
            }

            return true;
        }

        private static bool TryGetRequiredToken(
            JObject parent,
            string propertyName,
            string path,
            out JToken token,
            out string error
        ) {
            if (!parent.TryGetValue(propertyName, out token) || token.Type == JTokenType.Null) {
                error = $"{path} is required";
                return false;
            }

            error = null;
            return true;
        }
    }
}
