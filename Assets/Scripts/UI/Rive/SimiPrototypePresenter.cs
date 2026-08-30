using ManosLimpias.Core;
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
            if (introWidget == null) return;
            introWidget.OnWidgetStatusChanged += OnIntroStatusChanged;
            introWidget.OnRiveEventReported += OnIntroEventReported;
            if (introWidget.Status == WidgetStatus.Loaded)
                BindIntro();
        }

        void OnDisable()
        {
            UnbindIntro();
            if (introWidget == null) return;
            introWidget.OnWidgetStatusChanged -= OnIntroStatusChanged;
            introWidget.OnRiveEventReported -= OnIntroEventReported;
        }

        void OnIntroStatusChanged()
        {
            if (introWidget != null && introWidget.Status == WidgetStatus.Loaded)
                BindIntro();
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
