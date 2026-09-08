using System;
using ManosLimpias.Core;
using Rive.Components;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ManosLimpias.UI.Rive
{
    /// <summary>
    /// Binds the dedicated soap artboard widget. ApplySoapStage enables Unity
    /// panel drag (Rive hit testing stays off so the SM cannot lift the art
    /// off the artboard). Overlap with a hands hitbox fills progress; release
    /// snaps the panel home.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public sealed class Soap : MonoBehaviour, ISoapControl
    {
        public const string Artboard = SimiPrototypeArtboards.Soap;
        public const string StateMachine = "soap_StateMachine";
        public const string WidgetName = "SoapRive";
        public const string Hitbox = "soap-hitbox";

        public const string IsDragged = "isDragged";

        public const string AnimMain = "main";

        /// <summary>Normalized min/max (xMin, yMin, xMax, yMax) of the widget rect. Tweak in the Rect tool.</summary>
        public static readonly Vector4 HitboxNormalized = new(0.25f, 0.25f, 0.75f, 0.75f);

        public RiveWidget widget;
        public Hands hands;
        public RiveNodeHitbox hitbox;

        public event Action DragStarted;
        public bool IsDraggable { get; private set; }
        public bool IsGlowing { get; private set; }
        public bool FreezePlacement { get; private set; }

        RectTransform DragRect => RiveGlow.LayoutRect(widget) ?? widget?.RectTransform;
        bool _dragging;
        Vector2 _lastLocal;
        bool _hasLastLocal;
        bool _hasHome;
        bool _loggedInputs;
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
                EnsureHitbox();
                if (hands == null)
                    return false;
                return RiveNodeHitbox.Overlaps(hitbox, hands.hitbox1) ||
                       RiveNodeHitbox.Overlaps(hitbox, hands.hitbox2);
            }
        }

        public void SetDraggable(bool draggable)
        {
            IsDraggable = draggable;
            if (!draggable)
                EndDrag(restoreHome: true);

            ApplyHitTest();
        }

        public void SetGlow(bool on)
        {
            IsGlowing = on;
            RiveGlow.SetForWidget(widget, on);
        }

        public void ReturnHome()
        {
            EndDrag(restoreHome: false);
            RestoreHomeLayout();
        }

        public void Bind(RiveWidget soapWidget, Hands handsTarget = null)
        {
            widget = soapWidget;
            if (handsTarget != null)
                hands = handsTarget;
            EnsureHitbox();
            ApplyHitTest();
            PinIsDraggedFalse();
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

        void LateUpdate()
        {
            PinIsDraggedFalse();
        }

        void PollDrag()
        {
            if (!TryGetPointerScreen(out var screen, out var pressedThisFrame, out var held))
            {
                EndDrag(restoreHome: true);
                return;
            }

            if (!held)
            {
                EndDrag(restoreHome: true);
                return;
            }

            if (!TryGetLocalInParent(screen, out var local))
                return;

            NotifyParentLocalPointer(local, pressedThisFrame, held);
        }

        /// <summary>
        /// One pointer sample in the widget parent's local space. Grab keeps the
        /// widget in place; later samples move it by delta only. Release snaps home.
        /// </summary>
        public void NotifyParentLocalPointer(Vector2 parentLocal, bool pressedThisFrame, bool held)
        {
            var rectTransform = DragRect;
            if (rectTransform == null)
                return;

            if (!held)
            {
                EndDrag(restoreHome: true);
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
                PinIsDraggedFalse();
                DragStarted?.Invoke();
                return;
            }

            if (!_dragging || !_hasLastLocal)
                return;

            rectTransform.anchoredPosition += parentLocal - _lastLocal;
            _lastLocal = parentLocal;
        }

        void EndDrag(bool restoreHome)
        {
            if (!_dragging)
                return;
            _dragging = false;
            _hasLastLocal = false;
            PinIsDraggedFalse();
            if (restoreHome)
                RestoreHomeLayout();
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

        void EnsureHitbox()
        {
            if (widget != null && hitbox == null)
                hitbox = RiveNodeHitbox.FindOrCreate(widget, Hitbox, HitboxNormalized);
        }

        void ApplyHitTest()
        {
            // Unity PollDrag uses the widget rect; Rive Translucent hits let the
            // soap SM run isDragged and clip the bar off the 400×400 artboard.
            if (widget != null)
                widget.HitTestBehavior = HitTestBehavior.None;
        }

        /// <summary>
        /// Soap's SM uses isDragged for an authored lift pose that draws outside
        /// the artboard. Unity already moves the panel, so keep the input false.
        /// </summary>
        void PinIsDraggedFalse()
        {
            if (widget?.StateMachine == null)
                return;
            if (!_loggedInputs)
            {
                _loggedInputs = true;
                Debug.Log($"[Soap] SM inputs: {RiveStateMachineInputs.DescribeInputs(widget.StateMachine)}");
            }

            var input = RiveStateMachineInputs.FindBool(widget.StateMachine, IsDragged, "is_dragged", "IsDragged");
            if (input != null)
                input.Value = false;
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
            return RiveNodeHitbox.EventCamera(rectTransform);
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
