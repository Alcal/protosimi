using UnityEngine;

namespace ManosLimpias.Core
{
    [System.Serializable]
    public sealed class ApplySoapStage : GameStage
    {
        public const float FillStep = 0.02f;
        public const float FillInterval = 0.1f;

        public int stepId = 2;

        bool _completed;
        bool _soapGrabbed;
        float _fillTimer;

        public override string Id => "ApplySoap";

        protected override void OnEnter()
        {
            _completed = false;
            _soapGrabbed = false;
            _fillTimer = 0f;
            Services.ProgressBar?.SetProgress(0f);
            Services.StepIcon?.SetState(stepId, active: true, completed: false);
            Services.Hands?.SetDraggable(false);
            Services.Hands?.SetGlow(false);

            if (Services.Soap == null)
            {
                Debug.LogWarning("[ApplySoapStage] No Soap service; cannot enable drag.");
                return;
            }

            Debug.Log("[ApplySoapStage] Entered; soap glowing and draggable.");
            SubscribeSoap();
            Services.Soap.SetDraggable(true);
            Services.Soap.SetGlow(true);
        }

        protected override void OnTick(float deltaTime)
        {
            if (_completed || !IsEntered)
                return;

            if (!_soapGrabbed || Services.Soap == null || !Services.Soap.IsOverlapping)
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
            }

            if (progress >= 1f)
                CompleteOnce();
        }

        protected override void OnExit()
        {
            UnsubscribeSoap();
            Services.Soap?.SetGlow(false);
            Services.Soap?.SetDraggable(false);
            Services.Soap?.ReturnHome();
        }

        void OnSoapDragStarted()
        {
            _soapGrabbed = true;
            Services.Soap?.SetGlow(false);
        }

        void CompleteOnce()
        {
            if (_completed || !IsEntered)
                return;
            _completed = true;
            Services.ProgressBar?.SetProgress(1f);
            Services.StepIcon?.SetState(stepId, active: false, completed: true);
            Services.Soap?.SetGlow(false);
            Services.Soap?.SetDraggable(false);
            Services.Soap?.ReturnHome();
            Services.RequestStageCompletion(this);
        }

        void SubscribeSoap()
        {
            if (Services.Soap == null)
                return;
            Services.Soap.DragStarted -= OnSoapDragStarted;
            Services.Soap.DragStarted += OnSoapDragStarted;
        }

        void UnsubscribeSoap()
        {
            if (Services.Soap == null)
                return;
            Services.Soap.DragStarted -= OnSoapDragStarted;
        }

        public override GameStage CreateRuntime()
        {
            return new ApplySoapStage { stepId = stepId };
        }
    }
}
