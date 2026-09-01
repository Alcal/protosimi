using ManosLimpias.Core;
using ManosLimpias.UI;
using Rive;
using Rive.Components;
using UnityEngine;

namespace ManosLimpias.UI.Rive
{
    /// <summary>
    /// Mounts simi_prototype <c>main</c> + <c>intro</c> and dismisses intro when Jugar fires.
    /// </summary>
    public class SimiPrototypePresenter : MonoBehaviour
    {
        public GameFlowController flow;
        public RiveWidget mainWidget;
        public RiveWidget introWidget;
        public RiveWidget stepIconWidget;
        public RiveHudBinder hudBinder;
        public Faucet faucet;

        static readonly string[] TriggerPaths =
        {
            BubbleButtonViewModel.ButtonTrig,
            BubbleButtonViewModel.Name + "/" + BubbleButtonViewModel.ButtonTrig,
            SimiPrototypeArtboards.Button + "/" + BubbleButtonViewModel.ButtonTrig,
        };

        static readonly string[] BoolPaths =
        {
            BubbleButtonViewModel.ButtonBool,
            BubbleButtonViewModel.Name + "/" + BubbleButtonViewModel.ButtonBool,
            SimiPrototypeArtboards.Button + "/" + BubbleButtonViewModel.ButtonBool,
        };

        bool _dismissed;
        bool _introBound;
        ViewModelInstanceTriggerProperty _buttonTrig;
        ViewModelInstanceBooleanProperty _buttonBool;

        void OnEnable()
        {
            if (introWidget != null)
            {
                introWidget.OnWidgetStatusChanged += OnIntroStatusChanged;
                introWidget.OnRiveEventReported += OnIntroEventReported;
                if (introWidget.Status == WidgetStatus.Loaded)
                    BindIntro();
            }

            if (mainWidget != null)
            {
                mainWidget.OnWidgetStatusChanged += OnMainStatusChanged;
                if (mainWidget.Status == WidgetStatus.Loaded)
                    BindGameplay();
            }

            if (stepIconWidget != null)
            {
                stepIconWidget.OnWidgetStatusChanged += OnStepIconStatusChanged;
                if (stepIconWidget.Status == WidgetStatus.Loaded)
                    BindGameplay();
            }
        }

        void OnDisable()
        {
            UnbindIntro();
            if (introWidget != null)
            {
                introWidget.OnWidgetStatusChanged -= OnIntroStatusChanged;
                introWidget.OnRiveEventReported -= OnIntroEventReported;
            }

            if (mainWidget != null)
                mainWidget.OnWidgetStatusChanged -= OnMainStatusChanged;
            if (stepIconWidget != null)
                stepIconWidget.OnWidgetStatusChanged -= OnStepIconStatusChanged;
        }

        void OnIntroStatusChanged()
        {
            if (introWidget != null && introWidget.Status == WidgetStatus.Loaded)
                BindIntro();
        }

        void OnMainStatusChanged()
        {
            if (mainWidget != null && mainWidget.Status == WidgetStatus.Loaded)
                BindGameplay();
        }

        void OnStepIconStatusChanged()
        {
            if (stepIconWidget != null && stepIconWidget.Status == WidgetStatus.Loaded)
                BindGameplay();
        }

        void BindGameplay()
        {
            faucet?.Bind(mainWidget);
            hudBinder?.Bind(mainWidget, stepIconWidget);
        }

        void BindIntro()
        {
            if (_introBound || introWidget == null) return;
            var instance = introWidget.StateMachine?.ViewModelInstance;
            if (instance == null) return;

            _introBound = true;
            foreach (var path in TriggerPaths)
            {
                _buttonTrig = RiveStateMachineInputs.GetViewModelProperty<ViewModelInstanceTriggerProperty>(instance, path);
                if (_buttonTrig != null)
                {
                    _buttonTrig.OnTriggered += DismissIntro;
                    break;
                }
            }

            foreach (var path in BoolPaths)
            {
                _buttonBool = RiveStateMachineInputs.GetViewModelProperty<ViewModelInstanceBooleanProperty>(instance, path);
                if (_buttonBool != null)
                {
                    _buttonBool.OnValueChanged += OnButtonBoolChanged;
                    break;
                }
            }
        }

        void UnbindIntro()
        {
            if (_buttonTrig != null)
            {
                _buttonTrig.OnTriggered -= DismissIntro;
                _buttonTrig = null;
            }

            if (_buttonBool != null)
            {
                _buttonBool.OnValueChanged -= OnButtonBoolChanged;
                _buttonBool = null;
            }

            _introBound = false;
        }

        void OnIntroEventReported(ReportedEvent report)
        {
            if (report != null && report.Name == ButtonArtboard.EventButtonPress)
                DismissIntro();
        }

        void OnButtonBoolChanged(bool value)
        {
            if (value)
                DismissIntro();
        }

        public void DismissIntro()
        {
            if (_dismissed) return;
            _dismissed = true;
            UnbindIntro();
            if (introWidget != null)
                introWidget.gameObject.SetActive(false);
            flow?.DismissIntro();
        }
    }
}
