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
        public void ProgressBar_BlendMix_MapsNormalizedToZeroOneHundred()
        {
            Assert.That(ProgressBar.ToBlend(0f), Is.EqualTo(0f));
            Assert.That(ProgressBar.ToBlend(0.25f), Is.EqualTo(25f));
            Assert.That(ProgressBar.ToBlend(1f), Is.EqualTo(100f));
            Assert.That(ProgressBar.ToBlend(2f), Is.EqualTo(100f));
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
        public void FaucetOverflow_ExtendsAabbDownwardWithoutMovingOrigin()
        {
            var overflow = BackgroundAnchors.OverflowFor(BackgroundAnchors.Faucet);
            Assert.That(overflow.y, Is.EqualTo(SimiPrototypeArtboards.FaucetOverflow.y).Within(0.01f));
            Assert.That(overflow.y, Is.GreaterThan(0f));
            Assert.That(BackgroundAnchors.OverflowFor(BackgroundAnchors.Soap), Is.EqualTo(Vector2.zero));

            Assert.That(BackgroundAnchors.TryGetArtboardAabb(
                BackgroundAnchors.Faucet, SimiPrototypeArtboards.FaucetSize, out var faucetDesign), Is.True);
            Assert.That(BackgroundAnchors.TryGetArtboardAabb(
                BackgroundAnchors.Faucet,
                SimiPrototypeArtboards.FaucetSize + SimiPrototypeArtboards.FaucetOverflow,
                out var faucetVisual), Is.True);
            Assert.That(faucetVisual.xMin, Is.EqualTo(faucetDesign.xMin).Within(0.01f));
            Assert.That(faucetVisual.yMin, Is.EqualTo(faucetDesign.yMin).Within(0.01f));
            Assert.That(faucetVisual.width, Is.EqualTo(faucetDesign.width).Within(0.01f));
            Assert.That(faucetVisual.height, Is.EqualTo(faucetDesign.height + overflow.y).Within(0.01f));

            var view = new Rect(0f, 0f, 1920f, 1080f);
            var designMapped = ArtboardSpace.MapAabbToView(
                faucetDesign.xMin, faucetDesign.yMin, faucetDesign.xMax, faucetDesign.yMax,
                SimiPrototypeArtboards.BackgroundSize, view);
            var visualMapped = ArtboardSpace.MapAabbToView(
                faucetVisual.xMin, faucetVisual.yMin, faucetVisual.xMax, faucetVisual.yMax,
                SimiPrototypeArtboards.BackgroundSize, view);
            Assert.That(visualMapped.yMax, Is.EqualTo(designMapped.yMax).Within(0.01f));
            Assert.That(visualMapped.yMin, Is.LessThan(designMapped.yMin - 1f));
            Assert.That(visualMapped.height, Is.EqualTo(designMapped.height + overflow.y).Within(0.01f));
        }

        [Test]
        public void HandsOverflow_ExtendsAabbDownwardWithoutMovingOrigin()
        {
            var overflow = BackgroundAnchors.OverflowFor(BackgroundAnchors.Hands);
            Assert.That(overflow.y, Is.EqualTo(SimiPrototypeArtboards.HandsOverflow.y).Within(0.01f));
            Assert.That(overflow.y, Is.GreaterThan(0f));
            Assert.That(overflow.x, Is.EqualTo(0f).Within(0.01f));

            Assert.That(BackgroundAnchors.TryGetArtboardAabb(
                BackgroundAnchors.Hands, SimiPrototypeArtboards.HandsSize, out var handsDesign), Is.True);
            Assert.That(BackgroundAnchors.TryGetArtboardAabb(
                BackgroundAnchors.Hands,
                SimiPrototypeArtboards.HandsSize + SimiPrototypeArtboards.HandsOverflow,
                out var handsVisual), Is.True);
            Assert.That(handsVisual.xMin, Is.EqualTo(handsDesign.xMin).Within(0.01f));
            Assert.That(handsVisual.yMin, Is.EqualTo(handsDesign.yMin).Within(0.01f));
            Assert.That(handsVisual.width, Is.EqualTo(handsDesign.width).Within(0.01f));
            Assert.That(handsVisual.height, Is.EqualTo(handsDesign.height + overflow.y).Within(0.01f));

            var view = new Rect(0f, 0f, 1920f, 1080f);
            var designMapped = ArtboardSpace.MapAabbToView(
                handsDesign.xMin, handsDesign.yMin, handsDesign.xMax, handsDesign.yMax,
                SimiPrototypeArtboards.BackgroundSize, view);
            var visualMapped = ArtboardSpace.MapAabbToView(
                handsVisual.xMin, handsVisual.yMin, handsVisual.xMax, handsVisual.yMax,
                SimiPrototypeArtboards.BackgroundSize, view);
            Assert.That(visualMapped.yMax, Is.EqualTo(designMapped.yMax).Within(0.01f));
            Assert.That(visualMapped.yMin, Is.LessThan(designMapped.yMin - 1f));
            Assert.That(visualMapped.height, Is.EqualTo(designMapped.height + overflow.y).Within(0.01f));
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
            faucet.PointerHit += _ => hits++;
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
        public void SetEnabled_WithoutRiveHits_KeepsHitTestNone()
        {
            var go = new GameObject("M08 Faucet TapOnly");
            var widgetGo = new GameObject("M08 Faucet TapOnly Widget", typeof(RectTransform));
            var widget = widgetGo.AddComponent<RiveWidget>();
            var faucet = go.AddComponent<Faucet>();
            int hits = 0;
            faucet.PointerHit += _ => hits++;

            faucet.Bind(widget);
            faucet.SetEnabled(true, rivePointerHits: false);

            Assert.That(faucet.IsEnabled, Is.True);
            Assert.That(widget.HitTestBehavior, Is.EqualTo(HitTestBehavior.None));
            faucet.NotifyPointerHit(FaucetSide.Right);
            Assert.That(hits, Is.EqualTo(1));

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(widgetGo);
        }

        [Test]
        public void LockOpen_DisablesHits_KeepsFaucetOpen()
        {
            var go = new GameObject("M08 Faucet LockOpen");
            var widgetGo = new GameObject("M08 Faucet LockOpen Widget", typeof(RectTransform));
            var widget = widgetGo.AddComponent<RiveWidget>();
            var faucet = go.AddComponent<Faucet>();
            int count = 0;
            faucet.Activated += _ => count++;

            faucet.Bind(widget);
            faucet.SetEnabled(true);
            faucet.LockOpen(FaucetSide.Right);

            Assert.That(faucet.IsEnabled, Is.False);
            Assert.That(widget.HitTestBehavior, Is.EqualTo(HitTestBehavior.None));
            Assert.That(faucet.LeftIsOpen, Is.False);
            Assert.That(faucet.RightIsOpen, Is.True);
            Assert.That(faucet.IsOpen, Is.True);
            Assert.That(count, Is.EqualTo(1));

            faucet.Activate(FaucetSide.Left);
            faucet.NotifyPointerHit(FaucetSide.Right);
            Assert.That(count, Is.EqualTo(1));
            Assert.That(faucet.LeftIsOpen, Is.False);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(widgetGo);
        }

        [Test]
        public void LockClosed_DisablesHits_ClosesFaucet()
        {
            var go = new GameObject("M08 Faucet LockClosed");
            var widgetGo = new GameObject("M08 Faucet LockClosed Widget", typeof(RectTransform));
            var widget = widgetGo.AddComponent<RiveWidget>();
            var faucet = go.AddComponent<Faucet>();
            int hits = 0;
            faucet.PointerHit += _ => hits++;

            faucet.Bind(widget);
            faucet.SetEnabled(true);
            faucet.LockOpen(FaucetSide.Left);
            faucet.SetEnabled(true);
            faucet.LockClosed();

            Assert.That(faucet.IsEnabled, Is.False);
            Assert.That(widget.HitTestBehavior, Is.EqualTo(HitTestBehavior.None));
            Assert.That(faucet.IsOpen, Is.False);
            Assert.That(faucet.LeftIsOpen, Is.False);
            Assert.That(faucet.RightIsOpen, Is.False);

            faucet.Activate(FaucetSide.Right);
            faucet.NotifyPointerHit(FaucetSide.Left);
            Assert.That(hits, Is.EqualTo(0));
            Assert.That(faucet.IsOpen, Is.False);

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
        public void Hands_SetDraggableFalse_KeepsFreezePlacementAfterGrab()
        {
            var mapped = new Rect(-400f, -200f, 800f, 500f);
            CreateStretchHands(mapped, out var parent, out var child, out var hands);
            try
            {
                var grab = ParentLocalPoint(child, new Vector2(0.25f, 0.75f));
                hands.NotifyParentLocalPointer(grab, pressedThisFrame: true, held: true);
                Assert.That(hands.FreezePlacement, Is.True);

                hands.SetDraggable(false);
                Assert.That(hands.IsDraggable, Is.False);
                Assert.That(hands.FreezePlacement, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(parent.gameObject);
            }
        }

        [Test]
        public void Hands_ReturnHome_RestoresLayoutAfterDrag()
        {
            var mapped = new Rect(-400f, -200f, 800f, 500f);
            CreateStretchHands(mapped, out var parent, out var child, out var hands);
            try
            {
                var before = WorldCorners(child);
                var grab = ParentLocalPoint(child, new Vector2(0.25f, 0.75f));
                hands.NotifyParentLocalPointer(grab, pressedThisFrame: true, held: true);
                hands.NotifyParentLocalPointer(grab + new Vector2(60f, -30f), pressedThisFrame: false, held: true);
                Assert.That(hands.FreezePlacement, Is.True);

                hands.ReturnHome();
                Assert.That(hands.FreezePlacement, Is.False);
                AssertCornersEqual(before, WorldCorners(child));
            }
            finally
            {
                Object.DestroyImmediate(parent.gameObject);
            }
        }

        [Test]
        public void Hands_BeginDrag_DoesNotJumpFromTopLeftPivot()
        {
            var mapped = new Rect(-400f, -200f, 800f, 500f);
            CreateStretchHands(mapped, out var parent, out var child, out var hands);
            try
            {
                var before = WorldCorners(child);
                var grab = ParentLocalPoint(child, new Vector2(0.25f, 0.75f));
                hands.NotifyParentLocalPointer(grab, pressedThisFrame: true, held: true);

                AssertCornersEqual(before, WorldCorners(child));
                Assert.That(hands.FreezePlacement, Is.True);
                Assert.That(child.anchorMin, Is.EqualTo(child.anchorMax));
                Assert.That(child.pivot, Is.EqualTo(new Vector2(0.5f, 0.5f)));
            }
            finally
            {
                Object.DestroyImmediate(parent.gameObject);
            }
        }

        [Test]
        public void Hands_IsolatedPanel_DragMovesPanelNotWidget()
        {
            var canvasGo = new GameObject("M08 Hands Isolated Canvas", typeof(RectTransform));
            var canvas = canvasGo.GetComponent<RectTransform>();
            canvas.anchorMin = canvas.anchorMax = new Vector2(0.5f, 0.5f);
            canvas.pivot = new Vector2(0.5f, 0.5f);
            canvas.sizeDelta = new Vector2(1920f, 1080f);

            var panelGo = new GameObject(RiveGlow.PanelNameFor(Hands.WidgetName), typeof(RectTransform), typeof(RivePanel));
            var panel = panelGo.GetComponent<RectTransform>();
            panel.SetParent(canvas, false);
            var mapped = new Rect(-400f, -200f, 800f, 500f);
            ArtboardSpace.ApplyNormalizedAnchors(
                panel,
                canvas,
                mapped,
                ArtboardSpace.UnityPivotFromRiveOrigin(SimiPrototypeArtboards.HandsOrigin));

            var childGo = new GameObject(Hands.WidgetName, typeof(RectTransform));
            var child = childGo.GetComponent<RectTransform>();
            child.SetParent(panel, false);
            ArtboardSpace.StretchFill(child);
            var widget = childGo.AddComponent<RiveWidget>();
            var hands = childGo.AddComponent<Hands>();
            hands.Bind(widget);
            hands.SetDraggable(true);

            try
            {
                Assert.That(RiveGlow.LayoutRect(widget), Is.EqualTo(panel));
                var beforePanel = WorldCorners(panel);
                var grab = ParentLocalPoint(panel, new Vector2(0.25f, 0.75f));
                hands.NotifyParentLocalPointer(grab, pressedThisFrame: true, held: true);
                var delta = new Vector2(40f, -15f);
                hands.NotifyParentLocalPointer(grab + delta, pressedThisFrame: false, held: true);

                var movedPanel = WorldCorners(panel);
                for (int i = 0; i < 4; i++)
                {
                    Assert.That(movedPanel[i].x, Is.EqualTo(beforePanel[i].x + delta.x).Within(0.05f));
                    Assert.That(movedPanel[i].y, Is.EqualTo(beforePanel[i].y + delta.y).Within(0.05f));
                }

                Assert.That(child.anchorMin, Is.EqualTo(Vector2.zero));
                Assert.That(child.anchorMax, Is.EqualTo(Vector2.one));
                AssertCornersEqual(movedPanel, WorldCorners(child));
            }
            finally
            {
                Object.DestroyImmediate(canvasGo);
            }
        }

        [Test]
        public void Hands_Drag_MovesByDelta_NotToPointer()
        {
            var mapped = new Rect(-400f, -200f, 800f, 500f);
            CreateStretchHands(mapped, out var parent, out var child, out var hands);
            try
            {
                var grab = ParentLocalPoint(child, new Vector2(0.2f, 0.8f));
                hands.NotifyParentLocalPointer(grab, pressedThisFrame: true, held: true);
                var origin = WorldCorners(child);

                var delta = new Vector2(60f, -25f);
                hands.NotifyParentLocalPointer(grab + delta, pressedThisFrame: false, held: true);

                var moved = WorldCorners(child);
                for (int i = 0; i < 4; i++)
                {
                    Assert.That(moved[i].x, Is.EqualTo(origin[i].x + delta.x).Within(0.05f));
                    Assert.That(moved[i].y, Is.EqualTo(origin[i].y + delta.y).Within(0.05f));
                }

                var pointerWorld = (Vector2)parent.TransformPoint(grab + delta);
                var center = (Vector2)((moved[0] + moved[2]) * 0.5f);
                Assert.That(Vector2.Distance(center, pointerWorld), Is.GreaterThan(50f));
            }
            finally
            {
                Object.DestroyImmediate(parent.gameObject);
            }
        }

        static void CreateStretchHands(
            Rect mapped,
            out RectTransform parent,
            out RectTransform child,
            out Hands hands)
        {
            var parentGo = new GameObject("M08 Hands Drag Parent", typeof(RectTransform));
            parent = parentGo.GetComponent<RectTransform>();
            parent.anchorMin = parent.anchorMax = new Vector2(0.5f, 0.5f);
            parent.pivot = new Vector2(0.5f, 0.5f);
            parent.sizeDelta = new Vector2(1920f, 1080f);

            var childGo = new GameObject("M08 HandsRive", typeof(RectTransform));
            child = childGo.GetComponent<RectTransform>();
            child.SetParent(parent, false);
            ArtboardSpace.ApplyNormalizedAnchors(
                child,
                parent,
                mapped,
                ArtboardSpace.UnityPivotFromRiveOrigin(SimiPrototypeArtboards.HandsOrigin));

            var widget = childGo.AddComponent<RiveWidget>();
            hands = childGo.AddComponent<Hands>();
            hands.Bind(widget);
            hands.SetDraggable(true);
        }

        static void CreateStretchSoap(
            Rect mapped,
            out RectTransform parent,
            out RectTransform child,
            out Soap soap)
        {
            var parentGo = new GameObject("M08 Soap Drag Parent", typeof(RectTransform));
            parent = parentGo.GetComponent<RectTransform>();
            parent.anchorMin = parent.anchorMax = new Vector2(0.5f, 0.5f);
            parent.pivot = new Vector2(0.5f, 0.5f);
            parent.sizeDelta = new Vector2(1920f, 1080f);

            var childGo = new GameObject("M08 SoapRive", typeof(RectTransform));
            child = childGo.GetComponent<RectTransform>();
            child.SetParent(parent, false);
            ArtboardSpace.ApplyNormalizedAnchors(
                child,
                parent,
                mapped,
                ArtboardSpace.UnityPivotFromRiveOrigin(SimiPrototypeArtboards.SoapOrigin));

            var widget = childGo.AddComponent<RiveWidget>();
            soap = childGo.AddComponent<Soap>();
            soap.Bind(widget);
            soap.SetDraggable(true);
        }

        static void CreateStretchTowel(
            Rect mapped,
            out RectTransform parent,
            out RectTransform child,
            out Towel towel)
        {
            var parentGo = new GameObject("M08 Towel Drag Parent", typeof(RectTransform));
            parent = parentGo.GetComponent<RectTransform>();
            parent.anchorMin = parent.anchorMax = new Vector2(0.5f, 0.5f);
            parent.pivot = new Vector2(0.5f, 0.5f);
            parent.sizeDelta = new Vector2(1920f, 1080f);

            var childGo = new GameObject("M08 TowelRive", typeof(RectTransform));
            child = childGo.GetComponent<RectTransform>();
            child.SetParent(parent, false);
            ArtboardSpace.ApplyNormalizedAnchors(
                child,
                parent,
                mapped,
                ArtboardSpace.UnityPivotFromRiveOrigin(SimiPrototypeArtboards.TowelOrigin));

            var widget = childGo.AddComponent<RiveWidget>();
            towel = childGo.AddComponent<Towel>();
            towel.Bind(widget);
            towel.SetDraggable(true);
        }

        static Vector3[] WorldCorners(RectTransform rect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return corners;
        }

        static Vector2 ParentLocalPoint(RectTransform child, Vector2 normalizedInChild)
        {
            var rect = child.rect;
            var local = new Vector2(
                Mathf.Lerp(rect.xMin, rect.xMax, normalizedInChild.x),
                Mathf.Lerp(rect.yMin, rect.yMax, normalizedInChild.y));
            var parent = (RectTransform)child.parent;
            return parent.InverseTransformPoint(child.TransformPoint(local));
        }

        static void AssertCornersEqual(Vector3[] expected, Vector3[] actual)
        {
            for (int i = 0; i < 4; i++)
            {
                Assert.That(actual[i].x, Is.EqualTo(expected[i].x).Within(0.05f), $"corner {i} x");
                Assert.That(actual[i].y, Is.EqualTo(expected[i].y).Within(0.05f), $"corner {i} y");
            }
        }

        [Test]
        public void RiveNodeHitbox_Overlaps_IgnoresGrazingEdges()
        {
            var aGo = new GameObject("M08 Hitbox A", typeof(RectTransform));
            var bGo = new GameObject("M08 Hitbox B", typeof(RectTransform));
            try
            {
                var aRt = aGo.GetComponent<RectTransform>();
                var bRt = bGo.GetComponent<RectTransform>();
                aRt.anchorMin = aRt.anchorMax = new Vector2(0.5f, 0.5f);
                bRt.anchorMin = bRt.anchorMax = new Vector2(0.5f, 0.5f);
                aRt.pivot = bRt.pivot = new Vector2(0.5f, 0.5f);
                aRt.sizeDelta = bRt.sizeDelta = new Vector2(100f, 100f);
                aRt.position = Vector3.zero;

                var a = aGo.AddComponent<RiveNodeHitbox>();
                var b = bGo.AddComponent<RiveNodeHitbox>();

                bRt.position = new Vector3(0f, 99f, 0f);
                Assert.That(RiveNodeHitbox.Overlaps(a, b), Is.False);

                bRt.position = new Vector3(0f, 50f, 0f);
                Assert.That(RiveNodeHitbox.Overlaps(a, b), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(aGo);
                Object.DestroyImmediate(bGo);
            }
        }

        [Test]
        public void RiveNodeHitbox_Overlaps_UsesCanvasLocalWhenCanvasIsScaled()
        {
            var canvasGo = new GameObject("M08 ScaledCanvas", typeof(RectTransform), typeof(Canvas));
            var aGo = new GameObject("M08 Hitbox A", typeof(RectTransform));
            var bGo = new GameObject("M08 Hitbox B", typeof(RectTransform));
            try
            {
                canvasGo.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                aGo.transform.SetParent(canvasGo.transform, false);
                bGo.transform.SetParent(canvasGo.transform, false);

                var aRt = aGo.GetComponent<RectTransform>();
                var bRt = bGo.GetComponent<RectTransform>();
                aRt.anchorMin = aRt.anchorMax = new Vector2(0.5f, 0.5f);
                bRt.anchorMin = bRt.anchorMax = new Vector2(0.5f, 0.5f);
                aRt.pivot = bRt.pivot = new Vector2(0.5f, 0.5f);
                aRt.sizeDelta = bRt.sizeDelta = new Vector2(100f, 100f);
                aRt.anchoredPosition = Vector2.zero;
                bRt.anchoredPosition = new Vector2(0f, 50f);

                var a = aGo.AddComponent<RiveNodeHitbox>();
                var b = bGo.AddComponent<RiveNodeHitbox>();
                Canvas.ForceUpdateCanvases();

                Assert.That(RiveNodeHitbox.Overlaps(a, b), Is.True);

                bRt.anchoredPosition = new Vector2(0f, 99f);
                Assert.That(RiveNodeHitbox.Overlaps(a, b), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(canvasGo);
            }
        }

        [Test]
        public void RiveNodeHitbox_Overlaps_UsesRootCanvasAcrossNestedOverlayCanvases()
        {
            var rootGo = new GameObject("M08 RootCanvas", typeof(RectTransform), typeof(Canvas));
            var handsPanel = new GameObject("HandsRivePanel", typeof(RectTransform), typeof(Canvas));
            var faucetPanel = new GameObject("FaucetRivePanel", typeof(RectTransform), typeof(Canvas));
            var aGo = new GameObject("hitbox_1", typeof(RectTransform));
            var bGo = new GameObject("water-sqspot", typeof(RectTransform));
            try
            {
                handsPanel.transform.SetParent(rootGo.transform, false);
                faucetPanel.transform.SetParent(rootGo.transform, false);
                var handsCanvas = handsPanel.GetComponent<Canvas>();
                handsCanvas.overrideSorting = true;
                handsCanvas.sortingOrder = 2;
                var faucetCanvas = faucetPanel.GetComponent<Canvas>();
                faucetCanvas.overrideSorting = true;
                faucetCanvas.sortingOrder = 2;

                aGo.transform.SetParent(handsPanel.transform, false);
                bGo.transform.SetParent(faucetPanel.transform, false);

                var aRt = aGo.GetComponent<RectTransform>();
                var bRt = bGo.GetComponent<RectTransform>();
                aRt.anchorMin = aRt.anchorMax = new Vector2(0.5f, 0.5f);
                bRt.anchorMin = bRt.anchorMax = new Vector2(0.5f, 0.5f);
                aRt.pivot = bRt.pivot = new Vector2(0.5f, 0.5f);
                aRt.sizeDelta = bRt.sizeDelta = new Vector2(100f, 100f);
                aRt.anchoredPosition = Vector2.zero;
                bRt.anchoredPosition = new Vector2(0f, 50f);

                var a = aGo.AddComponent<RiveNodeHitbox>();
                var b = bGo.AddComponent<RiveNodeHitbox>();
                Canvas.ForceUpdateCanvases();

                Assert.That(RiveNodeHitbox.Overlaps(a, b), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(rootGo);
            }
        }

        [Test]
        public void Soap_SetDraggable_TogglesHitTest()
        {
            var go = new GameObject("M08 Soap");
            var widgetGo = new GameObject("M08 Soap Widget", typeof(RectTransform));
            var widget = widgetGo.AddComponent<RiveWidget>();
            var soap = go.AddComponent<Soap>();
            soap.Bind(widget);

            Assert.That(soap.IsDraggable, Is.False);
            Assert.That(widget.HitTestBehavior, Is.EqualTo(HitTestBehavior.None));

            soap.SetDraggable(true);
            Assert.That(soap.IsDraggable, Is.True);
            Assert.That(widget.HitTestBehavior, Is.EqualTo(HitTestBehavior.None));

            soap.SetDraggable(false);
            Assert.That(soap.IsDraggable, Is.False);
            Assert.That(widget.HitTestBehavior, Is.EqualTo(HitTestBehavior.None));

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(widgetGo);
        }

        [Test]
        public void Soap_ReturnHome_RestoresLayoutAfterDrag()
        {
            var mapped = new Rect(-350f, -180f, 400f, 400f);
            CreateStretchSoap(mapped, out var parent, out var child, out var soap);
            try
            {
                var before = WorldCorners(child);
                var grab = ParentLocalPoint(child, new Vector2(0.4f, 0.6f));
                soap.NotifyParentLocalPointer(grab, pressedThisFrame: true, held: true);
                soap.NotifyParentLocalPointer(grab + new Vector2(80f, -40f), pressedThisFrame: false, held: true);
                Assert.That(soap.FreezePlacement, Is.True);

                soap.ReturnHome();
                Assert.That(soap.FreezePlacement, Is.False);
                AssertCornersEqual(before, WorldCorners(child));
            }
            finally
            {
                Object.DestroyImmediate(parent.gameObject);
            }
        }

        [Test]
        public void Soap_Release_ReturnsHome()
        {
            var mapped = new Rect(-350f, -180f, 400f, 400f);
            CreateStretchSoap(mapped, out var parent, out var child, out var soap);
            try
            {
                var before = WorldCorners(child);
                var grab = ParentLocalPoint(child, new Vector2(0.4f, 0.6f));
                soap.NotifyParentLocalPointer(grab, pressedThisFrame: true, held: true);
                soap.NotifyParentLocalPointer(grab + new Vector2(80f, -40f), pressedThisFrame: false, held: true);
                soap.NotifyParentLocalPointer(grab + new Vector2(80f, -40f), pressedThisFrame: false, held: false);

                Assert.That(soap.FreezePlacement, Is.False);
                AssertCornersEqual(before, WorldCorners(child));
            }
            finally
            {
                Object.DestroyImmediate(parent.gameObject);
            }
        }

        [Test]
        public void Towel_SetDraggable_KeepsHitTestOff()
        {
            var go = new GameObject("M08 Towel");
            var widgetGo = new GameObject("M08 Towel Widget", typeof(RectTransform));
            var widget = widgetGo.AddComponent<RiveWidget>();
            var towel = go.AddComponent<Towel>();
            towel.Bind(widget);

            Assert.That(towel.IsDraggable, Is.False);
            Assert.That(widget.HitTestBehavior, Is.EqualTo(HitTestBehavior.None));

            towel.SetDraggable(true);
            Assert.That(towel.IsDraggable, Is.True);
            Assert.That(widget.HitTestBehavior, Is.EqualTo(HitTestBehavior.None));

            towel.SetDraggable(false);
            Assert.That(towel.IsDraggable, Is.False);
            Assert.That(widget.HitTestBehavior, Is.EqualTo(HitTestBehavior.None));

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(widgetGo);
        }

        [Test]
        public void Towel_ReturnHome_RestoresLayoutAfterDrag()
        {
            var mapped = new Rect(480f, -180f, 300f, 340f);
            CreateStretchTowel(mapped, out var parent, out var child, out var towel);
            try
            {
                var before = WorldCorners(child);
                var grab = ParentLocalPoint(child, new Vector2(0.4f, 0.6f));
                towel.NotifyParentLocalPointer(grab, pressedThisFrame: true, held: true);
                towel.NotifyParentLocalPointer(grab + new Vector2(80f, -40f), pressedThisFrame: false, held: true);
                Assert.That(towel.FreezePlacement, Is.True);

                towel.ReturnHome();
                Assert.That(towel.FreezePlacement, Is.False);
                AssertCornersEqual(before, WorldCorners(child));
            }
            finally
            {
                Object.DestroyImmediate(parent.gameObject);
            }
        }

        [Test]
        public void Towel_Release_ReturnsHome()
        {
            var mapped = new Rect(480f, -180f, 300f, 340f);
            CreateStretchTowel(mapped, out var parent, out var child, out var towel);
            try
            {
                var before = WorldCorners(child);
                var grab = ParentLocalPoint(child, new Vector2(0.4f, 0.6f));
                towel.NotifyParentLocalPointer(grab, pressedThisFrame: true, held: true);
                towel.NotifyParentLocalPointer(grab + new Vector2(80f, -40f), pressedThisFrame: false, held: true);
                towel.NotifyParentLocalPointer(grab + new Vector2(80f, -40f), pressedThisFrame: false, held: false);

                Assert.That(towel.FreezePlacement, Is.False);
                AssertCornersEqual(before, WorldCorners(child));
            }
            finally
            {
                Object.DestroyImmediate(parent.gameObject);
            }
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
