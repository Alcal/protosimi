using Rive;
using Rive.Components;
using ManosLimpias.Core;
using ManosLimpias.UI.Rive;
using UnityEngine;

namespace ManosLimpias.UI
{
    /// <summary>
    /// Drives the prototype's direct state-machine inputs. The prototype has no VM_HUD;
    /// progress lives on main and the single configured step icon is a direct widget.
    /// </summary>
    public class RiveHudBinder : MonoBehaviour, IProgressBarControl, IStepIconControl
    {
        public HudPresenter hud;
        public bool logBindings;
        public RiveWidget mainWidget;
        public RiveWidget stepIconWidget;

        public float Progress { get; private set; }

        float _lastProgress = -1f;
        int _stepId = 1;
        bool _stepActive;
        bool _stepCompleted;
        bool _hasStepState;
        int _lastStepId = -1;
        bool _lastStepActive;
        bool _lastStepCompleted;
        bool _progressInputMissing;
        bool _stepIdMissing;
        bool _stepActiveMissing;
        bool _stepCompletedMissing;

        public void Bind(RiveWidget main, RiveWidget stepIcon)
        {
            mainWidget = main;
            stepIconWidget = stepIcon;
            _lastProgress = -1f;
            _lastStepId = -1;
            _hasStepState = false;
        }

        public void SetProgress(float progress)
        {
            Progress = Mathf.Clamp01(progress);
            if (hud != null)
                hud.ApplyStage(hud.StageIndex, Progress);
        }

        public void SetState(int stepId, bool active, bool completed)
        {
            _stepId = Mathf.Max(1, stepId);
            _stepActive = active;
            _stepCompleted = completed;
            _hasStepState = true;
        }

        void LateUpdate()
        {
            if (logBindings)
                Debug.Log($"[RiveHudBinder] stageProgress={Progress:F2} stepId={_stepId}");
            PushProgress();
            PushStepIcon();
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
            if (!_hasStepState || stepIconWidget?.StateMachine == null)
                return;

            if (_lastStepId == _stepId &&
                _lastStepActive == _stepActive &&
                _lastStepCompleted == _stepCompleted)
                return;

            var stepId = RiveStateMachineInputs.GetNumber(stepIconWidget.StateMachine, StepIcon.StepId);
            var active = RiveStateMachineInputs.GetBool(stepIconWidget.StateMachine, StepIcon.IsActive);
            var completed = RiveStateMachineInputs.GetBool(stepIconWidget.StateMachine, StepIcon.IsCompleted);
            if (stepId == null)
                LogMissing(ref _stepIdMissing, StepIcon.StepId, StepIcon.StateMachine);
            if (active == null)
                LogMissing(ref _stepActiveMissing, StepIcon.IsActive, StepIcon.StateMachine);
            if (completed == null)
                LogMissing(ref _stepCompletedMissing, StepIcon.IsCompleted, StepIcon.StateMachine);
            if (stepId == null || active == null || completed == null)
                return;

            stepId.Value = _stepId;
            active.Value = _stepActive;
            completed.Value = _stepCompleted;
            _lastStepId = _stepId;
            _lastStepActive = _stepActive;
            _lastStepCompleted = _stepCompleted;
        }

        static void LogMissing(ref bool logged, string inputName, string stateMachine)
        {
            if (logged) return;
            logged = true;
            Debug.LogWarning($"[RiveHudBinder] Missing Rive input '{inputName}' on '{stateMachine}'.");
        }
    }
}
