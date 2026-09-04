using Rive;
using Rive.Components;
using ManosLimpias.Core;
using ManosLimpias.UI.Rive;
using UnityEngine;

namespace ManosLimpias.UI
{
    /// <summary>
    /// Drives overlay progressBar and the four stepIcon widgets at stepN-anchor.
    /// </summary>
    public class RiveHudBinder : MonoBehaviour, IProgressBarControl, IStepIconControl
    {
        public HudPresenter hud;
        public bool logBindings;
        public RiveWidget progressBarWidget;
        public RiveWidget[] stepIconWidgets;

        public float Progress { get; private set; }
        public int StepId => _stepId;
        public bool StepActive => _stepActive;
        public bool StepCompleted => _stepCompleted;
        public bool HasStepState => _hasStepState;

        float _lastProgress = -1f;
        int _stepId = 1;
        bool _stepActive;
        bool _stepCompleted;
        bool _hasStepState;
        int _pushedStepId = int.MinValue;
        bool _pushedActive;
        bool _pushedCompleted;
        bool _progressInputMissing;
        bool _pushing;
        RiveWidget[] _subscribedIcons;

        public void Bind(RiveWidget progressBar, params RiveWidget[] stepIcons)
        {
            progressBarWidget = progressBar;
            stepIconWidgets = stepIcons;
            _lastProgress = -1f;
            _pushedStepId = int.MinValue;
            SubscribeStepIcons();
            PushStepIcon();
        }

        public void SetProgress(float progress)
        {
            Progress = Mathf.Clamp01(progress);
            Debug.Log($"[RiveHudBinder] Stage progress {Progress:F2} barSM={(progressBarWidget?.StateMachine != null ? "ready" : "null")}");
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
            SubscribeStepIcons();
        }

        void OnDisable()
        {
            UnsubscribeStepIcons();
        }

        void LateUpdate()
        {
            if (logBindings)
                Debug.Log($"[RiveHudBinder] stageProgress={Progress:F2} stepId={_stepId} active={_stepActive} completed={_stepCompleted}");
            PushProgress();
            PushStepIcon();
        }

        void SubscribeStepIcons()
        {
            if (ReferenceEquals(_subscribedIcons, stepIconWidgets))
                return;

            UnsubscribeStepIcons();
            _subscribedIcons = stepIconWidgets;
            if (_subscribedIcons == null)
                return;

            for (int i = 0; i < _subscribedIcons.Length; i++)
            {
                if (_subscribedIcons[i] != null)
                    _subscribedIcons[i].OnWidgetStatusChanged += OnStepIconStatusChanged;
            }
        }

        void UnsubscribeStepIcons()
        {
            if (_subscribedIcons == null)
                return;

            for (int i = 0; i < _subscribedIcons.Length; i++)
            {
                if (_subscribedIcons[i] != null)
                    _subscribedIcons[i].OnWidgetStatusChanged -= OnStepIconStatusChanged;
            }

            _subscribedIcons = null;
        }

        void OnStepIconStatusChanged()
        {
            _pushedStepId = int.MinValue;
            PushStepIcon();
        }

        void PushProgress()
        {
            if (progressBarWidget?.StateMachine == null || Mathf.Approximately(_lastProgress, Progress))
                return;

            if (TrySetProgressInput(progressBarWidget, Progress))
            {
                _lastProgress = Progress;
                return;
            }

            LogMissing(ref _progressInputMissing, ProgressBar.ProgressNum, ProgressBar.Artboard);
        }

        public static bool TrySetProgressInput(RiveWidget widget, float progress)
        {
            if (widget?.StateMachine == null)
                return false;

            var sm = widget.StateMachine;
            bool wrote = false;

            var instance = sm.ViewModelInstance;
            var property = instance != null ? instance.GetNumberProperty(ProgressBar.ProgressNum) : null;
            if (property != null)
            {
                property.Value = ProgressBar.ToBlend(progress);
                wrote = true;
            }

            var progressNum = RiveStateMachineInputs.GetNumber(sm, ProgressBar.ProgressNum);
            if (progressNum != null)
            {
                progressNum.Value = ProgressBar.ToBlend(progress);
                wrote = true;
            }
            else if (!wrote)
            {
                var fallback = RiveStateMachineInputs.GetNumber(sm, ProgressBar.ProgressNumFallback);
                if (fallback != null)
                {
                    fallback.Value = progress;
                    wrote = true;
                }
            }

            if (wrote)
                sm.Advance(0f);

            return wrote;
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
                PushStepWidgets();

                if (changed)
                {
                    Debug.Log($"[RiveHudBinder] StepIcon state stepId={_stepId} active={_stepActive} completed={_stepCompleted}");
                    _pushedStepId = _stepId;
                    _pushedActive = _stepActive;
                    _pushedCompleted = _stepCompleted;
                    var target = WidgetForStep(_stepId);
                    target?.StateMachine?.Advance(0f);
                    target?.RivePanel?.Tick(0f);
                }
            }
            finally
            {
                _pushing = false;
            }
        }

        void PushStepWidgets()
        {
            if (stepIconWidgets == null)
                return;

            for (int i = 0; i < stepIconWidgets.Length; i++)
            {
                var widget = stepIconWidgets[i];
                if (widget?.StateMachine == null)
                    continue;

                int id = i + 1;
                bool isTarget = id == _stepId;
                SetNumberInput(widget, StepIcon.StepId, id, StepIcon.StepIdLegacy);
                SetBoolInput(widget, StepIcon.IsActive, isTarget && _stepActive);
                SetBoolInput(widget, StepIcon.IsCompleted, isTarget ? _stepCompleted : id < _stepId);
            }
        }

        RiveWidget WidgetForStep(int stepId)
        {
            int index = stepId - 1;
            if (stepIconWidgets == null || index < 0 || index >= stepIconWidgets.Length)
                return null;
            return stepIconWidgets[index];
        }

        static void SetNumberInput(RiveWidget widget, string name, float value, string fallback)
        {
            var input = RiveStateMachineInputs.FindNumber(widget.StateMachine, name, fallback);
            if (input != null)
                input.Value = value;
        }

        static void SetBoolInput(RiveWidget widget, string name, bool value)
        {
            var input = RiveStateMachineInputs.GetBool(widget.StateMachine, name);
            if (input != null)
                input.Value = value;
        }

        static void LogMissing(ref bool logged, string inputName, string owner)
        {
            if (logged) return;
            logged = true;
            Debug.LogWarning($"[RiveHudBinder] Missing Rive input '{inputName}' on '{owner}'.");
        }
    }
}
