using Rive.Components;
using UnityEngine;

namespace ManosLimpias.UI.Rive
{
    /// <summary>
    /// Authored Unity hitbox. Parent it under a Rive widget and size it in the
    /// Rect tool; overlap uses canvas-space AABB, not Rive node names.
    /// </summary>
    [ExecuteAlways]
    [DefaultExecutionOrder(110)]
    public sealed class RiveNodeHitbox : MonoBehaviour
    {
        public const string Hitbox1 = "hitbox_1";
        public const string Hitbox2 = "hitbox_2";
        public const string WaterSqspot = "water-sqspot";
        public const float MinOverlap = 8f;

        RectTransform _rect;
        BoxCollider2D _collider;

        public RectTransform RectTransform
        {
            get
            {
                if (_rect == null)
                    _rect = GetComponent<RectTransform>();
                return _rect;
            }
        }

        public static RiveNodeHitbox FindOrCreate(RiveWidget host, string name, Vector4 normalizedMinMax)
        {
            if (host == null || string.IsNullOrEmpty(name))
                return null;

            var existing = host.transform.Find(name);
            bool created = existing == null;
            GameObject go;
            if (existing != null)
            {
                go = existing.gameObject;
                if (go.GetComponent<RectTransform>() == null)
                    go.AddComponent<RectTransform>();
            }
            else
            {
                go = new GameObject(name, typeof(RectTransform));
                go.transform.SetParent(host.transform, false);
            }

            var hitbox = go.GetComponent<RiveNodeHitbox>();
            if (hitbox == null)
                hitbox = go.AddComponent<RiveNodeHitbox>();
            if (go.GetComponent<BoxCollider2D>() == null)
                go.AddComponent<BoxCollider2D>().isTrigger = true;

            if (created)
                hitbox.ApplyNormalized(normalizedMinMax);

            hitbox.SyncCollider();
            return hitbox;
        }

        public void ApplyNormalized(Vector4 minMax)
        {
            var child = RectTransform;
            var parent = child != null ? child.parent as RectTransform : null;
            if (child == null || parent == null)
                return;

            child.anchorMin = new Vector2(minMax.x, minMax.y);
            child.anchorMax = new Vector2(minMax.z, minMax.w);
            child.offsetMin = Vector2.zero;
            child.offsetMax = Vector2.zero;
            child.pivot = new Vector2(0.5f, 0.5f);
            child.localScale = Vector3.one;
            child.localRotation = Quaternion.identity;
        }

        void OnEnable()
        {
            SyncCollider();
        }

        void OnRectTransformDimensionsChange()
        {
            SyncCollider();
        }

        public void SyncCollider()
        {
            var child = RectTransform;
            if (child == null)
                return;

            if (_collider == null)
                _collider = GetComponent<BoxCollider2D>();
            if (_collider == null)
                return;

            _collider.isTrigger = true;
            var size = child.rect.size;
            _collider.size = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));
            _collider.offset = child.rect.center;
        }

        public bool TryGetWorldRect(out Rect world)
        {
            world = default;
            var child = RectTransform;
            if (child == null)
                return false;

            var corners = new Vector3[4];
            child.GetWorldCorners(corners);
            world = Rect.MinMaxRect(
                Mathf.Min(corners[0].x, corners[2].x),
                Mathf.Min(corners[0].y, corners[2].y),
                Mathf.Max(corners[0].x, corners[2].x),
                Mathf.Max(corners[0].y, corners[2].y));
            return ArtboardSpace.HasArea(world, 0.01f);
        }

        /// <summary>
        /// AABB in the root canvas's local units (authored pixel size). Falls
        /// back to world axes when the hitbox is not under a Canvas.
        /// </summary>
        public bool TryGetOverlapRect(out Rect overlapRect)
        {
            overlapRect = default;
            var child = RectTransform;
            if (child == null)
                return false;

            var corners = new Vector3[4];
            child.GetWorldCorners(corners);

            var canvas = child.GetComponentInParent<Canvas>();
            var space = canvas != null ? canvas.transform : null;
            if (space == null)
                return TryGetWorldRect(out overlapRect);

            var p = space.InverseTransformPoint(corners[0]);
            float minX = p.x, maxX = p.x, minY = p.y, maxY = p.y;
            for (int i = 1; i < 4; i++)
            {
                p = space.InverseTransformPoint(corners[i]);
                minX = Mathf.Min(minX, p.x);
                maxX = Mathf.Max(maxX, p.x);
                minY = Mathf.Min(minY, p.y);
                maxY = Mathf.Max(maxY, p.y);
            }

            overlapRect = Rect.MinMaxRect(minX, minY, maxX, maxY);
            return ArtboardSpace.HasArea(overlapRect, 0.01f);
        }

        public static bool Overlaps(RiveNodeHitbox a, RiveNodeHitbox b)
        {
            if (a == null || b == null)
                return false;
            if (!a.TryGetOverlapRect(out var ar) || !b.TryGetOverlapRect(out var br))
                return false;

            float x = Mathf.Min(ar.xMax, br.xMax) - Mathf.Max(ar.xMin, br.xMin);
            float y = Mathf.Min(ar.yMax, br.yMax) - Mathf.Max(ar.yMin, br.yMin);
            return x >= MinOverlap && y >= MinOverlap;
        }

        void OnDrawGizmos()
        {
            if (!TryGetWorldRect(out var world))
                return;
            Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.7f);
            var center = new Vector3(world.center.x, world.center.y, transform.position.z);
            Gizmos.DrawWireCube(center, new Vector3(world.width, world.height, 0.01f));
        }
    }
}
