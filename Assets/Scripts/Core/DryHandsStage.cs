using UnityEngine;

namespace ManosLimpias.Core
{
    [System.Serializable]
    public sealed class DryHandsStage : GameStage
    {
        public const float FillStep = 0.02f;
        public const float FillInterval = 0.1f;

        public int stepId = 4;

        bool _completed;
        bool _towelGrabbed;
        float _fillTimer;

        public override string Id => "DryHands";

        public static float WetnessForProgress(float progress)
        {
            return Mathf.Clamp01(1f - progress);
        }

        protected override void OnEnter()
        {
            _completed = false;
            _towelGrabbed = false;
            _fillTimer = 0f;
            Services.ProgressBar?.SetProgress(0f);
            Services.Wetness?.SetWetness(WetnessForProgress(0f));
            Services.StepIcon?.SetState(stepId, active: true, completed: false);
            Services.Hands?.SetDraggable(false);
            Services.Hands?.SetGlow(false);
            Services.Soap?.SetDraggable(false);
            Services.Soap?.SetGlow(false);

            if (Services.Towel == null)
            {
                Debug.LogWarning("[DryHandsStage] No Towel service; cannot enable drag.");
                return;
            }

            Debug.Log("[DryHandsStage] Entered; towel glowing and draggable.");
            SubscribeTowel();
            Services.Towel.SetDraggable(true);
            Services.Towel.SetGlow(true);
        }

        protected override void OnTick(float deltaTime)
        {
            if (_completed || !IsEntered)
                return;

            bool overlapping = _towelGrabbed && Services.Towel != null && Services.Towel.IsOverlapping;
            if (!overlapping)
            {
                _fillTimer = 0f;
                return;
            }

            _fillTimer += deltaTime;
            var progress = Services.ProgressBar != null ? Services.ProgressBar.Progress : 0f;
            while (_fillTimer >= FillInterval && progress < 1f)
            {
                _fillTimer -= FillInterval;
                progress = Mathf.Min(1f, progress + FillStep);
                Services.ProgressBar?.SetProgress(progress);
                Services.Wetness?.SetWetness(WetnessForProgress(progress));
            }

            if (progress >= 1f)
                CompleteOnce();
        }

        protected override void OnExit()
        {
            UnsubscribeTowel();
            Services.Towel?.SetGlow(false);
            Services.Towel?.SetDraggable(false);
            Services.Towel?.ReturnHome();
        }

        void OnTowelDragStarted()
        {
            _towelGrabbed = true;
            Services.Towel?.SetGlow(false);
        }

        void CompleteOnce()
        {
            if (_completed || !IsEntered)
                return;
            _completed = true;
            Services.ProgressBar?.SetProgress(1f);
            Services.Wetness?.SetWetness(0f);
            Services.StepIcon?.SetState(stepId, active: false, completed: true);
            Services.Towel?.SetGlow(false);
            Services.Towel?.SetDraggable(false);
            Services.Towel?.ReturnHome();
            Services.RequestStageCompletion(this);
        }

        void SubscribeTowel()
        {
            if (Services.Towel == null)
                return;
            Services.Towel.DragStarted -= OnTowelDragStarted;
            Services.Towel.DragStarted += OnTowelDragStarted;
        }

        void UnsubscribeTowel()
        {
            if (Services.Towel == null)
                return;
            Services.Towel.DragStarted -= OnTowelDragStarted;
        }

        public override GameStage CreateRuntime()
        {
            return new DryHandsStage { stepId = stepId };
        }
    }
}
