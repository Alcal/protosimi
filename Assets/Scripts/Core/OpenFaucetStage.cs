using UnityEngine;

namespace ManosLimpias.Core
{
    [System.Serializable]
    public sealed class OpenFaucetStage : GameStage
    {
        public const float OpenProgress = 0.2f;
        public const float WetProgress = 0.8f;
        public const float FillStep = 0.02f;
        public const float FillInterval = 0.1f;

        public int stepId = 1;

        bool _completed;
        bool _faucetLocked;
        bool _closePhase;
        bool _handsGrabbed;
        float _fillTimer;

        public override string Id => "OpenFaucet";

        public static float WetnessForProgress(float progress)
        {
            if (progress >= WetProgress)
                return 1f;
            if (progress <= OpenProgress)
                return 0f;
            return Mathf.Clamp01((progress - OpenProgress) / (WetProgress - OpenProgress));
        }

        protected override void OnEnter()
        {
            _completed = false;
            _faucetLocked = false;
            _closePhase = false;
            _handsGrabbed = false;
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
            SubscribeFaucetOpen();
            Services.Faucet.SetEnabled(true);
            Services.Faucet.SetGlow(true);
            if (Services.Faucet.IsOpen)
                LockFaucetOpen(OpenSide());
        }

        protected override void OnTick(float deltaTime)
        {
            if (_completed || !IsEntered || _closePhase)
                return;

            if (!_faucetLocked)
            {
                if (Services.Faucet != null && Services.Faucet.IsOpen)
                    LockFaucetOpen(OpenSide());
                return;
            }

            if (!_handsGrabbed || Services.WaterContact == null || !Services.WaterContact.IsOverlapping)
            {
                _fillTimer = 0f;
                return;
            }

            _fillTimer += deltaTime;
            var progress = Services.ProgressBar != null ? Services.ProgressBar.Progress : OpenProgress;
            while (_fillTimer >= FillInterval && progress < WetProgress)
            {
                _fillTimer -= FillInterval;
                progress = Mathf.Min(WetProgress, progress + FillStep);
                Services.ProgressBar?.SetProgress(progress);
                RaiseWetness(WetnessForProgress(progress));
            }

            if (progress >= WetProgress)
                BeginClosePhase();
        }

        protected override void OnExit()
        {
            UnsubscribeHands();
            UnsubscribeFaucet();
            Services.Hands?.SetGlow(false);
            Services.Hands?.SetDraggable(false);
            Services.Hands?.ReturnHome();
            if (Services.Faucet != null)
            {
                Services.Faucet.SetGlow(false);
                Services.Faucet.SetEnabled(false);
            }
        }

        void OnHandsDragStarted()
        {
            _handsGrabbed = true;
            Services.Hands?.SetGlow(false);
        }

        void OnFaucetActivated(FaucetSide side)
        {
            Debug.Log($"[OpenFaucetStage] Activated {side} entered={IsEntered}");
            LockFaucetOpen(side);
        }

        void OnFaucetPointerHit(FaucetSide side)
        {
            Debug.Log($"[OpenFaucetStage] PointerHit {side} closePhase={_closePhase} entered={IsEntered}");
            if (_closePhase)
                CloseFaucetAndComplete();
            else
                LockFaucetOpen(side);
        }

        void LockFaucetOpen(FaucetSide side)
        {
            if (_faucetLocked || _closePhase || _completed || !IsEntered)
                return;

            _faucetLocked = true;
            UnsubscribeFaucet();
            Services.Faucet?.LockOpen(side);
            Services.Faucet?.SetGlow(false);
            Services.ProgressBar?.SetProgress(OpenProgress);
            Services.Hands?.SetDraggable(true);
            Services.Hands?.SetGlow(true);
            SubscribeHands();
            Debug.Log($"[OpenFaucetStage] Faucet locked open ({side}) at 20%; hands draggable.");
        }

        void BeginClosePhase()
        {
            if (_closePhase || _completed || !IsEntered)
                return;

            _closePhase = true;
            _fillTimer = 0f;
            UnsubscribeHands();
            Services.ProgressBar?.SetProgress(WetProgress);
            RaiseWetness(1f);
            Services.Hands?.SetGlow(false);
            Services.Hands?.SetDraggable(false);
            Services.Hands?.ReturnHome();

            if (Services.Faucet == null)
            {
                Debug.LogWarning("[OpenFaucetStage] No Faucet service; cannot enable close tap.");
                return;
            }

            Debug.Log("[OpenFaucetStage] Hands wet to 80%; faucet glowing for close tap.");
            SubscribeFaucetClose();
            Services.Faucet.SetEnabled(true, rivePointerHits: false);
            Services.Faucet.SetGlow(true);
        }

        void CloseFaucetAndComplete()
        {
            if (!_closePhase || _completed || !IsEntered)
                return;

            UnsubscribeFaucet();
            Services.Faucet?.LockClosed();
            Services.Faucet?.SetGlow(false);
            CompleteOnce();
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

        void SubscribeFaucetOpen()
        {
            if (Services.Faucet == null)
                return;
            Services.Faucet.Activated -= OnFaucetActivated;
            Services.Faucet.Activated += OnFaucetActivated;
            Services.Faucet.PointerHit -= OnFaucetPointerHit;
            Services.Faucet.PointerHit += OnFaucetPointerHit;
        }

        void SubscribeFaucetClose()
        {
            if (Services.Faucet == null)
                return;
            Services.Faucet.Activated -= OnFaucetActivated;
            Services.Faucet.PointerHit -= OnFaucetPointerHit;
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

        void RaiseWetness(float wetness)
        {
            if (Services.Wetness == null)
                return;
            Services.Wetness.SetWetness(Mathf.Max(Services.Wetness.Wetness, wetness));
        }

        public override GameStage CreateRuntime()
        {
            return new OpenFaucetStage { stepId = stepId };
        }
    }
}
