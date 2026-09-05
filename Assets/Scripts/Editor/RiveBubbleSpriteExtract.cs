#if UNITY_EDITOR
using System.IO;
using ManosLimpias.UI.Rive;
using UnityEditor;
using UnityEngine;

namespace ManosLimpias.Editor
{
    /// <summary>
    /// Pulls the in-band PNG named "bubble" out of simi_prototype.riv into a
    /// Unity Sprite, plus a particle material that samples it.
    /// </summary>
    public static class RiveBubbleSpriteExtract
    {
        public const string SpritePath = "Assets/Art/Rive/bubble.png";
        public const string MaterialPath = "Assets/Materials/SoapBubbleParticle.mat";
        const string ImageName = "bubble";

        [MenuItem("ManosLimpias/Extract Bubble Sprite")]
        public static Sprite Extract()
        {
            var rivPath = SimiPrototypeArtboards.FilePath;
            if (!File.Exists(rivPath))
            {
                Debug.LogError($"[RiveBubbleSpriteExtract] Missing {rivPath}");
                return null;
            }

            var bytes = File.ReadAllBytes(rivPath);
            if (!RiveEmbeddedPng.TryExtractNamedPng(bytes, ImageName, out var png))
            {
                Debug.LogError("[RiveBubbleSpriteExtract] No embedded PNG named 'bubble'.");
                return null;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(SpritePath));
            File.WriteAllBytes(SpritePath, png);
            AssetDatabase.ImportAsset(SpritePath, ImportAssetOptions.ForceUpdate);
            ConfigureSpriteImporter(SpritePath);
            AssetDatabase.ImportAsset(SpritePath, ImportAssetOptions.ForceUpdate);

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
            if (sprite != null)
                Debug.Log($"[RiveBubbleSpriteExtract] Wrote {SpritePath} {sprite.rect.width}x{sprite.rect.height}");
            EnsureMaterial(sprite);
            return sprite;
        }

        public static Sprite LoadOrExtract()
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
            return sprite != null ? sprite : Extract();
        }

        public static Material EnsureMaterial(Sprite sprite)
        {
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                ?? Shader.Find("Sprites/Default")
                ?? Shader.Find("Unlit/Transparent");
            if (shader == null)
            {
                Debug.LogWarning("[RiveBubbleSpriteExtract] No particle shader found.");
                return null;
            }

            var mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, MaterialPath);
            }
            else
            {
                mat.shader = shader;
            }

            if (sprite != null && sprite.texture != null)
            {
                mat.mainTexture = sprite.texture;
                if (mat.HasProperty("_BaseMap"))
                    mat.SetTexture("_BaseMap", sprite.texture);
            }

            ConfigureTransparentParticles(mat);
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            return mat;
        }

        static void ConfigureTransparentParticles(Material mat)
        {
            if (mat == null)
                return;
            if (mat.HasProperty("_Surface"))
                mat.SetFloat("_Surface", 1f);
            if (mat.HasProperty("_Blend"))
                mat.SetFloat("_Blend", 0f);
            if (mat.HasProperty("_SrcBlend"))
                mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (mat.HasProperty("_DstBlend"))
                mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            if (mat.HasProperty("_SrcBlendAlpha"))
                mat.SetFloat("_SrcBlendAlpha", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (mat.HasProperty("_DstBlendAlpha"))
                mat.SetFloat("_DstBlendAlpha", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            if (mat.HasProperty("_ZWrite"))
                mat.SetFloat("_ZWrite", 0f);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.renderQueue = 3000;
        }

        static void ConfigureSpriteImporter(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.spritePixelsPerUnit = 100f;
            importer.npotScale = TextureImporterNPOTScale.None;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            settings.spritePivot = new Vector2(0.5f, 0.5f);
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }
    }
}
#endif
