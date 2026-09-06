using System;
using ManosLimpias.Core;
using Rive.Components;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ManosLimpias.UI.Rive
{
    /// <summary>
    /// Binds the dedicated hands artboard widget. OpenFaucetStage and
    /// RinseSoapStage enable drag after the faucet is locked open, then fill
    /// only after a grab while a hands hitbox overlaps the faucet water spot.
    /// At 80% both stages disable drag and <see cref="ReturnHome"/> snaps the
    /// panel back before the faucet close tap.
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

        public event Action DragStarted;
        public bool IsDraggable { get; private set; }
        public bool IsGlowing { get; private set; }
        public bool FreezePlacement { get; private set; }

        RectTransform DragRect => RiveGlow.LayoutRect(widget) ?? widget?.RectTransform;
        bool _dragging;
        Vector2 _lastLocal;
        bool _hasLastLocal;
        bool _hasHome;
        Vector2 _homeAnchorMin;
        Vector2 _homeAnchorMax;
        Vector2 _homePivot;
        Vector2 _homeAnchoredPosition;
        Vector2 _homeSizeDelta;
        Vector2 _homeOffsetMin;
        Vector2 _homeOffsetMax;

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
                EndDrag();

            ApplyHitTest();
        }

        public void SetGlow(bool on)
        {
            IsGlowing = on;
            RiveGlow.SetForWidget(widget, on);
        }

        public void ReturnHome()
        {
            EndDrag();
            RestoreHomeLayout();
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
            if (!TryGetPointerScreen(out var screen, out var pressedThisFrame, out var held))
            {
                EndDrag();
                return;
            }

            if (!held)
            {
                EndDrag();
                return;
            }

            if (!TryGetLocalInParent(screen, out var local))
                return;

            NotifyParentLocalPointer(local, pressedThisFrame, held);
        }

        /// <summary>
        /// One pointer sample in the widget parent's local space. Grab keeps the
        /// widget in place; later samples move it by delta only.
        /// </summary>
        public void NotifyParentLocalPointer(Vector2 parentLocal, bool pressedThisFrame, bool held)
        {
            var rectTransform = DragRect;
            if (rectTransform == null)
                return;

            if (!held)
            {
                EndDrag();
                return;
            }

            if (pressedThisFrame && ContainsParentLocal(rectTransform, parentLocal))
            {
                CaptureHome(rectTransform);
                ConvertToFreeLayout(rectTransform);
                FreezePlacement = true;
                _dragging = true;
                _lastLocal = parentLocal;
                _hasLastLocal = true;
                DragStarted?.Invoke();
                return;
            }

            if (!_dragging || !_hasLastLocal)
                return;

            rectTransform.anchoredPosition += parentLocal - _lastLocal;
            _lastLocal = parentLocal;
        }

        void EndDrag()
        {
            _dragging = false;
            _hasLastLocal = false;
        }

        void RestoreHomeLayout()
        {
            FreezePlacement = false;
            var rectTransform = DragRect;
            if (!_hasHome || rectTransform == null)
                return;

            rectTransform.anchorMin = _homeAnchorMin;
            rectTransform.anchorMax = _homeAnchorMax;
            rectTransform.pivot = _homePivot;
            rectTransform.anchoredPosition = _homeAnchoredPosition;
            rectTransform.sizeDelta = _homeSizeDelta;
            rectTransform.offsetMin = _homeOffsetMin;
            rectTransform.offsetMax = _homeOffsetMax;
        }

        void CaptureHome(RectTransform rect)
        {
            if (_hasHome || rect == null)
                return;

            _homeAnchorMin = rect.anchorMin;
            _homeAnchorMax = rect.anchorMax;
            _homePivot = rect.pivot;
            _homeAnchoredPosition = rect.anchoredPosition;
            _homeSizeDelta = rect.sizeDelta;
            _homeOffsetMin = rect.offsetMin;
            _homeOffsetMax = rect.offsetMax;
            _hasHome = true;
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

            var centerPivot = new Vector2(0.5f, 0.5f);
            if (rect.anchorMin == rect.anchorMax && rect.pivot == centerPivot)
                return;

            var size = rect.rect.size;
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            var worldCenter = (corners[0] + corners[2]) * 0.5f;

            rect.anchorMin = rect.anchorMax = centerPivot;
            rect.pivot = centerPivot;
            rect.sizeDelta = size;
            rect.position = worldCenter;
        }

        static bool ContainsParentLocal(RectTransform rectTransform, Vector2 parentLocal)
        {
            var parent = rectTransform.parent as RectTransform;
            if (parent == null)
                return false;

            var world = parent.TransformPoint(parentLocal);
            var local = (Vector2)rectTransform.InverseTransformPoint(world);
            return rectTransform.rect.Contains(local);
        }

        bool TryGetLocalInParent(Vector2 screen, out Vector2 local)
        {
            local = default;
            var rectTransform = DragRect;
            var parent = rectTransform != null ? rectTransform.parent as RectTransform : null;
            if (parent == null)
                return false;

            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent, screen, CanvasCamera(parent), out local);
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
