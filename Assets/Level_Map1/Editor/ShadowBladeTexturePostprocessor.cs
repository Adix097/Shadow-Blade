#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ShadowBlade.LevelEditor
{
    // Applies pixel-art import settings to every PNG inside Assets/Level_Map1/Art automatically.
    public class ShadowBladeTexturePostprocessor : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/Level_Map1/Art")) return;
            var ti = (TextureImporter)assetImporter;
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 16;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;

            var s = new TextureImporterSettings();
            ti.ReadTextureSettings(s);
            s.spriteMeshType = SpriteMeshType.FullRect;
            if (assetPath.Contains("samurai"))
            {
                s.spriteAlignment = (int)SpriteAlignment.Custom;
                s.spritePivot = new Vector2(51f / 112f, 4f / 72f); // feet
            }
            else s.spriteAlignment = (int)SpriteAlignment.Center;
            ti.SetTextureSettings(s);
        }
    }
}
#endif
