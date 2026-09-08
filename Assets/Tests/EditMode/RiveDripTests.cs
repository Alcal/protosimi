using ManosLimpias.UI.Rive;
using NUnit.Framework;
using Rive.Components;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ManosLimpias.Tests
{
    public class RiveDripTests
    {
        [Test]
        public void Shader_ExposesDripPropertyIds()
        {
            var shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/Shaders/RiveAlphaDrip.shader");
            Assert.That(shader, Is.Not.Null);
            Assert.That(shader.FindPropertyIndex("_DripColor"), Is.GreaterThanOrEqualTo(0));
            Assert.That(shader.FindPropertyIndex("_RimWidth"), Is.GreaterThanOrEqualTo(0));
            Assert.That(shader.FindPropertyIndex("_DripWidth"), Is.GreaterThanOrEqualTo(0));
            Assert.That(shader.FindPropertyIndex("_DripLoad"), Is.GreaterThanOrEqualTo(0));
            Assert.That(shader.FindPropertyIndex("_DripIntensity"), Is.GreaterThanOrEqualTo(0));
            Assert.That(shader.FindPropertyIndex("_DripThreshold"), Is.GreaterThanOrEqualTo(0));
            Assert.That(shader.FindPropertyIndex("_WobbleAmp"), Is.GreaterThanOrEqualTo(0));
            Assert.That(shader.FindPropertyIndex("_WobbleFreq"), Is.GreaterThanOrEqualTo(0));
            Assert.That(shader.FindPropertyIndex("_WobbleSpeed"), Is.GreaterThanOrEqualTo(0));
            Assert.That(shader.FindPropertyIndex("_Gravity"), Is.GreaterThanOrEqualTo(0));
            Assert.That(RiveDrip.ShaderName, Is.EqualTo("ManosLimpias/UI/RiveAlphaDrip"));
        }

        [Test]
        public void SetOn_DrivesIntensityAndParticleEmission()
        {
            var go = new GameObject("RiveDrip Intensity", typeof(RectTransform));
            try
            {
                var shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/Shaders/RiveAlphaDrip.shader");
                Assert.That(shader, Is.Not.Null);
                var template = new Material(shader);
                var drip = go.AddComponent<RiveDrip>();
                drip.followWaterContact = false;
                drip.template = template;
                drip.intensity = 1.25f;
                drip.SetWetness(0.4f);
                Assert.That(drip.IsOn, Is.True);
                Assert.That(drip.Wetness, Is.EqualTo(0.4f).Within(0.001f));
                Assert.That(drip.RuntimeMaterial, Is.Not.Null);
                Assert.That(drip.RuntimeMaterial.GetFloat(RiveDrip.DripIntensityId), Is.EqualTo(0.5f).Within(0.001f));

                drip.SetOn(false);
                Assert.That(drip.IsOn, Is.False);
                Assert.That(drip.RuntimeMaterial.GetFloat(RiveDrip.DripIntensityId), Is.EqualTo(0f).Within(0.001f));
                Object.DestroyImmediate(template);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Emitter_SetWetness_ScalesBakedParticleValues()
        {
            var go = new GameObject("RiveDripEmitter Wetness", typeof(ParticleSystem));
            try
            {
                var ps = go.GetComponent<ParticleSystem>();
                var emission = ps.emission;
                emission.enabled = true;
                emission.rateOverTime = 10f;
                var main = ps.main;
                main.startSize = 20f;
                main.startColor = new Color(1f, 1f, 1f, 0.8f);
                main.gravityModifier = 0.45f;

                var emitter = go.AddComponent<RiveDripEmitter>();
                emitter.SetWetness(0.5f);

                Assert.That(emitter.Wetness, Is.EqualTo(0.5f).Within(0.001f));
                Assert.That(ps.emission.enabled, Is.True);
                Assert.That(ps.emission.rateOverTime.constant, Is.EqualTo(5f).Within(0.001f));
                Assert.That(ps.main.startSize.constant, Is.EqualTo(15.5f).Within(0.001f));
                Assert.That(ps.main.startColor.color.a, Is.EqualTo(0.4f).Within(0.001f));
                Assert.That(ps.main.gravityModifier.constant, Is.EqualTo(0.45f).Within(0.001f));

                emitter.SetWetness(0f);
                Assert.That(ps.emission.enabled, Is.False);
                Assert.That(ps.emission.rateOverTime.constant, Is.EqualTo(0f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void SetWetness_InstantiatesEmitterBehindEachHitbox()
        {
            var prefabGo = AssetDatabase.LoadAssetAtPath<GameObject>(RiveDrip.EmitterPrefabPath);
            Assert.That(prefabGo, Is.Not.Null, "RiveDripEmitter prefab missing");
            var prefab = prefabGo.GetComponent<RiveDripEmitter>();
            Assert.That(prefab, Is.Not.Null);

            var go = new GameObject("Hands", typeof(RectTransform));
            var hb1 = new GameObject(RiveNodeHitbox.Hitbox1, typeof(RectTransform));
            hb1.transform.SetParent(go.transform, false);
            var hb2 = new GameObject(RiveNodeHitbox.Hitbox2, typeof(RectTransform));
            hb2.transform.SetParent(go.transform, false);

            try
            {
                var drip = go.AddComponent<RiveDrip>();
                drip.followWaterContact = false;
                drip.emitterPrefab = prefab;
                drip.SetWetness(0.5f);

                Assert.That(drip.Emitters.Count, Is.EqualTo(2));
                Assert.That(drip.Emitters[0].transform.parent, Is.EqualTo(hb1.transform));
                Assert.That(drip.Emitters[1].transform.parent, Is.EqualTo(hb2.transform));
                Assert.That(drip.Emitters[0].transform.GetSiblingIndex(), Is.EqualTo(0));
                Assert.That(drip.Emitters[1].transform.GetSiblingIndex(), Is.EqualTo(0));
                Assert.That(drip.Emitters[0].Wetness, Is.EqualTo(0.5f).Within(0.001f));
                Assert.That(drip.Emitters[1].Wetness, Is.EqualTo(0.5f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void EnsureDrawOrder_PlacesParticlesBetweenBackgroundAndHands()
        {
            var rootGo = new GameObject("GameplayCanvas", typeof(RectTransform), typeof(Canvas));
            var bg = new GameObject("Rive Panel", typeof(RectTransform));
            bg.transform.SetParent(rootGo.transform, false);
            var handsPanel = new GameObject("HandsRivePanel", typeof(RectTransform), typeof(RivePanel));
            handsPanel.transform.SetParent(rootGo.transform, false);
            var faucetPanel = new GameObject("FaucetRivePanel", typeof(RectTransform));
            faucetPanel.transform.SetParent(rootGo.transform, false);
            var introPanel = new GameObject(RiveGlow.PanelNameFor(Intro.WidgetName), typeof(RectTransform));
            introPanel.transform.SetParent(rootGo.transform, false);
            var hands = new GameObject(Hands.WidgetName, typeof(RectTransform));
            hands.transform.SetParent(handsPanel.transform, false);
            var hb = new GameObject(RiveNodeHitbox.Hitbox1, typeof(RectTransform));
            hb.transform.SetParent(hands.transform, false);

            try
            {
                var prefabGo = AssetDatabase.LoadAssetAtPath<GameObject>(RiveDrip.EmitterPrefabPath);
                Assert.That(prefabGo, Is.Not.Null);
                var drip = hands.AddComponent<RiveDrip>();
                drip.followWaterContact = false;
                drip.widget = hands.AddComponent<RiveWidget>();
                drip.emitterPrefab = prefabGo.GetComponent<RiveDripEmitter>();
                drip.SetWetness(1f);
                drip.EnsureDrawOrder();

                Assert.That(drip.Emitters.Count, Is.EqualTo(1));
                var psr = drip.Emitters[0].GetComponent<ParticleSystemRenderer>();
                Assert.That(psr.sortingOrder, Is.EqualTo(RiveDripEmitter.ParticleSortingOffset));

                var handsCanvas = handsPanel.GetComponent<Canvas>();
                Assert.That(handsCanvas, Is.Not.Null);
                Assert.That(handsCanvas.overrideSorting, Is.True);
                Assert.That(handsCanvas.sortingOrder, Is.EqualTo(RiveDripEmitter.OverlaySortingOffset));
                Assert.That(handsCanvas.sortingOrder, Is.GreaterThan(psr.sortingOrder));
                Assert.That(bg.GetComponent<Canvas>(), Is.Null);

                var faucetCanvas = faucetPanel.GetComponent<Canvas>();
                Assert.That(faucetCanvas, Is.Not.Null);
                Assert.That(faucetCanvas.sortingOrder, Is.EqualTo(RiveDripEmitter.OverlaySortingOffset));

                var introCanvas = introPanel.GetComponent<Canvas>();
                Assert.That(introCanvas, Is.Not.Null);
                Assert.That(introCanvas.overrideSorting, Is.True);
                Assert.That(introCanvas.sortingOrder, Is.EqualTo(RiveDripEmitter.ModalSortingOffset));
                Assert.That(introCanvas.sortingOrder, Is.GreaterThan(faucetCanvas.sortingOrder));
            }
            finally
            {
                Object.DestroyImmediate(rootGo);
            }
        }

        [Test]
        public void SetOnFalse_RestoresGlowMaterialWhenGlowIsOn()
        {
            var root = new GameObject("RiveDrip Glow Restore", typeof(RectTransform), typeof(Canvas));
            var panelGo = new GameObject("Panel", typeof(RectTransform), typeof(RivePanel), typeof(RiveCanvasRenderer));
            panelGo.transform.SetParent(root.transform, false);
            var widgetGo = new GameObject("Widget", typeof(RectTransform));
            widgetGo.transform.SetParent(panelGo.transform, false);
            ArtboardSpace.StretchFill(widgetGo.GetComponent<RectTransform>());
            var widget = widgetGo.AddComponent<RiveWidget>();

            try
            {
                var glowShader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/Shaders/RiveAlphaGlow.shader");
                var dripShader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/Shaders/RiveAlphaDrip.shader");
                Assert.That(glowShader, Is.Not.Null);
                Assert.That(dripShader, Is.Not.Null);
                var glowTemplate = new Material(glowShader);
                var dripTemplate = new Material(dripShader);

                var glow = panelGo.AddComponent<RiveGlow>();
                glow.widget = widget;
                glow.template = glowTemplate;
                glow.intensity = 0.8f;
                glow.SetOn(true);

                var drip = panelGo.AddComponent<RiveDrip>();
                drip.followWaterContact = false;
                drip.widget = widget;
                drip.template = dripTemplate;
                drip.SetOn(true);

                var renderer = panelGo.GetComponent<RiveCanvasRenderer>();
                Assert.That(renderer.CustomMaterial, Is.EqualTo(drip.RuntimeMaterial));

                drip.SetOn(false);
                Assert.That(glow.IsOn, Is.True);
                Assert.That(renderer.CustomMaterial, Is.EqualTo(glow.RuntimeMaterial));

                Object.DestroyImmediate(glowTemplate);
                Object.DestroyImmediate(dripTemplate);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void GameplayScene_HandsRiveHasDrip()
        {
            const string path = "Assets/Scenes/Gameplay.unity";
            var scene = SceneManager.GetSceneByPath(path);
            bool opened = false;
            if (!scene.IsValid() || !scene.isLoaded)
            {
                scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                opened = true;
            }

            try
            {
                RiveWidget hands = null;
                foreach (var root in scene.GetRootGameObjects())
                {
                    var found = FindNamed(root.transform, Hands.WidgetName);
                    if (found != null)
                        hands = found.GetComponent<RiveWidget>();
                }

                Assert.That(hands, Is.Not.Null, "HandsRive missing from Gameplay");
                var drip = hands.GetComponent<RiveDrip>();
                Assert.That(drip, Is.Not.Null, "RiveDrip should be on HandsRive");
                Assert.That(drip.followWaterContact, Is.False);
                Assert.That(drip.widget, Is.EqualTo(hands));
                Assert.That(drip.emitterPrefab, Is.Not.Null, "RiveDrip should reference the drip emitter prefab");
            }
            finally
            {
                if (opened)
                    EditorSceneManager.CloseScene(scene, true);
            }
        }

        static Transform FindNamed(Transform root, string name)
        {
            if (root.name == name)
                return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var found = FindNamed(root.GetChild(i), name);
                if (found != null)
                    return found;
            }

            return null;
        }
    }
}
