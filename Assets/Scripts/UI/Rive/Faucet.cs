using System;
using ManosLimpias.Core;
using Rive;
using Rive.Components;
using UnityEngine;

namespace ManosLimpias.UI.Rive
{
    /// <summary>
    /// Nested faucet listeners (faucet_L_ON, faucet___R_ON, …) fire trigger inputs
    /// (faucet_L_On / faucet_R_On). Those triggers never surface as ReportedEvents on
    /// main's progress_StateMachine, so this adapter polls the nested inputs.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class Faucet : MonoBehaviour, IFaucetControl
    {
        public const string Artboard = SimiPrototypeArtboards.Faucet;
        public const string StateMachine = "faucet_StateMachine";

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

        static readonly string[] NestedInstancePaths =
        {
            "faucet",
            "Faucet",
            "faucet 1",
            Artboard,
        };

        [Tooltip("Nested instance path inside the main artboard. Override this if the Rive hierarchy uses another instance name.")]
        public string nestedPath;
        [Tooltip("Use only when a valid nested instance path is known. Rive pointer input normally drives the Faucet directly.")]
        public bool fireNestedInput;
        public RiveWidget mainWidget;

        public event Action<FaucetSide> Activated;
        public bool IsEnabled { get; private set; } = true;

        bool _nestedResolved;
        bool _loggedMissingPath;
        bool _leftWasOn;
        bool _rightWasOn;

        public void SetEnabled(bool enabled)
        {
            IsEnabled = enabled;
            if (!enabled)
            {
                _leftWasOn = false;
                _rightWasOn = false;
            }
        }

        void OnEnable()
        {
            if (mainWidget != null)
                mainWidget.OnRiveEventReported += OnRiveEventReported;
            _nestedResolved = false;
        }

        void OnDisable()
        {
            if (mainWidget != null)
                mainWidget.OnRiveEventReported -= OnRiveEventReported;
        }

        public void Bind(RiveWidget widget)
        {
            if (mainWidget != widget)
            {
                if (mainWidget != null)
                    mainWidget.OnRiveEventReported -= OnRiveEventReported;
                mainWidget = widget;
                if (isActiveAndEnabled && mainWidget != null)
                    mainWidget.OnRiveEventReported += OnRiveEventReported;
            }

            _nestedResolved = false;
            _loggedMissingPath = false;
            TryResolveNested();
        }

        void Update()
        {
            TryResolveNested();
            if (!IsEnabled || mainWidget?.Artboard == null || string.IsNullOrEmpty(nestedPath))
                return;

            PollTrigger(FaucetLOn, FaucetSide.Left, ref _leftWasOn);
            PollTrigger(FaucetROn, FaucetSide.Right, ref _rightWasOn);
        }

        void PollTrigger(string inputName, FaucetSide side, ref bool wasOn)
        {
            var isOn = RiveStateMachineInputs.TryReadNestedTrigger(mainWidget.Artboard, inputName, nestedPath);
            if (isOn && !wasOn)
            {
                Debug.Log($"[Faucet] Nested trigger '{inputName}' at '{nestedPath}'");
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
            if (fireNestedInput && mainWidget?.Artboard != null && !string.IsNullOrEmpty(nestedPath))
            {
                string inputName = side == FaucetSide.Left ? FaucetLOn : FaucetROn;
                mainWidget.Artboard.FireInputStateAtPath(inputName, nestedPath);
            }

            NotifyActivated(side);
        }

        void TryResolveNested()
        {
            if (_nestedResolved || mainWidget?.Artboard == null)
                return;

            var artboard = mainWidget.Artboard;
            if (!string.IsNullOrEmpty(nestedPath) &&
                RiveStateMachineInputs.TryFindNestedInputPath(artboard, FaucetLOn, new[] { nestedPath }, out _))
            {
                _nestedResolved = true;
                Debug.Log($"[Faucet] Tracking nested triggers at '{nestedPath}'");
                return;
            }

            if (RiveStateMachineInputs.TryFindNestedInputPath(artboard, FaucetLOn, NestedInstancePaths, out var found))
            {
                nestedPath = found;
                _nestedResolved = true;
                Debug.Log($"[Faucet] Tracking nested triggers at '{nestedPath}'");
                return;
            }

            if (mainWidget.Status == WidgetStatus.Loaded && !_loggedMissingPath)
            {
                _loggedMissingPath = true;
                _nestedResolved = true;
                Debug.LogWarning("[Faucet] Nested faucet_L_On trigger not found on main. Open Faucet cannot see handle listeners.");
            }
        }

        void NotifyActivated(FaucetSide side)
        {
            Debug.Log($"[Faucet] Activated {side} listeners={Activated?.GetInvocationList().Length ?? 0}");
            Activated?.Invoke(side);
        }
    }
}
