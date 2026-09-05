using System;
using Rive.Components;
using UnityEngine;

namespace ManosLimpias.UI.Rive
{
    /// <summary>
    /// Positions sibling Rive widgets from named nodes on the background artboard.
    /// Empty Rive groups have no drawable AABB, so placement uses Node x/y plus the
    /// component artboard size, shifted by that artboard's origin (0–1), then mapped
    /// through Fit.Contain + Center with a Y-flip.
    /// </summary>
    public sealed class RiveAnchorMount : MonoBehaviour
    {
        [Serializable]
        public sealed class Slot
        {
            public string anchorName;
            public RiveWidget widget;
        }

        public RiveWidget backgroundWidget;
        public Slot[] slots = Array.Empty<Slot>();

        Vector2 _lastParentSize;
        bool _applied;

        public void Bind(RiveWidget background, Slot[] mountSlots)
        {
            backgroundWidget = background;
            slots = mountSlots ?? Array.Empty<Slot>();
            _applied = false;
            TryApply();
        }

        void OnEnable()
        {
            if (backgroundWidget != null)
            {
                backgroundWidget.OnWidgetStatusChanged += OnBackgroundStatusChanged;
                if (backgroundWidget.Status == WidgetStatus.Loaded)
                    TryApply();
            }
        }

        void OnDisable()
        {
            if (backgroundWidget != null)
                backgroundWidget.OnWidgetStatusChanged -= OnBackgroundStatusChanged;
        }

        void LateUpdate()
        {
            TryApply(forceIfSizeChanged: true);
        }

        void OnBackgroundStatusChanged()
        {
            if (backgroundWidget != null && backgroundWidget.Status == WidgetStatus.Loaded)
            {
                _applied = false;
                TryApply();
            }
        }

        public bool TryApply(bool forceIfSizeChanged = false)
        {
            if (backgroundWidget == null || slots == null || slots.Length == 0)
                return false;

            var backgroundRect = backgroundWidget.GetComponent<RectTransform>();
            if (backgroundRect == null)
                return false;

            var view = backgroundRect.rect;
            if (!ArtboardSpace.HasArea(view))
            {
                _applied = false;
                return false;
            }

            var parentSize = view.size;
            if (_applied && forceIfSizeChanged && parentSize == _lastParentSize)
                return true;
            if (_applied && !forceIfSizeChanged)
                return true;

            var artboard = backgroundWidget.Artboard;
            var artboardSize = artboard != null
                ? new Vector2(artboard.Width, artboard.Height)
                : SimiPrototypeArtboards.BackgroundSize;
            if (artboardSize.x <= 0f || artboardSize.y <= 0f)
                artboardSize = SimiPrototypeArtboards.BackgroundSize;

            var placed = 0;
            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                if (slot == null || slot.widget == null || string.IsNullOrEmpty(slot.anchorName))
                    continue;

                var child = RiveGlow.LayoutRect(slot.widget);
                if (child == null)
                    continue;

                var hands = slot.widget.GetComponent<Hands>();
                if (hands != null && hands.FreezePlacement)
                {
                    placed++;
                    continue;
                }

                if (!TryResolveAabb(slot, out var aabb))
                {
                    Debug.LogWarning($"[RiveAnchorMount] No origin/size for '{slot.anchorName}'.");
                    continue;
                }

                var mapped = ArtboardSpace.MapAabbToView(
                    aabb.xMin,
                    aabb.yMin,
                    aabb.xMax,
                    aabb.yMax,
                    artboardSize,
                    view);
                if (!ArtboardSpace.HasArea(mapped))
                    continue;

                var host = child.parent as RectTransform ?? backgroundRect;
                var mappedInHost = mapped;
                if (host != backgroundRect)
                    mappedInHost = MapRectToHost(mapped, view, host.rect);

                ArtboardSpace.ApplyNormalizedAnchors(
                    child,
                    host,
                    mappedInHost,
                    ArtboardSpace.UnityPivotFromRiveOrigin(SimiPrototypeArtboards.OriginForAnchor(slot.anchorName)));
                if (child != slot.widget.RectTransform)
                    ArtboardSpace.StretchFill(slot.widget.RectTransform);
                placed++;
            }

            _applied = placed > 0;
            _lastParentSize = parentSize;
            if (_applied)
                Debug.Log($"[RiveAnchorMount] Placed {placed}/{slots.Length} widgets from background anchors.");
            return _applied;
        }

        static bool TryResolveAabb(Slot slot, out Rect aabb)
        {
            var size = BackgroundAnchors.SizeFor(slot.anchorName);
            var artboard = slot.widget != null ? slot.widget.Artboard : null;
            if (artboard != null && artboard.Width > 1f && artboard.Height > 1f)
                size = new Vector2(artboard.Width, artboard.Height);

            return BackgroundAnchors.TryGetArtboardAabb(slot.anchorName, size, out aabb);
        }

        static Rect MapRectToHost(Rect mappedInBackground, Rect backgroundView, Rect hostView)
        {
            if (!ArtboardSpace.HasArea(backgroundView) || !ArtboardSpace.HasArea(hostView))
                return mappedInBackground;

            float nx0 = (mappedInBackground.xMin - backgroundView.xMin) / backgroundView.width;
            float ny0 = (mappedInBackground.yMin - backgroundView.yMin) / backgroundView.height;
            float nx1 = (mappedInBackground.xMax - backgroundView.xMin) / backgroundView.width;
            float ny1 = (mappedInBackground.yMax - backgroundView.yMin) / backgroundView.height;
            return Rect.MinMaxRect(
                hostView.xMin + nx0 * hostView.width,
                hostView.yMin + ny0 * hostView.height,
                hostView.xMin + nx1 * hostView.width,
                hostView.yMin + ny1 * hostView.height);
        }
    }
}
