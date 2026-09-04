using ManosLimpias.Core;
using Rive.Components;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ManosLimpias.UI.Rive
{
    /// <summary>
    /// Binds the dedicated hands artboard widget. OpenFaucetStage enables drag
    /// after the faucet is locked open, and fills from hands/water hitbox overlap.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public sealed class Hands : MonoBehaviour, IHandsControl, IWaterContactControl
    {
        public const string Artboard = SimiPrototypeArtboards.Hands;
        public const string StateMachine = "main";
        public const string WidgetName = "HandsRive";

        public const string IsWashing = "isWashing";
        public const string Hitbox1 = RiveNodeHitbox.Hitbox1;
        public const string Hitbox2 = RiveNodeHitbox.Hitbox2;
        public const string WaterSqspot = RiveNodeHitbox.WaterSqspot;

        public const string AnimIdle = "idle";
        public const string AnimReturn = "return";
        public const string AnimWashPos = "wash_pos";
        public const string AnimDefaultPos = "defaul_pos";

        /// <summary>Normalized min/max (xMin, yMin, xMax, yMax) of the widget rect. Tweak in the Rect tool.</summary>
        public static readonly Vector4 Hitbox1Normalized = new(0.32f, 0.22f, 0.46f, 0.48f);
        public static readonly Vector4 Hitbox2Normalized = new(0.54f, 0.22f, 0.68f, 0.48f);
        public static readonly Vector4 WaterNormalized = new(0.38f, 0.02f, 0.62f, 0.42f);

        public RiveWidget widget;
        public RiveWidget faucetWidget;
        public RiveNodeHitbox hitbox1;
        public RiveNodeHitbox hitbox2;
        public RiveNodeHitbox waterHitbox;

        public bool IsDraggable { get; private set; }
        public bool FreezePlacement { get; private set; }
        bool _dragging;
        Vector2 _lastLocal;
        bool _hasLastLocal;

        public bool IsOverlapping
        {
            get
            {
                EnsureHitboxes();
                return RiveNodeHitbox.Overlaps(hitbox1, waterHitbox) ||
                       RiveNodeHitbox.Overlaps(hitbox2, waterHitbox);
            }
        }

        public void SetDraggable(bool draggable)
        {
            IsDraggable = draggable;
            if (!draggable)
            {
                _dragging = false;
                FreezePlacement = false;
            }

            ApplyHitTest();
        }

        public void Bind(RiveWidget handsWidget, RiveWidget waterWidget = null)
        {
            widget = handsWidget;
            if (waterWidget != null)
                faucetWidget = waterWidget;
            EnsureHitboxes();
            ApplyHitTest();
        }

        void OnEnable()
        {
            ApplyHitTest();
        }

        void Update()
        {
            if (!IsDraggable)
                return;
            PollDrag();
        }

        void PollDrag()
        {
            var rectTransform = widget != null ? widget.RectTransform : null;
            if (rectTransform == null)
                return;

            bool pressed = TryGetPointerScreen(out var screen, out var pressedThisFrame, out var held);
            if (!pressed || !held)
            {
                _dragging = false;
                _hasLastLocal = false;
                return;
            }

            if (pressedThisFrame && TryGetLocalInParent(screen, out _))
            {
                if (RectContainsScreen(rectTransform, screen))
                {
                    ConvertToFreeLayout(rectTransform);
                    FreezePlacement = true;
                    _dragging = true;
                }
            }

            if (!_dragging)
                return;

            if (!TryGetLocalInParent(screen, out var local))
                return;

            if (_hasLastLocal)
                rectTransform.anchoredPosition += local - _lastLocal;

            _lastLocal = local;
            _hasLastLocal = true;
        }

        void EnsureHitboxes()
        {
            if (widget != null)
            {
                if (hitbox1 == null)
                    hitbox1 = RiveNodeHitbox.FindOrCreate(widget, Hitbox1, Hitbox1Normalized);
                if (hitbox2 == null)
                    hitbox2 = RiveNodeHitbox.FindOrCreate(widget, Hitbox2, Hitbox2Normalized);
            }

            if (faucetWidget != null && waterHitbox == null)
                waterHitbox = RiveNodeHitbox.FindOrCreate(faucetWidget, WaterSqspot, WaterNormalized);
        }

        void ApplyHitTest()
        {
            if (widget != null)
                widget.HitTestBehavior = IsDraggable ? HitTestBehavior.Translucent : HitTestBehavior.None;
        }

        static void ConvertToFreeLayout(RectTransform rect)
        {
            if (rect == null)
                return;
            if (rect.anchorMin == rect.anchorMax)
                return;

            var size = rect.rect.size;
            var world = rect.position;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.position = world;
        }

        bool RectContainsScreen(RectTransform rectTransform, Vector2 screen)
        {
            return TryGetNormalizedPoint(rectTransform, screen, out _);
        }

        bool TryGetLocalInParent(Vector2 screen, out Vector2 local)
        {
            local = default;
            var rectTransform = widget != null ? widget.RectTransform : null;
            var parent = rectTransform != null ? rectTransform.parent as RectTransform : null;
            if (parent == null)
                return false;

            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent, screen, CanvasCamera(parent), out local);
        }

        bool TryGetNormalizedPoint(RectTransform rectTransform, Vector2 screen, out Vector2 normalized)
        {
            normalized = default;
            if (rectTransform == null)
                return false;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform, screen, CanvasCamera(rectTransform), out var local))
                return false;

            var rect = rectTransform.rect;
            if (rect.width <= 0f || rect.height <= 0f || !rect.Contains(local))
                return false;

            normalized = new Vector2(
                (local.x - rect.xMin) / rect.width,
                (local.y - rect.yMin) / rect.height);
            return true;
        }

        static Camera CanvasCamera(RectTransform rectTransform)
        {
            var canvas = rectTransform.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                return canvas.worldCamera;
            return null;
        }

        static bool TryGetPointerScreen(out Vector2 screen, out bool pressedThisFrame, out bool held)
        {
            if (Pointer.current != null)
            {
                screen = Pointer.current.position.ReadValue();
                pressedThisFrame = Pointer.current.press.wasPressedThisFrame;
                held = Pointer.current.press.isPressed;
                return true;
            }

            if (Mouse.current != null)
            {
                screen = Mouse.current.position.ReadValue();
                pressedThisFrame = Mouse.current.leftButton.wasPressedThisFrame;
                held = Mouse.current.leftButton.isPressed;
                return true;
            }

            screen = default;
            pressedThisFrame = false;
            held = false;
            return false;
        }
    }
}
