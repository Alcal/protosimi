#if UNITY_EDITOR
using System.IO;
using ManosLimpias.UI.Rive;
using UnityEditor;
using UnityEngine;

namespace ManosLimpias.Editor
{
    /// <summary>
    /// Creates the drip emitter prefab with ParticleSystem values baked at full wetness.
    /// </summary>
    public static class RiveDripEmitterPrefab
    {
        const int CircleSize = 32;

        [MenuItem("ManosLimpias/Create Rive Drip Emitter Prefab")]
        public static GameObject CreateOrUpdate()
        {
            var texture = EnsureCircleTexture();
            var material = EnsureParticleMaterial(texture);

            var go = new GameObject("RiveDripEmitter", typeof(RectTransform), typeof(ParticleSystem));
            go.SetActive(false);
            try
            {
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(24f, 8f);
                rt.localScale = Vector3.one;

                var ps = go.GetComponent<ParticleSystem>();
                BakeParticleSystem(ps, material);

                var emitter = go.AddComponent<RiveDripEmitter>();
                emitter.BakeFromParticleSystem();

                Directory.CreateDirectory(Path.GetDirectoryName(RiveDripEmitter.PrefabAssetPath));
                var prefab = PrefabUtility.SaveAsPrefabAsset(go, RiveDripEmitter.PrefabAssetPath);
                var prefabSo = new SerializedObject(prefab);
                prefabSo.FindProperty("m_IsActive").boolValue = true;
                prefabSo.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.SaveAssets();
                Debug.Log($"[RiveDripEmitterPrefab] Wrote {RiveDripEmitter.PrefabAssetPath}");
                return prefab;
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        public static GameObject LoadOrCreate()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(RiveDripEmitter.PrefabAssetPath);
            return existing != null ? existing : CreateOrUpdate();
        }

        static void BakeParticleSystem(ParticleSystem ps, Material material)
        {
            if (ps.isPlaying)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = true;
            main.duration = 1f;
            main.startLifetime = 0.7f;
            main.startSpeed = 0f;
            main.startSize = 14f;
            main.startColor = new Color(0.55f, 0.85f, 1f, 0.85f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.maxParticles = 64;
            main.gravityModifier = 0.45f;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 10f;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(24f, 8f, 1f);

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.World;
            vel.x = new ParticleSystem.MinMaxCurve(-12f, 12f);
            vel.y = new ParticleSystem.MinMaxCurve(-80f, -32f);
            vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            var colorOver = ps.colorOverLifetime;
            colorOver.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOver.color = gradient;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.alignment = ParticleSystemRenderSpace.View;
            renderer.sharedMaterial = material;
        }

        static Texture2D EnsureCircleTexture()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(RiveDripEmitter.CircleTexturePath);
            if (existing != null)
                return existing;

            var tex = new Texture2D(CircleSize, CircleSize, TextureFormat.RGBA32, false)
            {
                name = "drip-circle",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                alphaIsTransparency = true
            };

            var pixels = new Color[CircleSize * CircleSize];
            float center = (CircleSize - 1) * 0.5f;
            for (int y = 0; y < CircleSize; y++)
            {
                for (int x = 0; x < CircleSize; x++)
                {
                    float dx = (x - center) / center;
                    float dy = (y - center) / center;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.SmoothStep(1f, 0f, Mathf.Clamp01(r));
                    pixels[y * CircleSize + x] = new Color(1f, 1f, 1f, a);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(false, false);

            Directory.CreateDirectory(Path.GetDirectoryName(RiveDripEmitter.CircleTexturePath));
            File.WriteAllBytes(RiveDripEmitter.CircleTexturePath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(RiveDripEmitter.CircleTexturePath, ImportAssetOptions.ForceUpdate);

            var importer = AssetImporter.GetAtPath(RiveDripEmitter.CircleTexturePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(RiveDripEmitter.CircleTexturePath);
        }

        static Material EnsureParticleMaterial(Texture2D texture)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(RiveDrip.ParticleTemplateAssetPath);
            if (mat == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                    ?? Shader.Find("Sprites/Default");
                if (shader == null)
                    return null;
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, RiveDrip.ParticleTemplateAssetPath);
            }

            if (texture != null)
            {
                mat.mainTexture = texture;
                if (mat.HasProperty("_BaseMap"))
                    mat.SetTexture("_BaseMap", texture);
                if (mat.HasProperty("_AlphaClip"))
                    mat.SetFloat("_AlphaClip", 0f);
                mat.DisableKeyword("_ALPHATEST_ON");
                EditorUtility.SetDirty(mat);
            }

            return mat;
        }
    }
}
#endif
