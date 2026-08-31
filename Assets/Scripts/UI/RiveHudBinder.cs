using Rive;
using Rive.Components;
using ManosLimpias.Core;
using ManosLimpias.UI.Rive;
using UnityEngine;

namespace ManosLimpias.UI
{
    /// <summary>
    /// Drives the prototype's direct state-machine inputs. The prototype has no VM_HUD;
    /// progress lives on main and the configured step icon is written both to the overlay
    /// widget and to nested <c>step</c> instances inside <c>main</c>.
    /// </summary>
    public class RiveHudBinder : MonoBehaviour, IProgressBarControl, IStepIconControl
    {
        public HudPresenter hud;
        public bool logBindings;
        public RiveWidget mainWidget;
        public RiveWidget stepIconWidget;

        public float Progress { get; private set; }
        public int StepId => _stepId;
        public bool StepActive => _stepActive;
        public bool StepCompleted => _stepCompleted;
        public bool HasStepState => _hasStepState;
        public string NestedStepPath => _nestedPath;

        float _lastProgress = -1f;
        int _stepId = 1;
        bool _stepActive;
        bool _stepCompleted;
        bool _hasStepState;
        int _pushedStepId = int.MinValue;
        bool _pushedActive;
        bool _pushedCompleted;
        bool _progressInputMissing;
        bool _stepIdMissing;
        bool _stepActiveMissing;
        bool _stepCompletedMissing;
        bool _nestedPathResolved;
        bool _nestedPathMissing;
        bool _pushing;
        string _nestedPath;
        RiveWidget _subscribedStepIcon;

        public void Bind(RiveWidget main, RiveWidget stepIcon)
        {
            mainWidget = main;
            stepIconWidget = stepIcon;
            _lastProgress = -1f;
            _pushedStepId = int.MinValue;
            _nestedPathResolved = false;
            _nestedPath = null;
            SubscribeStepIcon();
            PushStepIcon();
        }

        public void SetProgress(float progress)
        {
            Progress = Mathf.Clamp01(progress);
            Debug.Log($"[RiveHudBinder] Stage progress {Progress:F2} mainSM={(mainWidget?.StateMachine != null ? "ready" : "null")}");
            if (hud != null)
                hud.ApplyStage(hud.StageIndex, Progress);
            PushProgress();
        }

        public void SetState(int stepId, bool active, bool completed)
        {
            _stepId = Mathf.Max(1, stepId);
            _stepActive = active;
            _stepCompleted = completed;
            _hasStepState = true;
            PushStepIcon();
        }

        void OnEnable()
        {
            SubscribeStepIcon();
        }

        void OnDisable()
        {
            UnsubscribeStepIcon();
        }

        void LateUpdate()
        {
            if (logBindings)
                Debug.Log($"[RiveHudBinder] stageProgress={Progress:F2} stepId={_stepId} active={_stepActive} completed={_stepCompleted} nested={_nestedPath}");
            PushProgress();
            PushStepIcon();
        }

        void SubscribeStepIcon()
        {
            if (_subscribedStepIcon == stepIconWidget)
                return;

            UnsubscribeStepIcon();
            _subscribedStepIcon = stepIconWidget;
            if (_subscribedStepIcon != null)
                _subscribedStepIcon.OnWidgetStatusChanged += OnStepIconStatusChanged;
        }

        void UnsubscribeStepIcon()
        {
            if (_subscribedStepIcon == null)
                return;
            _subscribedStepIcon.OnWidgetStatusChanged -= OnStepIconStatusChanged;
            _subscribedStepIcon = null;
        }

        void OnStepIconStatusChanged()
        {
            if (stepIconWidget != null && stepIconWidget.Status == WidgetStatus.Loaded)
            {
                _pushedStepId = int.MinValue;
                PushStepIcon();
            }
        }

        void PushProgress()
        {
            if (mainWidget?.StateMachine == null || Mathf.Approximately(_lastProgress, Progress))
                return;

            var input = RiveStateMachineInputs.GetNumber(mainWidget.StateMachine, MainProgress.ProgressNum);
            if (input == null)
            {
                LogMissing(ref _progressInputMissing, MainProgress.ProgressNum, MainProgress.StateMachine);
                return;
            }

            input.Value = Progress;
            _lastProgress = Progress;
        }

