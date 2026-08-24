using UnityEditor;
using UnityEngine;

public class CharacterAnimationTextureImporter : AssetPostprocessor {
    private const string AnimationTexturePath = "Assets/Textures/Resources/Animations/";
    private const float SpritePixelsPerUnit = 512f / 3f;

    private void OnPreprocessTexture() {
        if (!assetPath.StartsWith(AnimationTexturePath)) return;

        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = SpritePixelsPerUnit;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
    }
}