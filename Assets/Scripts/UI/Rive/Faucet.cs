using System;
using System.Collections.Generic;
using ManosLimpias.Core;
using Rive;
using Rive.Components;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ManosLimpias.UI.Rive
{
    /// <summary>
    /// Binds the dedicated faucet artboard widget. OpenFaucetStage locks the faucet
    /// open from a pointer press inside this widget; Close Water should not subscribe
    /// to <see cref="PointerHit"/>.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public sealed class Faucet : MonoBehaviour, IFaucetControl
    {
        public const string Artboard = SimiPrototypeArtboards.Faucet;
        public const string StateMachine = "faucet_StateMachine";
        public const string WidgetName = "FaucetRive";

        public const string FaucetLOn = "faucet_L_On";
        public const string FaucetLOff = "faucet_L_Off";
        public const string FaucetROn = "faucet_R_On";
        public const string FaucetROff = "faucet_R_Off";

        public const string AnimOffR = "off_R";
        public const string AnimOffL = "offL";
        public const string AnimActiveL = "active_L";
        public const string AnimActiveR = "activeR";
        public const string AnimWater = "water";
        public const string AnimWaterOff = "watreroff";
        public const string AnimOffSink = "off sink";

        public bool fireInput;
        public RiveWidget widget;

        public event Action<FaucetSide> Activated;
        public event Action<FaucetSide> PointerHit;
        public bool IsEnabled { get; private set; }
        public bool LeftIsOpen { get; private set; }
        public bool RightIsOpen { get; private set; }
        public bool IsOpen => LeftIsOpen || RightIsOpen;

        bool _inputsResolved;
        bool _inputsLogged;
        bool _pendingOpenVisual;
        FaucetSide _pendingOpenSide;
        SMIInput _leftOnInput;
        SMIInput _rightOnInput;
        SMIInput _leftOffInput;
        SMIInput _rightOffInput;
        readonly List<string> _changedStates = new();

        public void SetEnabled(bool enabled)
        {
            IsEnabled = enabled;
            ApplyHitTest();
            if (enabled)
                _pendingOpenVisual = false;
            else
                ClearOpenState();
        }

        public void LockOpen(FaucetSide side)
        {
            IsEnabled = false;
            ApplyHitTest();
            SetOpen(side, true);
            if (!TryFireOpenTrigger(side))
            {
                _pendingOpenSide = side;
                _pendingOpenVisual = true;
            }
        }

        void OnEnable()
        {
            Subscribe();
            ApplyHitTest();
        }

        void OnDisable()
        {
            Unsubscribe();
        }

        public void Bind(RiveWidget riveWidget)
        {
            if (widget != riveWidget)
            {
                Unsubscribe();
                widget = riveWidget;
                Subscribe();
            }

            _inputsResolved = false;
            _inputsLogged = false;
            _pendingOpenVisual = false;
            _leftOnInput = null;
            _rightOnInput = null;
            _leftOffInput = null;
            _rightOffInput = null;
            ApplyHitTest();
        }

        void LateUpdate()
        {
            if (_pendingOpenVisual && TryFireOpenTrigger(_pendingOpenSide))
                _pendingOpenVisual = false;

            if (!IsEnabled)
                return;

            if (widget?.StateMachine != null)
            {
                ResolveInputs();
                PollChangedStates();
                PollInput(_leftOnInput, FaucetSide.Left, open: true);
                PollInput(_rightOnInput, FaucetSide.Right, open: true);
                PollInput(_leftOffInput, FaucetSide.Left, open: false);
                PollInput(_rightOffInput, FaucetSide.Right, open: false);
                PollReportedEvents();
            }

            if (IsEnabled)
                PollPointerHit();
        }

        void PollPointerHit()
        {
            if (!TryGetPressScreenPoint(out var screen))
                return;
            if (!TryGetNormalizedPoint(screen, out var normalized))
                return;

            var side = normalized.x < 0.5f ? FaucetSide.Left : FaucetSide.Right;
            NotifyPointerHit(side);
        }

        static bool TryGetPressScreenPoint(out Vector2 screen)
        {
            if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
            {
                screen = Pointer.current.position.ReadValue();
                return true;
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                screen = Mouse.current.position.ReadValue();
                return true;
            }

            screen = default;
            return false;
        }

        bool TryGetNormalizedPoint(Vector2 screen, out Vector2 normalized)
        {
            normalized = default;
            var rectTransform = widget != null ? widget.RectTransform : null;
            if (rectTransform == null)
                return false;

            var canvas = rectTransform.GetComponentInParent<Canvas>();
            Camera camera = null;
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                camera = canvas.worldCamera;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform, screen, camera, out var local))
                return false;

            var rect = rectTransform.rect;
            if (rect.width <= 0f || rect.height <= 0f || !rect.Contains(local))
                return false;

            normalized = new Vector2(
                (local.x - rect.xMin) / rect.width,
                (local.y - rect.yMin) / rect.height);
            return true;
        }

        void PollChangedStates()
        {
            if (!RiveStateMachineInputs.TryCopyChangedStateNames(widget.StateMachine, _changedStates))
                return;

            for (int i = 0; i < _changedStates.Count; i++)
                ApplyStateName(_changedStates[i]);
        }

        void ApplyStateName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return;

            if (name == AnimActiveL)
                SetOpen(FaucetSide.Left, true);
            else if (name == AnimOffL)
                SetOpen(FaucetSide.Left, false);
            else if (name == AnimActiveR)
                SetOpen(FaucetSide.Right, true);
            else if (name == AnimOffR)
                SetOpen(FaucetSide.Right, false);
            else if (name == AnimWater && !IsOpen)
                SetOpen(FaucetSide.Left, true);
        }

        void PollInput(SMIInput input, FaucetSide side, bool open)
        {
            if (input == null)
                return;
            if (input.IsBoolean)
            {
                var boolInput = input as SMIBool;
                if (boolInput != null && boolInput.Value)
                    SetOpen(side, open);
                return;
            }

            if (RiveStateMachineInputs.TryReadInputBool(input, out var pulsed) && pulsed)
                SetOpen(side, open);
        }

        void PollReportedEvents()
        {
            var stateMachine = widget.StateMachine;
            if (stateMachine == null)
                return;

            foreach (var report in stateMachine.EnumerateReportedEvents())
                ApplyReportedName(report?.Name);
        }

        void OnRiveEventReported(ReportedEvent report)
        {
            if (report == null) return;
            Debug.Log($"[Faucet] Rive event '{report.Name}' enabled={IsEnabled}");
            if (!IsEnabled) return;
            ApplyReportedName(report.Name);
        }

        void ApplyReportedName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return;
            if (IsLeftOn(name))
                SetOpen(FaucetSide.Left, true);
            else if (IsRightOn(name))
                SetOpen(FaucetSide.Right, true);
            else if (IsLeftOff(name))
                SetOpen(FaucetSide.Left, false);
            else if (IsRightOff(name))
                SetOpen(FaucetSide.Right, false);
            else
                ApplyStateName(name);
        }

        static bool IsLeftOn(string name)
        {
            return name == FaucetLOn ||
                   string.Equals(name, "faucet_L_ON", StringComparison.OrdinalIgnoreCase);
        }

        static bool IsRightOn(string name)
        {
            return name == FaucetROn ||
                   string.Equals(name, "faucet_R_ON", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(name, "faucet___R_ON", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(name, "faucet__R_ON", StringComparison.OrdinalIgnoreCase);
        }

        static bool IsLeftOff(string name)
        {
            return name == FaucetLOff ||
                   string.Equals(name, "faucet_L_OFF", StringComparison.OrdinalIgnoreCase);
        }

        static bool IsRightOff(string name)
        {
            return name == FaucetROff ||
                   string.Equals(name, "faucet_R_OFF", StringComparison.OrdinalIgnoreCase);
        }

        public void NotifyPointerHit() => NotifyPointerHit(FaucetSide.Left);

        public void NotifyPointerHit(FaucetSide side)
        {
            if (!IsEnabled) return;
            Debug.Log($"[Faucet] PointerHit {side} listeners={PointerHit?.GetInvocationList().Length ?? 0}");
            PointerHit?.Invoke(side);
        }

        public void ActivateLeft() => Activate(FaucetSide.Left);

        public void ActivateRight() => Activate(FaucetSide.Right);

        public void Activate(FaucetSide side)
        {
            if (!IsEnabled) return;
            FireOpenTrigger(side);
            SetOpen(side, true);
        }

        bool TryFireOpenTrigger(FaucetSide side)
        {
            ResolveInputs();
            if (widget?.StateMachine == null)
                return false;

            FireOpenTrigger(side);
            Debug.Log($"[Faucet] LockOpen fired {(side == FaucetSide.Left ? FaucetLOn : FaucetROn)}.");
            return true;
        }

        void FireOpenTrigger(FaucetSide side)
        {
            ResolveInputs();
            FireInput(side == FaucetSide.Left ? _leftOnInput : _rightOnInput);
        }

        static void FireInput(SMIInput input)
        {
            if (input == null)
                return;
            if (input is SMITrigger trigger)
                trigger.Fire();
            else if (input is SMIBool boolean)
                boolean.Value = true;
        }

        void ResolveInputs()
        {
            if (_inputsResolved || widget?.StateMachine == null)
                return;

            _inputsResolved = true;
            var sm = widget.StateMachine;
            _leftOnInput = RiveStateMachineInputs.FindBool(sm, FaucetLOn)
                           ?? (SMIInput)RiveStateMachineInputs.FindTrigger(sm, FaucetLOn);
            _rightOnInput = RiveStateMachineInputs.FindBool(sm, FaucetROn)
                            ?? (SMIInput)RiveStateMachineInputs.FindTrigger(sm, FaucetROn);
            _leftOffInput = RiveStateMachineInputs.FindBool(sm, FaucetLOff)
                            ?? (SMIInput)RiveStateMachineInputs.FindTrigger(sm, FaucetLOff);
            _rightOffInput = RiveStateMachineInputs.FindBool(sm, FaucetROff)
                             ?? (SMIInput)RiveStateMachineInputs.FindTrigger(sm, FaucetROff);

            if (_inputsLogged)
                return;
            _inputsLogged = true;
            Debug.Log($"[Faucet] SM inputs: {RiveStateMachineInputs.DescribeInputs(sm)}");
        }

        void Subscribe()
        {
            if (widget != null)
                widget.OnRiveEventReported += OnRiveEventReported;
        }

        void Unsubscribe()
        {
            if (widget != null)
                widget.OnRiveEventReported -= OnRiveEventReported;
        }

        void ApplyHitTest()
        {
            if (widget != null)
                widget.HitTestBehavior = IsEnabled ? HitTestBehavior.Translucent : HitTestBehavior.None;
        }

        void ClearOpenState()
        {
            LeftIsOpen = false;
            RightIsOpen = false;
        }

        void SetOpen(FaucetSide side, bool open)
        {
            bool wasOpen = side == FaucetSide.Left ? LeftIsOpen : RightIsOpen;
            if (side == FaucetSide.Left)
                LeftIsOpen = open;
            else
                RightIsOpen = open;

            if (open && !wasOpen)
            {
                Debug.Log($"[Faucet] Activated {side} listeners={Activated?.GetInvocationList().Length ?? 0}");
                Activated?.Invoke(side);
            }
        }
    }
}
