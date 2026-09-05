using ManosLimpias.Core;
using ManosLimpias.UI.Rive;
using NUnit.Framework;
using Rive.Components;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ManosLimpias.Tests
{
    public class RiveGlowTests
    {
        [Test]
        public void Shader_ExposesGlowPropertyIds()
        {
            var shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/Shaders/RiveAlphaGlow.shader");
            Assert.That(shader, Is.Not.Null);
            Assert.That(shader.FindPropertyIndex("_GlowColor"), Is.GreaterThanOrEqualTo(0));
            Assert.That(shader.FindPropertyIndex("_GlowWidth"), Is.GreaterThanOrEqualTo(0));
            Assert.That(shader.FindPropertyIndex("_GlowIntensity"), Is.GreaterThanOrEqualTo(0));
            Assert.That(shader.FindPropertyIndex("_GlowThreshold"), Is.GreaterThanOrEqualTo(0));
            Assert.That(RiveGlow.ShaderName, Is.EqualTo("ManosLimpias/UI/RiveAlphaGlow"));
        }

        [Test]
        public void SetOn_DrivesMaterialIntensity()
        {
            var go = new GameObject("RiveGlow Intensity");
            try
            {
                var shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/Shaders/RiveAlphaGlow.shader");
                Assert.That(shader, Is.Not.Null);
                var template = new Material(shader);
                var glow = go.AddComponent<RiveGlow>();
                glow.template = template;
                glow.intensity = 1.25f;
                glow.SetOn(true);
                Assert.That(glow.IsOn, Is.True);
                Assert.That(glow.RuntimeMaterial, Is.Not.Null);
                Assert.That(glow.RuntimeMaterial.GetFloat(RiveGlow.GlowIntensityId), Is.EqualTo(1.25f).Within(0.001f));

                glow.SetOn(false);
                Assert.That(glow.IsOn, Is.False);
                Assert.That(glow.RuntimeMaterial.GetFloat(RiveGlow.GlowIntensityId), Is.EqualTo(0f).Within(0.001f));

                glow.SetOn(true);
                Assert.That(glow.RuntimeMaterial.GetFloat(RiveGlow.GlowIntensityId), Is.EqualTo(1.25f).Within(0.001f));

                glow.Tick(0.5f);
                Assert.That(glow.IsOn, Is.True);
                Assert.That(glow.RuntimeMaterial.GetFloat(RiveGlow.GlowIntensityId), Is.EqualTo(0f).Within(0.001f));

                glow.Tick(0.5f);
                Assert.That(glow.RuntimeMaterial.GetFloat(RiveGlow.GlowIntensityId), Is.EqualTo(1.25f).Within(0.001f));
                Object.DestroyImmediate(template);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void EnsureIsolated_WrapsSharedPanelWidget()
        {
            var canvasGo = new GameObject("RiveGlow Canvas", typeof(RectTransform), typeof(Canvas));
            var sharedGo = new GameObject("Rive Panel", typeof(RectTransform), typeof(RivePanel));
            sharedGo.transform.SetParent(canvasGo.transform, false);
            var bgGo = new GameObject(Background.WidgetName, typeof(RectTransform));
            bgGo.transform.SetParent(sharedGo.transform, false);
            var faucetGo = new GameObject(Faucet.WidgetName, typeof(RectTransform));
            faucetGo.transform.SetParent(sharedGo.transform, false);
            var bg = bgGo.AddComponent<RiveWidget>();
            var faucet = faucetGo.AddComponent<RiveWidget>();
            var glowGo = new GameObject("RiveGlow Host");
            glowGo.transform.SetParent(canvasGo.transform, false);
            var glow = glowGo.AddComponent<RiveGlow>();
            glow.widget = faucet;

            try
            {
                Assert.That(RiveGlow.IsIsolated(faucet), Is.False);
                Assert.That(glow.EnsureIsolated(), Is.True);
                Assert.That(RiveGlow.IsIsolated(faucet), Is.True);
                var isolatedPanel = faucet.GetComponentInParent<RivePanel>();
                Assert.That(isolatedPanel, Is.Not.Null);
                Assert.That(isolatedPanel, Is.Not.EqualTo(sharedGo.GetComponent<RivePanel>()));
                Assert.That(isolatedPanel, Is.Not.EqualTo(bg.GetComponentInParent<RivePanel>()));
                Assert.That(RiveGlow.LayoutRect(faucet), Is.EqualTo(isolatedPanel.WidgetContainer));
            }
            finally
            {
                Object.DestroyImmediate(canvasGo);
            }
        }

        [Test]
        public void AnchorMount_LayoutsIsolatedPanel()
        {
            var canvasGo = new GameObject("RiveGlow Anchor Canvas", typeof(RectTransform), typeof(Canvas));
            var canvas = canvasGo.GetComponent<RectTransform>();
            canvas.anchorMin = canvas.anchorMax = new Vector2(0.5f, 0.5f);
            canvas.pivot = new Vector2(0.5f, 0.5f);
            canvas.sizeDelta = new Vector2(1920f, 1080f);

            var rootGo = new GameObject("Rive Panel", typeof(RectTransform), typeof(RivePanel));
            var root = rootGo.GetComponent<RectTransform>();
            root.SetParent(canvas, false);
            ArtboardSpace.StretchFill(root);

            var bgGo = new GameObject(Background.WidgetName, typeof(RectTransform));
            var bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.SetParent(root, false);
            ArtboardSpace.StretchFill(bgRt);
            var background = bgGo.AddComponent<RiveWidget>();

            var panelGo = new GameObject(RiveGlow.PanelNameFor(Faucet.WidgetName), typeof(RectTransform), typeof(RivePanel));
            var panelRt = panelGo.GetComponent<RectTransform>();
            panelRt.SetParent(canvas, false);
            var faucetGo = new GameObject(Faucet.WidgetName, typeof(RectTransform));
            var faucetRt = faucetGo.GetComponent<RectTransform>();
            faucetRt.SetParent(panelRt, false);
            ArtboardSpace.StretchFill(faucetRt);
            var faucet = faucetGo.AddComponent<RiveWidget>();

            var mount = rootGo.AddComponent<RiveAnchorMount>();
            mount.Bind(background, new[]
            {
                new RiveAnchorMount.Slot { anchorName = BackgroundAnchors.Faucet, widget = faucet },
            });

            try
            {
                Assert.That(mount.TryApply(), Is.True);
                Assert.That(RiveGlow.LayoutRect(faucet), Is.EqualTo(panelRt));
                Assert.That(panelRt.rect.width, Is.GreaterThan(1f));
                Assert.That(panelRt.rect.height, Is.GreaterThan(1f));

                var view = canvas.rect;
                BackgroundAnchors.TryGetArtboardAabb(
                    BackgroundAnchors.Faucet, SimiPrototypeArtboards.FaucetSize, out var designAabb);
                BackgroundAnchors.TryGetArtboardAabb(
                    BackgroundAnchors.Faucet,
                    SimiPrototypeArtboards.FaucetSize + SimiPrototypeArtboards.FaucetOverflow,
                    out var visualAabb);
                var designMapped = ArtboardSpace.MapAabbToView(
                    designAabb.xMin, designAabb.yMin, designAabb.xMax, designAabb.yMax,
                    SimiPrototypeArtboards.BackgroundSize, view);
                var visualMapped = ArtboardSpace.MapAabbToView(
                    visualAabb.xMin, visualAabb.yMin, visualAabb.xMax, visualAabb.yMax,
                    SimiPrototypeArtboards.BackgroundSize, view);
                Assert.That(panelRt.rect.height, Is.EqualTo(visualMapped.height).Within(0.5f));
                Assert.That(panelRt.rect.height, Is.GreaterThan(designMapped.height + 1f));
                Assert.That(faucetRt.anchorMin, Is.EqualTo(Vector2.zero));
                Assert.That(faucetRt.anchorMax, Is.EqualTo(Vector2.one));
            }
            finally
            {
                Object.DestroyImmediate(canvasGo);
            }
        }

        [Test]
        public void Faucet_SetGlow_DrivesRiveGlow()
        {
            CreateHintTarget("FaucetHint", out var root, out var widget, out var glow);
            try
            {
                var faucet = widget.gameObject.AddComponent<Faucet>();
                faucet.Bind(widget);
                Assert.That(glow.IsOn, Is.False);

                faucet.SetEnabled(true);
                Assert.That(glow.IsOn, Is.False);

                faucet.SetGlow(true);
                Assert.That(faucet.IsGlowing, Is.True);
                Assert.That(glow.IsOn, Is.True);

                faucet.LockOpen(FaucetSide.Left);
                Assert.That(glow.IsOn, Is.True);

                faucet.SetGlow(false);
                Assert.That(faucet.IsGlowing, Is.False);
                Assert.That(glow.IsOn, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Hands_SetGlow_AndGrabRaisesDragStartedWithoutTogglingGlow()
        {
            CreateHintTarget("HandsHint", out var root, out var widget, out var glow);
            var canvas = root.GetComponent<RectTransform>();
            var panel = (RectTransform)widget.transform.parent;
            ArtboardSpace.ApplyNormalizedAnchors(
                panel,
                canvas,
                new Rect(-400f, -200f, 800f, 500f),
                ArtboardSpace.UnityPivotFromRiveOrigin(SimiPrototypeArtboards.HandsOrigin));
            ArtboardSpace.StretchFill(widget.RectTransform);

            var hands = widget.gameObject.AddComponent<Hands>();
            hands.Bind(widget);
            hands.SetDraggable(true);
            hands.SetGlow(true);

            try
            {
                Assert.That(glow.IsOn, Is.True);

                var started = false;
                hands.DragStarted += () => started = true;
                var grab = ParentLocalOf(panel, new Vector2(0.25f, 0.75f));
                hands.NotifyParentLocalPointer(grab, pressedThisFrame: true, held: true);
                Assert.That(started, Is.True);
                Assert.That(hands.IsGlowing, Is.True);
                Assert.That(glow.IsOn, Is.True);

                hands.SetGlow(false);
                Assert.That(glow.IsOn, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        static void CreateHintTarget(string name, out GameObject root, out RiveWidget widget, out RiveGlow glow)
        {
            root = new GameObject(name + " Root", typeof(RectTransform), typeof(Canvas));
            var canvas = root.GetComponent<RectTransform>();
            canvas.anchorMin = canvas.anchorMax = new Vector2(0.5f, 0.5f);
            canvas.pivot = new Vector2(0.5f, 0.5f);
            canvas.sizeDelta = new Vector2(1920f, 1080f);

            var panelGo = new GameObject(name + " Panel", typeof(RectTransform), typeof(RivePanel));
            panelGo.transform.SetParent(root.transform, false);
            ArtboardSpace.StretchFill(panelGo.GetComponent<RectTransform>());

            var widgetGo = new GameObject(name + " Widget", typeof(RectTransform));
            widgetGo.transform.SetParent(panelGo.transform, false);
            ArtboardSpace.StretchFill(widgetGo.GetComponent<RectTransform>());
            widget = widgetGo.AddComponent<RiveWidget>();
            glow = panelGo.AddComponent<RiveGlow>();
        }

        static Vector2 ParentLocalOf(RectTransform child, Vector2 normalizedInChild)
        {
            var rect = child.rect;
            var local = new Vector2(
                Mathf.Lerp(rect.xMin, rect.xMax, normalizedInChild.x),
                Mathf.Lerp(rect.yMin, rect.yMax, normalizedInChild.y));
            var parent = (RectTransform)child.parent;
            return parent.InverseTransformPoint(child.TransformPoint(local));
        }

        [Test]
        public void GameplayScene_FaucetIsIsolatedWithGlow()
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
                RiveWidget faucet = null;
                Transform intro = null;
                foreach (var root in scene.GetRootGameObjects())
                {
                    var f = FindNamed(root.transform, Faucet.WidgetName);
                    if (f != null)
                        faucet = f.GetComponent<RiveWidget>();
                    var i = FindNamed(root.transform, Intro.WidgetName);
                    if (i != null)
                        intro = i;
                }

                Assert.That(faucet, Is.Not.Null, "FaucetRive missing from Gameplay");
                Assert.That(RiveGlow.IsIsolated(faucet), Is.True);
                Assert.That(faucet.GetComponentInParent<RiveGlow>(), Is.Not.Null);
                Assert.That(intro, Is.Not.Null, "IntroRive missing from Gameplay");
                Assert.That(intro.parent.name, Is.EqualTo(RiveGlow.PanelNameFor(Intro.WidgetName)));
                Assert.That(intro.parent.GetSiblingIndex(), Is.GreaterThan(faucet.transform.parent.GetSiblingIndex()));
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
