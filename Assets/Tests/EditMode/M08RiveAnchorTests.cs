using ManosLimpias.Core;
using ManosLimpias.UI.Rive;
using NUnit.Framework;
using Rive.Components;
using UnityEngine;

namespace ManosLimpias.Tests
{
    public class M08RiveAnchorTests
    {
        [Test]
        public void AC01_BackgroundArtboard_IsMounted()
        {
            Assert.That(Background.Artboard, Is.EqualTo("background"));
            Assert.That(Background.Artboard, Is.Not.EqualTo(SimiPrototypeArtboards.Main));
            Assert.That(ProgressBar.Artboard, Is.EqualTo("progressBar"));
        }

        [Test]
        public void AC02_AnchorSlots_HaveWidgets()
        {
            Assert.That(BackgroundAnchors.All, Does.Contain(BackgroundAnchors.Faucet));
            Assert.That(BackgroundAnchors.All, Does.Contain(BackgroundAnchors.ProgressBar));
            Assert.That(StepIcon.AnchorNames, Is.EqualTo(new[]
            {
                BackgroundAnchors.Step1,
                BackgroundAnchors.Step2,
                BackgroundAnchors.Step3,
                BackgroundAnchors.Step4,
            }));
            Assert.That(BackgroundAnchors.Faucet, Is.EqualTo("faucet-anchor"));
            Assert.That(BackgroundAnchors.Faucet, Does.Not.Contain("["));
        }

        [Test]
        public void AC03_ContainCenter_MapsAabbToView()
        {
            var view = new Rect(0f, 0f, 1920f, 1080f);
            var artboard = new Vector2(1920f, 1080f);
            var mapped = ArtboardSpace.MapAabbToView(100f, 200f, 300f, 400f, artboard, view);

            Assert.That(mapped.xMin, Is.EqualTo(100f).Within(0.01f));
            Assert.That(mapped.xMax, Is.EqualTo(300f).Within(0.01f));
            Assert.That(mapped.yMin, Is.EqualTo(1080f - 400f).Within(0.01f));
            Assert.That(mapped.yMax, Is.EqualTo(1080f - 200f).Within(0.01f));

            var topLeft = ArtboardSpace.MapAabbToView(0f, 0f, 200f, 100f, artboard, view);
            Assert.That(topLeft.yMax, Is.EqualTo(1080f).Within(0.01f));
            Assert.That(topLeft.yMin, Is.EqualTo(980f).Within(0.01f));
            Assert.That(topLeft.width, Is.EqualTo(200f).Within(0.01f));
            Assert.That(topLeft.height, Is.EqualTo(100f).Within(0.01f));

            var letterboxed = new Rect(0f, 0f, 1920f, 1200f);
            var full = ArtboardSpace.MapAabbToView(0f, 0f, 1920f, 1080f, artboard, letterboxed);
            Assert.That(full.yMin, Is.EqualTo(60f).Within(0.01f));
            Assert.That(full.height, Is.EqualTo(1080f).Within(0.01f));

            var stretched = ArtboardSpace.MapAabbToViewFill(0f, 0f, 192f, 108f, artboard, letterboxed);
            Assert.That(stretched.xMin, Is.EqualTo(0f).Within(0.01f));
            Assert.That(stretched.width, Is.EqualTo(192f).Within(0.01f));
            Assert.That(stretched.yMax, Is.EqualTo(1200f).Within(0.01f));
            Assert.That(stretched.height, Is.EqualTo(120f).Within(0.01f));

            var anchors = ArtboardSpace.ToNormalizedAnchors(mapped, view);
            Assert.That(anchors.x, Is.EqualTo(100f / 1920f).Within(0.0001f));
            Assert.That(anchors.w, Is.EqualTo((1080f - 200f) / 1080f).Within(0.0001f));
        }

