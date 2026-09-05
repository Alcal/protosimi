using UnityEngine;

namespace ManosLimpias.Core
{
    [System.Serializable]
    public sealed class OpenFaucetStage : GameStage
    {
        public const float OpenProgress = 0.25f;
        public const float FillStep = 0.02f;
        public const float FillInterval = 0.1f;

        public int stepId = 1;

        bool _completed;
        bool _faucetLocked;
        float _fillTimer;

        public override string Id => "OpenFaucet";

        protected override void OnEnter()
        {
            _completed = false;
            _faucetLocked = false;
            _fillTimer = 0f;
            Services.ProgressBar?.SetProgress(0f);
            Services.StepIcon?.SetState(stepId, active: true, completed: false);
            Services.Hands?.SetDraggable(false);
            Services.Hands?.SetGlow(false);

            if (Services.Faucet == null)
            {
                Debug.LogWarning("[OpenFaucetStage] No Faucet service; cannot subscribe to activation.");
                return;
            }

            Debug.Log("[OpenFaucetStage] Entered; subscribed to Faucet pointer hit.");
            SubscribeFaucet();
            Services.Faucet.SetEnabled(true);
            Services.Faucet.SetGlow(true);
            if (Services.Faucet.IsOpen)
                LockFaucetOpen(OpenSide());
        }

        protected override void OnTick(float deltaTime)
        {
            if (_completed || !IsEntered)
                return;

            if (!_faucetLocked)
            {
                if (Services.Faucet != null && Services.Faucet.IsOpen)
                    LockFaucetOpen(OpenSide());
                return;
            }

            if (Services.WaterContact != null && Services.WaterContact.IsOverlapping)
            {
                _fillTimer += deltaTime;
                var progress = Services.ProgressBar != null ? Services.ProgressBar.Progress : OpenProgress;
                while (_fillTimer >= FillInterval && progress < 1f)
                {
                    _fillTimer -= FillInterval;
                    progress = Mathf.Min(1f, progress + FillStep);
                    Services.ProgressBar?.SetProgress(progress);
                }

                if (progress >= 1f)
                    CompleteOnce();
            }
            else
            {
                _fillTimer = 0f;
            }
        }

        protected override void OnExit()
        {
            UnsubscribeHands();
            UnsubscribeFaucet();
            Services.Hands?.SetGlow(false);
            Services.Hands?.SetDraggable(false);
            if (Services.Faucet != null)
            {
                Services.Faucet.SetGlow(false);
                Services.Faucet.SetEnabled(false);
            }
        }

        void OnHandsDragStarted()
        {
            Services.Hands?.SetGlow(false);
        }

        void OnFaucetActivated(FaucetSide side)
        {
            Debug.Log($"[OpenFaucetStage] Activated {side} entered={IsEntered}");
            LockFaucetOpen(side);
        }

        void OnFaucetPointerHit(FaucetSide side)
        {
            Debug.Log($"[OpenFaucetStage] PointerHit {side} entered={IsEntered}");
            LockFaucetOpen(side);
        }

        void LockFaucetOpen(FaucetSide side)
        {
            if (_faucetLocked || _completed || !IsEntered)
                return;

            _faucetLocked = true;
            UnsubscribeFaucet();
            Services.Faucet?.LockOpen(side);
            Services.Faucet?.SetGlow(false);
            Services.ProgressBar?.SetProgress(OpenProgress);
            Services.Hands?.SetDraggable(true);
            Services.Hands?.SetGlow(true);
            SubscribeHands();
            Debug.Log($"[OpenFaucetStage] Faucet locked open ({side}) at 25%; hands draggable.");
        }

        FaucetSide OpenSide()
        {
            if (Services.Faucet != null && Services.Faucet.RightIsOpen && !Services.Faucet.LeftIsOpen)
                return FaucetSide.Right;
            return FaucetSide.Left;
        }

        void CompleteOnce()
        {
            if (_completed || !IsEntered)
                return;
            _completed = true;
            Services.ProgressBar?.SetProgress(1f);
            Services.StepIcon?.SetState(stepId, active: false, completed: true);
            Services.RequestStageCompletion(this);
        }

        void SubscribeFaucet()
        {
            if (Services.Faucet == null)
                return;
            Services.Faucet.Activated += OnFaucetActivated;
            Services.Faucet.PointerHit += OnFaucetPointerHit;
        }

        void UnsubscribeFaucet()
        {
            if (Services.Faucet == null)
                return;
            Services.Faucet.Activated -= OnFaucetActivated;
            Services.Faucet.PointerHit -= OnFaucetPointerHit;
        }

        void SubscribeHands()
        {
            if (Services.Hands == null)
                return;
            Services.Hands.DragStarted -= OnHandsDragStarted;
            Services.Hands.DragStarted += OnHandsDragStarted;
        }

        void UnsubscribeHands()
        {
            if (Services.Hands == null)
                return;
            Services.Hands.DragStarted -= OnHandsDragStarted;
        }

        public override GameStage CreateRuntime()
        {
            return new OpenFaucetStage { stepId = stepId };
        }
    }
}
