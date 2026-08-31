using System;
using ManosLimpias.Core;
using Rive;
using Rive.Components;
using UnityEngine;

namespace ManosLimpias.UI.Rive
{
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

        [Tooltip("Nested instance path inside the main artboard. Override this if the Rive hierarchy uses another instance name.")]
        public string nestedPath;
        [Tooltip("Use only when a valid nested instance path is known. Rive pointer input normally drives the Faucet directly.")]
        public bool fireNestedInput;
        public RiveWidget mainWidget;

        public event Action<FaucetSide> Activated;
        public bool IsEnabled { get; private set; } = true;

        public void SetEnabled(bool enabled)
        {
            IsEnabled = enabled;
        }

        void OnEnable()
        {
            if (mainWidget != null)
                mainWidget.OnRiveEventReported += OnRiveEventReported;
        }

        void OnDisable()
        {
            if (mainWidget != null)
                mainWidget.OnRiveEventReported -= OnRiveEventReported;
        }

        public void Bind(RiveWidget widget)
        {
            if (mainWidget == widget) return;
            if (mainWidget != null)
                mainWidget.OnRiveEventReported -= OnRiveEventReported;
            mainWidget = widget;
            if (isActiveAndEnabled && mainWidget != null)
                mainWidget.OnRiveEventReported += OnRiveEventReported;
        }

        void OnRiveEventReported(ReportedEvent report)
        {
            if (!IsEnabled || report == null) return;
            if (report.Name == FaucetLOn || report.Name == "faucet_L_On")
                NotifyActivated(FaucetSide.Left);
            else if (report.Name == FaucetROn || report.Name == "faucet_R_On")
                NotifyActivated(FaucetSide.Right);
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

        void NotifyActivated(FaucetSide side)
        {
            Activated?.Invoke(side);
        }
    }
}
