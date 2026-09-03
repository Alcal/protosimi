using ManosLimpias.Core;
using ManosLimpias.UI;
using Rive;
using Rive.Components;
using UnityEngine;

namespace ManosLimpias.UI.Rive
{
    /// <summary>
    /// Mounts simi_prototype background + intro and dismisses intro when Jugar fires.
    /// </summary>
    public class SimiPrototypePresenter : MonoBehaviour
    {
        public GameFlowController flow;
        public RiveWidget backgroundWidget;
        public RiveWidget introWidget;
        public RiveWidget progressBarWidget;
        public RiveWidget faucetWidget;
        public RiveWidget[] stepIconWidgets;
        public RiveHudBinder hudBinder;
        public Faucet faucet;
        public RiveAnchorMount anchorMount;

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

            if (backgroundWidget != null)
            {
                backgroundWidget.OnWidgetStatusChanged += OnBackgroundStatusChanged;
                if (backgroundWidget.Status == WidgetStatus.Loaded)
                    BindGameplay();
            }

            SubscribeStepIcons();
        }

        void OnDisable()
        {
            UnbindIntro();
            if (introWidget != null)
            {
                introWidget.OnWidgetStatusChanged -= OnIntroStatusChanged;
                introWidget.OnRiveEventReported -= OnIntroEventReported;
            }

            if (backgroundWidget != null)
                backgroundWidget.OnWidgetStatusChanged -= OnBackgroundStatusChanged;
            UnsubscribeStepIcons();
        }

        void OnIntroStatusChanged()
        {
            if (introWidget != null && introWidget.Status == WidgetStatus.Loaded)
                BindIntro();
        }

        void OnBackgroundStatusChanged()
        {
            if (backgroundWidget != null && backgroundWidget.Status == WidgetStatus.Loaded)
                BindGameplay();
        }

        void OnStepIconStatusChanged()
        {
            BindGameplay();
        }

        void BindGameplay()
        {
            faucet?.Bind(faucetWidget);
            hudBinder?.Bind(progressBarWidget, stepIconWidgets);
            anchorMount?.TryApply();
        }

        void SubscribeStepIcons()
        {
            if (stepIconWidgets == null)
                return;
            for (int i = 0; i < stepIconWidgets.Length; i++)
            {
                if (stepIconWidgets[i] != null)
                {
                    stepIconWidgets[i].OnWidgetStatusChanged += OnStepIconStatusChanged;
                    if (stepIconWidgets[i].Status == WidgetStatus.Loaded)
                        BindGameplay();
                }
            }

            if (progressBarWidget != null)
            {
                progressBarWidget.OnWidgetStatusChanged += OnStepIconStatusChanged;
                if (progressBarWidget.Status == WidgetStatus.Loaded)
                    BindGameplay();
            }
        }

        void UnsubscribeStepIcons()
        {
            if (stepIconWidgets != null)
            {
                for (int i = 0; i < stepIconWidgets.Length; i++)
                {
                    if (stepIconWidgets[i] != null)
                        stepIconWidgets[i].OnWidgetStatusChanged -= OnStepIconStatusChanged;
                }
            }

            if (progressBarWidget != null)
                progressBarWidget.OnWidgetStatusChanged -= OnStepIconStatusChanged;
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
