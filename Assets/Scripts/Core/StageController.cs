using System;
using UnityEngine;

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

    public class StageController : MonoBehaviour
    {
        public FineTuningVariables tuning;
        public int StageIndex { get; private set; }
        public float Progress { get; private set; }
        public bool IsComplete => Progress >= 1f;
        public bool AcceptingInput { get; set; } = true;

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

        public void Begin(int stageIndex = 0)
        {
            StageIndex = Mathf.Clamp(stageIndex, 0, 5);
            Progress = 0f;
            _stageStartTime = Time.time;
            AcceptingInput = true;
            StageStarted?.Invoke(StageIndex);
            ProgressChanged?.Invoke();
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

        public void AddProgressFromFamily(InputFamily family, float deltaTime)
        {
            if (FamilyFor(StageIndex) != family || tuning == null) return;
            float rate = family switch
            {
                InputFamily.TapOpenClose => tuning.tapOpenCloseRate,
                InputFamily.HandsUnderWater => tuning.handsUnderWaterRate,
                _ => tuning.rubOnHandsRate
            };
            AddProgress(rate * deltaTime);
        }

        void CompleteCurrentStage()
        {
            AcceptingInput = false;
            float duration = Time.time - _stageStartTime;
            StageCompleted?.Invoke(StageIndex, duration);
            int completed = StageIndex;
            if (completed >= 5)
            {
                AllStagesCompleted?.Invoke();
                return;
            }

            StageIndex = completed + 1;
            Progress = 0f;
            _stageStartTime = Time.time;
            AcceptingInput = true;
            StageStarted?.Invoke(StageIndex);
            ProgressChanged?.Invoke();
        }
    }
}