        [Test]
        public void AC03_CatalogOrigins_MatchRivAndProduceNonZeroRects()
        {
            var bytes = System.IO.File.ReadAllBytes(
                System.IO.Path.Combine(Application.dataPath, "Art/Rive/simi_prototype.riv"));

            Assert.That(RiveAnchorFile.TryReadLocal(bytes, BackgroundAnchors.Faucet, out var faucetParent, out var faucetLocal), Is.True);
            Assert.That(faucetParent, Is.EqualTo(0));
            Assert.That(faucetLocal.x, Is.EqualTo(557f).Within(0.01f));
            Assert.That(faucetLocal.y, Is.EqualTo(105f).Within(0.01f));

            Assert.That(RiveAnchorFile.TryReadLocal(bytes, BackgroundAnchors.Step1, out var stepParent, out var stepLocal), Is.True);
            Assert.That(stepParent, Is.EqualTo(8));
            Assert.That(stepLocal.y, Is.EqualTo(-360f).Within(0.01f));

            Assert.That(RiveAnchorFile.TryReadStepGroupOrigin(bytes, out var group), Is.True);
            Assert.That(group, Is.EqualTo(BackgroundAnchors.StepGroupOrigin));

            Assert.That(BackgroundAnchors.TryGetWorldOrigin(BackgroundAnchors.Faucet, out var faucetOrigin), Is.True);
            Assert.That(faucetOrigin, Is.EqualTo(faucetLocal));
            Assert.That(BackgroundAnchors.TryGetWorldOrigin(BackgroundAnchors.Step1, out var stepOrigin), Is.True);
            Assert.That(stepOrigin, Is.EqualTo(group + stepLocal));

            var view = new Rect(0f, 0f, 1920f, 1080f);
            Assert.That(BackgroundAnchors.TryGetArtboardAabb(
                BackgroundAnchors.Faucet, SimiPrototypeArtboards.FaucetSize, out var aabb), Is.True);
            Assert.That(aabb.xMin, Is.EqualTo(557f).Within(0.01f));
            Assert.That(aabb.yMin, Is.EqualTo(105f).Within(0.01f));
            var mapped = ArtboardSpace.MapAabbToView(
                aabb.xMin, aabb.yMin, aabb.xMax, aabb.yMax, SimiPrototypeArtboards.BackgroundSize, view);
            Assert.That(ArtboardSpace.HasArea(mapped), Is.True);

            var parentGo = new GameObject("M08 Anchor Parent", typeof(RectTransform));
            var childGo = new GameObject("M08 Anchor Child", typeof(RectTransform));
            var parent = parentGo.GetComponent<RectTransform>();
            var child = childGo.GetComponent<RectTransform>();
            parent.anchorMin = parent.anchorMax = new Vector2(0.5f, 0.5f);
            parent.pivot = new Vector2(0.5f, 0.5f);
            parent.sizeDelta = new Vector2(1920f, 1080f);
            child.SetParent(parent, false);

            ArtboardSpace.ApplyNormalizedAnchors(child, parent, mapped);
            Assert.That(child.rect.width, Is.GreaterThan(1f));
            Assert.That(child.rect.height, Is.GreaterThan(1f));
            Assert.That(child.anchorMin, Is.Not.EqualTo(child.anchorMax));

            UnityEngine.Object.DestroyImmediate(parentGo);
        }

        [Test]
        public void AC03_ArtboardOrigin_OffsetsAabbFromNode()
        {
            var bytes = System.IO.File.ReadAllBytes(
                System.IO.Path.Combine(Application.dataPath, "Art/Rive/simi_prototype.riv"));

            Assert.That(RiveAnchorFile.TryReadArtboardOrigin(bytes, SimiPrototypeArtboards.Faucet, out var faucetOrigin), Is.True);
            Assert.That(faucetOrigin, Is.EqualTo(SimiPrototypeArtboards.FaucetOrigin));
            Assert.That(faucetOrigin, Is.EqualTo(Vector2.zero));

            Assert.That(RiveAnchorFile.TryReadArtboardOrigin(bytes, SimiPrototypeArtboards.Soap, out var soapOrigin), Is.True);
            Assert.That(soapOrigin, Is.EqualTo(SimiPrototypeArtboards.SoapOrigin));
            Assert.That(soapOrigin.x, Is.EqualTo(0.5f).Within(0.001f));
            Assert.That(soapOrigin.y, Is.EqualTo(0.5f).Within(0.001f));

            Assert.That(RiveAnchorFile.TryReadArtboardOrigin(bytes, SimiPrototypeArtboards.StepIcon, out var stepOrigin), Is.True);
            Assert.That(stepOrigin, Is.EqualTo(SimiPrototypeArtboards.StepIconOrigin));

            Assert.That(BackgroundAnchors.TryGetWorldOrigin(BackgroundAnchors.Soap, out var soapNode), Is.True);
            Assert.That(BackgroundAnchors.TryGetArtboardAabb(
                BackgroundAnchors.Soap, SimiPrototypeArtboards.SoapSize, out var soapAabb), Is.True);
            Assert.That(soapAabb.xMin, Is.EqualTo(soapNode.x - SimiPrototypeArtboards.SoapSize.x * 0.5f).Within(0.01f));
            Assert.That(soapAabb.yMin, Is.EqualTo(soapNode.y - SimiPrototypeArtboards.SoapSize.y * 0.5f).Within(0.01f));
            Assert.That(soapAabb.width, Is.EqualTo(400f).Within(0.01f));

            var centerBox = ArtboardSpace.RectFromOriginSize(
                new Vector2(100f, 200f), new Vector2(40f, 80f), new Vector2(0.5f, 0.5f));
            Assert.That(centerBox.xMin, Is.EqualTo(80f).Within(0.01f));
            Assert.That(centerBox.yMin, Is.EqualTo(160f).Within(0.01f));
            Assert.That(ArtboardSpace.UnityPivotFromRiveOrigin(new Vector2(0.5f, 0.5f)), Is.EqualTo(new Vector2(0.5f, 0.5f)));
            Assert.That(ArtboardSpace.UnityPivotFromRiveOrigin(Vector2.zero), Is.EqualTo(new Vector2(0f, 1f)));
        }