        void PushStepIcon()
        {
            if (!_hasStepState || _pushing)
                return;

            var changed = _pushedStepId != _stepId ||
                          _pushedActive != _stepActive ||
                          _pushedCompleted != _stepCompleted;

            _pushing = true;
            try
            {
                var overlayReady = stepIconWidget?.StateMachine != null;
                PushOverlayInputs();
                PushNestedMainInputs();

                if (changed)
                {
                    Debug.Log($"[RiveHudBinder] StepIcon state stepId={_stepId} active={_stepActive} completed={_stepCompleted} overlaySM={(overlayReady ? "ready" : "null")} nested={_nestedPath ?? "none"}");
                    _pushedStepId = _stepId;
                    _pushedActive = _stepActive;
                    _pushedCompleted = _stepCompleted;
                    stepIconWidget?.StateMachine?.Advance(0f);
                    mainWidget?.StateMachine?.Advance(0f);
                    var panel = stepIconWidget != null ? stepIconWidget.RivePanel : mainWidget?.RivePanel;
                    panel?.Tick(0f);
                }
            }
            finally
            {
                _pushing = false;
            }
        }

        void PushOverlayInputs()
        {
            if (stepIconWidget?.StateMachine == null)
                return;

            var stepId = RiveStateMachineInputs.GetNumber(stepIconWidget.StateMachine, StepIcon.StepId);
            var active = RiveStateMachineInputs.GetBool(stepIconWidget.StateMachine, StepIcon.IsActive);
            var completed = RiveStateMachineInputs.GetBool(stepIconWidget.StateMachine, StepIcon.IsCompleted);
            if (stepId == null)
                LogMissing(ref _stepIdMissing, StepIcon.StepId, StepIcon.StateMachine);
            else
                stepId.Value = _stepId;
            if (active == null)
                LogMissing(ref _stepActiveMissing, StepIcon.IsActive, StepIcon.StateMachine);
            else
                active.Value = _stepActive;
            if (completed == null)
                LogMissing(ref _stepCompletedMissing, StepIcon.IsCompleted, StepIcon.StateMachine);
            else
                completed.Value = _stepCompleted;
        }

        void PushNestedMainInputs()
        {
            var artboard = mainWidget?.Artboard;
            if (artboard == null)
                return;

            if (!_nestedPathResolved)
            {
                _nestedPath = ResolveNestedPath(artboard);
                if (!string.IsNullOrEmpty(_nestedPath) || mainWidget.Status == WidgetStatus.Loaded)
                    _nestedPathResolved = true;
                if (!string.IsNullOrEmpty(_nestedPath))
                    Debug.Log($"[RiveHudBinder] Driving nested step instance '{_nestedPath}' on main.");
                else if (_nestedPathResolved && !_nestedPathMissing)
                {
                    _nestedPathMissing = true;
                    Debug.LogWarning("[RiveHudBinder] No nested stepIcon instance found on main. Overlay widget is the only rail.");
                }
            }

            if (string.IsNullOrEmpty(_nestedPath))
                return;

            RiveStateMachineInputs.TrySetNumber(artboard, StepIcon.StepId, _stepId, _nestedPath);
            RiveStateMachineInputs.TrySetBool(artboard, StepIcon.IsActive, _stepActive, _nestedPath);
            RiveStateMachineInputs.TrySetBool(artboard, StepIcon.IsCompleted, _stepCompleted, _nestedPath);
        }

        static string ResolveNestedPath(Artboard artboard)
        {
            foreach (var path in StepIcon.NestedInstancePaths)
            {
                if (NodeExists(artboard, path) &&
                    artboard.GetBooleanInputStateAtPath(StepIcon.IsActive, path).HasValue)
                    return path;
            }

            return null;
        }

        static bool NodeExists(Artboard artboard, string name)
        {
#pragma warning disable CS0618
            var component = artboard.Component(name);
#pragma warning restore CS0618
            if (component == null)
                return false;
            System.GC.KeepAlive(component);
            return true;
        }

        static void LogMissing(ref bool logged, string inputName, string stateMachine)
        {
            if (logged) return;
            logged = true;
            Debug.LogWarning($"[RiveHudBinder] Missing Rive input '{inputName}' on '{stateMachine}'.");
        }
    }
}
