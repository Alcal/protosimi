using UnityEngine;

namespace ManosLimpias.UI.Rive
{
    /// <summary>
    /// Maps Rive artboard AABBs through Fit.Contain + Center into a Unity view rect.
    /// Rive artboard space is Y-down with origin at the top-left; uGUI is Y-up.
    /// </summary>
    public static class ArtboardSpace
    {
        public static Rect RectFromOriginSize(Vector2 node, Vector2 size)
        {
            return RectFromOriginSize(node, size, Vector2.zero);
        }

        /// <summary>
        /// Artboard box in Rive space: <paramref name="artboardOrigin01"/> (0–1) sits on
        /// <paramref name="node"/>. (0,0) is top-left; (0.5, 0.5) is the center.
        /// </summary>
        public static Rect RectFromOriginSize(Vector2 node, Vector2 size, Vector2 artboardOrigin01)
        {
            float ox = Mathf.Clamp01(artboardOrigin01.x);
            float oy = Mathf.Clamp01(artboardOrigin01.y);
            float left = node.x - Mathf.Max(0f, size.x) * ox;
            float top = node.y - Mathf.Max(0f, size.y) * oy;
            return new Rect(left, top, Mathf.Max(0f, size.x), Mathf.Max(0f, size.y));
        }

        public static Vector2 UnityPivotFromRiveOrigin(Vector2 riveOrigin01)
        {
            return new Vector2(Mathf.Clamp01(riveOrigin01.x), 1f - Mathf.Clamp01(riveOrigin01.y));
        }

        public static bool HasArea(Rect rect, float epsilon = 0.5f)
        {
            return rect.width > epsilon && rect.height > epsilon;
        }

        public static Rect MapAabbToView(
            float minX,
            float minY,
            float maxX,
            float maxY,
            Vector2 artboardSize,
            Rect view)
        {
            if (artboardSize.x <= 0f || artboardSize.y <= 0f || view.width <= 0f || view.height <= 0f)
                return new Rect(view.x, view.y, 0f, 0f);

            float scale = Mathf.Min(view.width / artboardSize.x, view.height / artboardSize.y);
            float drawW = artboardSize.x * scale;
            float drawH = artboardSize.y * scale;
            float padX = (view.width - drawW) * 0.5f;
            float padY = (view.height - drawH) * 0.5f;

            float riveMinX = Mathf.Min(minX, maxX);
            float riveMaxX = Mathf.Max(minX, maxX);
            float riveMinY = Mathf.Min(minY, maxY);
            float riveMaxY = Mathf.Max(minY, maxY);

            float x0 = view.xMin + padX + riveMinX * scale;
            float x1 = view.xMin + padX + riveMaxX * scale;
            float y0 = view.yMin + padY + (artboardSize.y - riveMaxY) * scale;
            float y1 = view.yMin + padY + (artboardSize.y - riveMinY) * scale;
            return Rect.MinMaxRect(x0, y0, x1, y1);
        }

        /// <summary>
        /// Maps a Rive AABB into a view that uses Fit.Fill (artboard stretched to the view).
        /// </summary>
        public static Rect MapAabbToViewFill(
            float minX,
            float minY,
            float maxX,
            float maxY,
            Vector2 artboardSize,
            Rect view)
        {
            if (artboardSize.x <= 0f || artboardSize.y <= 0f || view.width <= 0f || view.height <= 0f)
                return new Rect(view.x, view.y, 0f, 0f);

            float riveMinX = Mathf.Min(minX, maxX);
            float riveMaxX = Mathf.Max(minX, maxX);
            float riveMinY = Mathf.Min(minY, maxY);
            float riveMaxY = Mathf.Max(minY, maxY);

            float x0 = view.xMin + riveMinX / artboardSize.x * view.width;
            float x1 = view.xMin + riveMaxX / artboardSize.x * view.width;
            float y0 = view.yMin + (artboardSize.y - riveMaxY) / artboardSize.y * view.height;
            float y1 = view.yMin + (artboardSize.y - riveMinY) / artboardSize.y * view.height;
            return Rect.MinMaxRect(x0, y0, x1, y1);
        }

        public static Vector4 ToNormalizedAnchors(Rect mapped, Rect view)
        {
            if (view.width <= 0f || view.height <= 0f)
                return Vector4.zero;

            return new Vector4(
                (mapped.xMin - view.xMin) / view.width,
                (mapped.yMin - view.yMin) / view.height,
                (mapped.xMax - view.xMin) / view.width,
                (mapped.yMax - view.yMin) / view.height);
        }

        public static void ApplyNormalizedAnchors(
            RectTransform child,
            RectTransform parent,
            Rect mappedInParent,
            Vector2 unityPivot)
        {
            if (child == null || parent == null)
                return;

            var anchors = ToNormalizedAnchors(mappedInParent, parent.rect);
            child.anchorMin = new Vector2(anchors.x, anchors.y);
            child.anchorMax = new Vector2(anchors.z, anchors.w);
            child.offsetMin = Vector2.zero;
            child.offsetMax = Vector2.zero;
            child.pivot = unityPivot;
            child.localScale = Vector3.one;
            child.localRotation = Quaternion.identity;
        }

        public static void ApplyNormalizedAnchors(RectTransform child, RectTransform parent, Rect mappedInParent)
        {
            ApplyNormalizedAnchors(child, parent, mappedInParent, new Vector2(0.5f, 0.5f));
        }
    }
}