        [Test]
        public void AC04_DisabledFaucet_IgnoresActivation()
        {
            var go = new GameObject("M08 Faucet");
            var widgetGo = new GameObject("M08 Faucet Widget", typeof(RectTransform));
            var widget = widgetGo.AddComponent<RiveWidget>();
            var faucet = go.AddComponent<Faucet>();
            int count = 0;
            faucet.Activated += _ => count++;

            faucet.Bind(widget);
            Assert.That(faucet.IsEnabled, Is.False);
            Assert.That(widget.HitTestBehavior, Is.EqualTo(HitTestBehavior.None));

            faucet.Activate(FaucetSide.Left);
            Assert.That(count, Is.Zero);

            faucet.SetEnabled(true);
            Assert.That(widget.HitTestBehavior, Is.EqualTo(HitTestBehavior.Translucent));
            faucet.Activate(FaucetSide.Left);
            Assert.That(count, Is.EqualTo(1));
            Assert.That(faucet.LeftIsOpen, Is.True);
            Assert.That(faucet.IsOpen, Is.True);

            int hits = 0;
            faucet.PointerHit += () => hits++;
            faucet.NotifyPointerHit();
            Assert.That(hits, Is.EqualTo(1));

            faucet.SetEnabled(false);
            Assert.That(widget.HitTestBehavior, Is.EqualTo(HitTestBehavior.None));
            Assert.That(faucet.IsOpen, Is.False);
            faucet.Activate(FaucetSide.Right);
            faucet.NotifyPointerHit();
            Assert.That(count, Is.EqualTo(1));
            Assert.That(hits, Is.EqualTo(1));

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(widgetGo);
        }

        [Test]
        public void Hands_SetDraggable_TogglesHitTest()
        {
            var go = new GameObject("M08 Hands");
            var widgetGo = new GameObject("M08 Hands Widget", typeof(RectTransform));
            var widget = widgetGo.AddComponent<RiveWidget>();
            var hands = go.AddComponent<Hands>();
            hands.Bind(widget);

            Assert.That(hands.IsDraggable, Is.False);
            Assert.That(widget.HitTestBehavior, Is.EqualTo(HitTestBehavior.None));

            hands.SetDraggable(true);
            Assert.That(hands.IsDraggable, Is.True);
            Assert.That(widget.HitTestBehavior, Is.EqualTo(HitTestBehavior.Translucent));

            hands.SetDraggable(false);
            Assert.That(hands.IsDraggable, Is.False);
            Assert.That(widget.HitTestBehavior, Is.EqualTo(HitTestBehavior.None));
            Assert.That(hands.FreezePlacement, Is.False);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(widgetGo);
        }

        [Test]
        public void AC05_NonFaucetSlots_StartWithoutHits()
        {
            Assert.That(Hands.Artboard, Is.EqualTo(SimiPrototypeArtboards.Hands));
            Assert.That(Soap.Artboard, Is.EqualTo(SimiPrototypeArtboards.Soap));
            Assert.That(Towel.Artboard, Is.EqualTo(SimiPrototypeArtboards.Towel));
            Assert.That(Character.Artboard, Is.EqualTo(SimiPrototypeArtboards.Character));
        }
    }
}
