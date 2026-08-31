using System;
using UnityEngine;
using UnityEngine.Events;

namespace ManosLimpias.Core
{
    public enum WashStage
    {
        OpenWater = 0,
        WetHands = 1,
        RubSoap = 2,
        Rinse = 3,
        CloseWater = 4,
        DryHands = 5
    }

    public enum InputFamily
    {
        TapOpenClose,
        HandsUnderWater,
        RubOnHands
    }

    [Serializable]
    public class StageDefinition
    {
        public string id = "OpenWater";
        public InputFamily inputFamily = InputFamily.TapOpenClose;
        [Min(1)] public int stepIconId = 1;
        public UnityEvent OnEnter = new();
        public UnityEvent OnExit = new();
    }

    public class StageController : MonoBehaviour
    {
        public FineTuningVariables tuning;
        [Tooltip("Ordered stages available to this session. The default prototype contains Open Water only.")]
        public StageDefinition[] stageDefinitions =
        {
            new StageDefinition()
        };

        public int StageIndex { get; private set; }
        public float Progress { get; private set; }
        public bool IsComplete => Progress >= 1f;
        public bool AcceptingInput { get; set; } = true;
        public StageDefinition CurrentDefinition { get; private set; }
        public int ConfiguredStageCount => stageDefinitions?.Length ?? 0;

        public event Action<int> StageStarted;
        public event Action<int, float> StageCompleted;
        public event Action ProgressChanged;
        public event Action AllStagesCompleted;

        float _stageStartTime;

        public static InputFamily FamilyFor(int stageIndex)
        {
            return stageIndex switch
            {
                0 or 4 => InputFamily.TapOpenClose,
                1 or 3 => InputFamily.HandsUnderWater,
                _ => InputFamily.RubOnHands
            };
        }

        public StageDefinition DefinitionFor(int stageIndex)
        {
            if (stageDefinitions == null || stageIndex < 0 || stageIndex >= stageDefinitions.Length)
                return null;
            return stageDefinitions[stageIndex];
        }

        public void Begin(int stageIndex = 0)
        {
            if (stageDefinitions == null || stageDefinitions.Length == 0)
            {
                AcceptingInput = false;
                CurrentDefinition = null;
                return;
            }

            StageIndex = Mathf.Clamp(stageIndex, 0, stageDefinitions.Length - 1);
            CurrentDefinition = stageDefinitions[StageIndex];
            Progress = 0f;
            _stageStartTime = Time.time;
            AcceptingInput = true;
            StageStarted?.Invoke(StageIndex);
            ProgressChanged?.Invoke();
            CurrentDefinition?.OnEnter?.Invoke();
        }

        public void ResetSession()
        {
            Begin(0);
        }

        public void AddProgress(float amount)
        {
            if (!AcceptingInput || amount <= 0f || IsComplete) return;
            Progress = Mathf.Clamp01(Progress + amount);
            ProgressChanged?.Invoke();
            if (Progress >= 1f)
                CompleteCurrentStage();
        }

        public void CompleteCurrentStage()
        {
            if (!AcceptingInput || IsComplete || CurrentDefinition == null) return;
            Progress = 1f;
            ProgressChanged?.Invoke();
            CompleteCurrentStageInternal();
        }

        public void AddProgressFromFamily(InputFamily family, float deltaTime)
        {
            if (CurrentDefinition == null || CurrentDefinition.inputFamily != family || tuning == null) return;
            float rate = family switch
            {
                InputFamily.TapOpenClose => tuning.tapOpenCloseRate,
                InputFamily.HandsUnderWater => tuning.handsUnderWaterRate,
                _ => tuning.rubOnHandsRate
            };
            AddProgress(rate * deltaTime);
        }

        void CompleteCurrentStageInternal()
        {
            AcceptingInput = false;
            float duration = Time.time - _stageStartTime;
            StageCompleted?.Invoke(StageIndex, duration);
            CurrentDefinition?.OnExit?.Invoke();

            if (StageIndex >= ConfiguredStageCount - 1)
            {
                AllStagesCompleted?.Invoke();
                return;
            }

            Begin(StageIndex + 1);
        }
    }
}
