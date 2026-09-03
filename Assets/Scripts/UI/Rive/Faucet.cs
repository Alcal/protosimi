using System;
using ManosLimpias.Core;
using Rive;
using Rive.Components;
using UnityEngine;

namespace ManosLimpias.UI.Rive
{
    /// <summary>
    /// Binds the dedicated faucet artboard widget. Pointer hits fire SM triggers that
    /// may not always surface as ReportedEvents, so this adapter also polls bool/trigger
    /// inputs on the widget's own state machine.
    /// </summary>
    [DefaultExecutionOrder(-100)]
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
        public bool IsEnabled { get; private set; }

        bool _inputsResolved;
        bool _leftWasOn;
        bool _rightWasOn;
        SMIBool _leftBool;
        SMIBool _rightBool;

        public void SetEnabled(bool enabled)
        {
            IsEnabled = enabled;
            ApplyHitTest();
            if (!enabled)
            {
                _leftWasOn = false;
                _rightWasOn = false;
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
            _leftBool = null;
            _rightBool = null;
            ApplyHitTest();
        }

        void Update()
        {
            if (!IsEnabled || widget?.StateMachine == null)
                return;

            ResolveInputs();
            if (_leftBool != null)
                PollBool(_leftBool, FaucetSide.Left, ref _leftWasOn);
            else
                PollTrigger(FaucetLOn, FaucetSide.Left, ref _leftWasOn);

            if (_rightBool != null)
                PollBool(_rightBool, FaucetSide.Right, ref _rightWasOn);
            else
                PollTrigger(FaucetROn, FaucetSide.Right, ref _rightWasOn);
        }

        void PollBool(SMIBool input, FaucetSide side, ref bool wasOn)
        {
            var isOn = input != null && input.Value;
            if (isOn && !wasOn)
            {
                Debug.Log($"[Faucet] Bool '{input.Name}'");
                NotifyActivated(side);
            }
            wasOn = isOn;
        }

        void PollTrigger(string inputName, FaucetSide side, ref bool wasOn)
        {
            if (widget?.Artboard == null)
                return;

            var isOn = RiveStateMachineInputs.TryReadNestedTrigger(widget.Artboard, inputName, string.Empty);
            if (isOn && !wasOn)
            {
                Debug.Log($"[Faucet] Trigger '{inputName}'");
                NotifyActivated(side);
            }
            wasOn = isOn;
        }

        void OnRiveEventReported(ReportedEvent report)
        {
            if (report == null) return;
            Debug.Log($"[Faucet] Rive event '{report.Name}' enabled={IsEnabled}");
            if (!IsEnabled) return;
            if (IsLeftOn(report.Name))
                NotifyActivated(FaucetSide.Left);
            else if (IsRightOn(report.Name))
                NotifyActivated(FaucetSide.Right);
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

        public void ActivateLeft() => Activate(FaucetSide.Left);

        public void ActivateRight() => Activate(FaucetSide.Right);

        public void Activate(FaucetSide side)
        {
            if (!IsEnabled) return;
            if (fireInput && widget?.StateMachine != null)
            {
                string inputName = side == FaucetSide.Left ? FaucetLOn : FaucetROn;
                RiveStateMachineInputs.FindTrigger(widget.StateMachine, inputName)?.Fire();
            }

            NotifyActivated(side);
        }

        void ResolveInputs()
        {
            if (_inputsResolved || widget?.StateMachine == null)
                return;

            _inputsResolved = true;
            _leftBool = RiveStateMachineInputs.GetBool(widget.StateMachine, FaucetLOn);
            _rightBool = RiveStateMachineInputs.GetBool(widget.StateMachine, FaucetROn);
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

        void NotifyActivated(FaucetSide side)
        {
            Debug.Log($"[Faucet] Activated {side} listeners={Activated?.GetInvocationList().Length ?? 0}");
            Activated?.Invoke(side);
        }
    }
}
