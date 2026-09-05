using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Tests.EditMode.Core {
    public class GameConfigParserTests {
        private static readonly FieldInfo VersionField = GetBackingField("Version");
        private static readonly FieldInfo TileSizeField = GetBackingField("TileSize");
        private static readonly FieldInfo PlayerRadiusField = GetBackingField("PlayerRadius");
        private static readonly FieldInfo NormalAttackCoolDownField = GetBackingField("NormalAttackCoolDown");
        private static readonly FieldInfo ProjectileRadiusField = GetBackingField("ProjectileRadius");

        private string validJson;
        private int previousVersion;
        private float previousTileSize;
        private float previousPlayerRadius;
        private GameConfig.PlayerConfig[] previousPlayerConfigs;
        private int previousNormalAttackCoolDown;
        private float previousProjectileRadius;

        [OneTimeSetUp]
        public void LoadActualStreamingAssetsConfig() {
            string path = FindStreamingAssetsConfig();
            validJson = File.ReadAllText(path);
        }

        [SetUp]
        public void PreserveGameConfigState() {
            previousVersion = (int)VersionField.GetValue(null);
            previousTileSize = (float)TileSizeField.GetValue(null);
            previousPlayerRadius = (float)PlayerRadiusField.GetValue(null);
            previousPlayerConfigs = GameConfig.PlayerConfigs;
            previousNormalAttackCoolDown = (int)NormalAttackCoolDownField.GetValue(null);
            previousProjectileRadius = (float)ProjectileRadiusField.GetValue(null);
        }

        [TearDown]
        public void RestoreGameConfigState() {
            VersionField.SetValue(null, previousVersion);
            TileSizeField.SetValue(null, previousTileSize);
            PlayerRadiusField.SetValue(null, previousPlayerRadius);
            GameConfig.PlayerConfigs = previousPlayerConfigs;
            NormalAttackCoolDownField.SetValue(null, previousNormalAttackCoolDown);
            ProjectileRadiusField.SetValue(null, previousProjectileRadius);
        }

        [Test]
        public void TryParse_AcceptsActualStreamingAssetsConfig() {
            bool parsed = GameConfigParser.TryParse(validJson, out var config, out string error);

            Assert.That(parsed, Is.True, error);
            Assert.That(config, Is.Not.Null);
            Assert.That(config.Version, Is.EqualTo(3));
            Assert.That(config.Characters.Select(character => character.Type), Is.EquivalentTo(new[] { 0, 1, 2 }));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("{")]
        [TestCase("null")]
        public void TryParse_RejectsMalformedEmptyOrNullJson(string json) {
            AssertRejected(json, "config");
        }

        [TestCaseSource(nameof(TopLevelRequiredProperties))]
        public void TryParse_RejectsMissingTopLevelProperty(string propertyName) {
            AssertRejected(Mutate(root => root.Remove(propertyName)), propertyName);
        }

        [TestCaseSource(nameof(TopLevelRequiredProperties))]
        public void TryParse_RejectsNullTopLevelProperty(string propertyName) {
            AssertRejected(Mutate(root => root[propertyName] = JValue.CreateNull()), propertyName);
        }

        [TestCaseSource(nameof(CharacterRequiredProperties))]
        public void TryParse_RejectsMissingCharacterProperty(string propertyName) {
            AssertRejected(Mutate(root => CharacterAt(root, 1).Remove(propertyName)), $"characters[1].{propertyName}");
        }

        [TestCaseSource(nameof(CharacterRequiredProperties))]
        public void TryParse_RejectsNullCharacterProperty(string propertyName) {
            AssertRejected(
                Mutate(root => CharacterAt(root, 1)[propertyName] = JValue.CreateNull()),
                $"characters[1].{propertyName}"
            );
        }

        [TestCase(2)]
        [TestCase(4)]
        public void TryParse_RejectsUnsupportedVersion(int version) {
            AssertRejected(Mutate(root => root["version"] = version), "version");
        }

        [TestCase("tileSize")]
        [TestCase("playerRadius")]
        [TestCase("projectileRadius")]
        public void TryParse_RejectsNonpositiveTopLevelNumber(string propertyName) {
            AssertRejected(Mutate(root => root[propertyName] = 0), propertyName);
            AssertRejected(Mutate(root => root[propertyName] = -1), propertyName);
        }

        [TestCase("normalAttackDistance")]
        [TestCase("skillAttackDistance")]
        public void TryParse_RejectsNonpositiveCharacterNumber(string propertyName) {
            AssertRejected(Mutate(root => CharacterAt(root, 0)[propertyName] = 0), $"characters[0].{propertyName}");
            AssertRejected(Mutate(root => CharacterAt(root, 0)[propertyName] = -1), $"characters[0].{propertyName}");
        }

        [Test]
        public void TryParse_RejectsNonpositiveIntegerValues() {
            AssertRejected(Mutate(root => root["normalAttackCoolDown"] = 0), "normalAttackCoolDown");
            AssertRejected(Mutate(root => CharacterAt(root, 0)["skillAttackCoolDown"] = -1), "characters[0].skillAttackCoolDown");
            AssertRejected(Mutate(root => CharacterAt(root, 2)["maxBullets"] = 0), "characters[2].maxBullets");
        }

        [Test]
        public void TryParse_RejectsFractionalIntegerFields() {
            AssertRejected(Mutate(root => root["version"] = 3.5), "version");
            AssertRejected(Mutate(root => root["normalAttackCoolDown"] = 1.5), "normalAttackCoolDown");
            AssertRejected(Mutate(root => CharacterAt(root, 0)["type"] = 0.5), "characters[0].type");
            AssertRejected(Mutate(root => CharacterAt(root, 1)["skillAttackCoolDown"] = 1.5), "characters[1].skillAttackCoolDown");
            AssertRejected(Mutate(root => CharacterAt(root, 2)["maxBullets"] = 2.5), "characters[2].maxBullets");
        }

        [TestCase("tileSize")]
        [TestCase("playerRadius")]
        [TestCase("projectileRadius")]
        public void TryParse_RejectsNonfiniteTopLevelNumber(string propertyName) {
            AssertRejected(Mutate(root => root[propertyName] = double.NaN), propertyName);
            AssertRejected(Mutate(root => root[propertyName] = double.PositiveInfinity), propertyName);
            AssertRejected(Mutate(root => root[propertyName] = double.NegativeInfinity), propertyName);
        }

        [TestCase("normalAttackDistance")]
        [TestCase("skillAttackDistance")]
        public void TryParse_RejectsNonfiniteCharacterNumber(string propertyName) {
            AssertRejected(Mutate(root => CharacterAt(root, 2)[propertyName] = double.NaN), $"characters[2].{propertyName}");
            AssertRejected(Mutate(root => CharacterAt(root, 2)[propertyName] = double.PositiveInfinity), $"characters[2].{propertyName}");
        }

        [TestCase(-1)]
        [TestCase(3)]
        public void TryParse_RejectsUnsupportedCharacterType(int type) {
            AssertRejected(Mutate(root => CharacterAt(root, 0)["type"] = type), "characters[0].type");
        }

        [Test]
        public void TryParse_RejectsDuplicateAndThereforeMissingCharacterType() {
            AssertRejected(Mutate(root => CharacterAt(root, 2)["type"] = 1), "characters[2].type");
        }

        [TestCase(2)]
        [TestCase(4)]
        public void TryParse_RejectsWrongCharacterArraySize(int size) {
            AssertRejected(Mutate(root => {
                var characters = (JArray)root["characters"];
                while (characters.Count > size) {
                    characters.RemoveAt(characters.Count - 1);
                }
                while (characters.Count < size) {
                    characters.Add(characters[0].DeepClone());
                }
            }), "characters");
        }

        [Test]
        public void TryParse_AcceptsReorderedCharacterTypes() {
            string json = Mutate(root => {
                var characters = (JArray)root["characters"];
                JToken first = characters[0];
                first.Remove();
                characters.Add(first);
            });

            bool parsed = GameConfigParser.TryParse(json, out var config, out string error);

            Assert.That(parsed, Is.True, error);
            Assert.That(config.Characters.Select(character => character.Type), Is.EqualTo(new[] { 1, 2, 0 }));
        }

        [Test]
        public void TryParse_AllowsUnknownAdditiveProperties() {
            string json = Mutate(root => {
                root["futureTopLevel"] = true;
                CharacterAt(root, 0)["futureCharacterField"] = "value";
            });

            bool parsed = GameConfigParser.TryParse(json, out var config, out string error);

            Assert.That(parsed, Is.True, error);
            Assert.That(config.Characters, Has.Length.EqualTo(3));
        }

        [Test]
        public void TryApplyJson_CommitsValidatedValuesToExistingConsumerApi() {
            string json = Mutate(root => {
                root["tileSize"] = 2.25;
                root["normalAttackCoolDown"] = 7;
                CharacterAt(root, 1)["maxBullets"] = 9;
            });

            bool applied = TryApplyJson(json, out string error);

            Assert.That(applied, Is.True, error);
            Assert.That(GameConfig.TileSize, Is.EqualTo(2.25f));
            Assert.That(GameConfig.NormalAttackCoolDown, Is.EqualTo(7));
            Assert.That(GameConfig.PlayerConfigs.Single(character => character.type == 1).maxBullets, Is.EqualTo(9));
        }

        [Test]
        public void TryApplyJson_InvalidConfigLeavesStaticStateUntouched() {
            string baseline = Mutate(root => {
                root["tileSize"] = 2.5;
                root["playerRadius"] = 0.75;
                root["normalAttackCoolDown"] = 8;
            });
            Assert.That(TryApplyJson(baseline, out string baselineError), Is.True, baselineError);
            GameConfig.PlayerConfig[] baselineCharacters = GameConfig.PlayerConfigs;

            string invalid = Mutate(root => {
                root["tileSize"] = 9.5;
                root["playerRadius"] = 0;
                root["normalAttackCoolDown"] = 99;
            });
            bool applied = TryApplyJson(invalid, out string error);

            Assert.That(applied, Is.False);
            Assert.That(error, Does.Contain("playerRadius"));
            Assert.That(GameConfig.TileSize, Is.EqualTo(2.5f));
            Assert.That(GameConfig.PlayerRadius, Is.EqualTo(0.75f));
            Assert.That(GameConfig.NormalAttackCoolDown, Is.EqualTo(8));
            Assert.That(GameConfig.PlayerConfigs, Is.SameAs(baselineCharacters));
        }

        [Test]
        public void BuildValidator_RunsAfterFileSynchronizer() {
            Assert.That(
                new GameConfigBuildValidator().callbackOrder,
                Is.GreaterThan(new FileSynchronizer().callbackOrder)
            );
        }

        private static IEnumerable<string> TopLevelRequiredProperties() {
            yield return "version";
            yield return "tileSize";
            yield return "playerRadius";
            yield return "characters";
            yield return "normalAttackCoolDown";
            yield return "projectileRadius";
        }

        private static IEnumerable<string> CharacterRequiredProperties() {
            yield return "type";
            yield return "normalAttackDistance";
            yield return "skillAttackDistance";
            yield return "skillAttackCoolDown";
            yield return "maxBullets";
        }

        private void AssertRejected(string json, string expectedErrorPath) {
            bool parsed = GameConfigParser.TryParse(json, out var config, out string error);

            Assert.That(parsed, Is.False);
            Assert.That(config, Is.Null, "invalid input must not expose a partially populated config");
            Assert.That(error, Is.Not.Null.And.Not.Empty);
            Assert.That(error, Does.Contain(expectedErrorPath));
        }

        private string Mutate(Action<JObject> mutation) {
            var root = JObject.Parse(validJson);
            mutation(root);
            return root.ToString(Formatting.None);
        }

        private static JObject CharacterAt(JObject root, int index) {
            return (JObject)root["characters"][index];
        }

        private static bool TryApplyJson(string json, out string error) {
            MethodInfo method = typeof(GameConfig).GetMethod(
                "TryApplyJson",
                BindingFlags.Static | BindingFlags.NonPublic
            );
            Assert.That(method, Is.Not.Null, "GameConfig.LoadAsync must delegate validated state application to TryApplyJson");

            object[] arguments = { json, null };
            bool result = (bool)method.Invoke(null, arguments);
            error = (string)arguments[1];
            return result;
        }

        private static FieldInfo GetBackingField(string propertyName) {
            return typeof(GameConfig).GetField(
                $"<{propertyName}>k__BackingField",
                BindingFlags.Static | BindingFlags.NonPublic
            );
        }

        private static string FindStreamingAssetsConfig() {
            string[] candidates = {
                Path.Combine(TestContext.CurrentContext.TestDirectory, "game-config.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "Assets", "StreamingAssets", "game-config.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "CrawlStars", "Assets", "StreamingAssets", "game-config.json")
            };

            string path = candidates.FirstOrDefault(File.Exists);
            Assert.That(path, Is.Not.Null, $"Unable to find actual StreamingAssets config. Checked: {string.Join(", ", candidates)}");
            return path;
        }
    }
}
