using UnityEngine;

namespace ManosLimpias.Core
{
    [System.Serializable]
    public sealed class RinseSoapStage : GameStage
    {
        public const float OpenProgress = 0.2f;
        public const float RinseProgress = 0.8f;
        public const float FillStep = 0.02f;
        public const float FillInterval = 0.1f;

        public int stepId = 3;

        bool _completed;
        bool _faucetLocked;
        bool _closePhase;
        bool _handsGrabbed;
        float _fillTimer;

        public override string Id => "RinseSoap";

        public static float FoamCoverageForProgress(float progress)
        {
            if (progress >= RinseProgress)
                return 0f;
            if (progress <= OpenProgress)
                return 1f;
            var span = RinseProgress - OpenProgress;
            return Mathf.Clamp01(1f - (progress - OpenProgress) / span);
        }

        public static float WetnessForProgress(float progress)
        {
            return Mathf.Clamp01(1f - FoamCoverageForProgress(progress));
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
            Services.Soap?.SetDraggable(false);
            Services.Soap?.SetGlow(false);
            Services.Hands?.SetDraggable(false);
            Services.Hands?.SetGlow(false);

            if (Services.Faucet == null)
            {
                Debug.LogWarning("[RinseSoapStage] No Faucet service; cannot subscribe to activation.");
                return;
            }

            Debug.Log("[RinseSoapStage] Entered; faucet glowing to open.");
            SubscribeFaucetOpen();
            Services.Faucet.SetEnabled(true, rivePointerHits: false);
            Services.Faucet.SetGlow(true);
        }

        protected override void OnTick(float deltaTime)
        {
            if (_completed || !IsEntered || _closePhase)
                return;

            if (!_faucetLocked)
                return;

            if (!_handsGrabbed || Services.WaterContact == null || !Services.WaterContact.IsOverlapping)
            {
                _fillTimer = 0f;
                return;
            }

            _fillTimer += deltaTime;
            var progress = Services.ProgressBar != null ? Services.ProgressBar.Progress : OpenProgress;
            while (_fillTimer >= FillInterval && progress < RinseProgress)
            {
                _fillTimer -= FillInterval;
                progress = Mathf.Min(RinseProgress, progress + FillStep);
                Services.ProgressBar?.SetProgress(progress);
                Services.SoapFoam?.SetCoverage(FoamCoverageForProgress(progress));
                Services.Wetness?.SetWetness(WetnessForProgress(progress));
            }

            if (progress >= RinseProgress)
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

        void OnFaucetPointerHit(FaucetSide side)
        {
            Debug.Log($"[RinseSoapStage] PointerHit {side} closePhase={_closePhase} entered={IsEntered}");
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
            Debug.Log($"[RinseSoapStage] Faucet locked open ({side}) at 20%; hands draggable.");
        }

        void BeginClosePhase()
        {
            if (_closePhase || _completed || !IsEntered)
                return;

            _closePhase = true;
            _fillTimer = 0f;
            UnsubscribeHands();
            Services.ProgressBar?.SetProgress(RinseProgress);
            Services.SoapFoam?.SetCoverage(0f);
            Services.Wetness?.SetWetness(1f);
            Services.Hands?.SetGlow(false);
            Services.Hands?.SetDraggable(false);
            Services.Hands?.ReturnHome();

            if (Services.Faucet == null)
            {
                Debug.LogWarning("[RinseSoapStage] No Faucet service; cannot enable close tap.");
                return;
            }

            Debug.Log("[RinseSoapStage] Rinse filled to 80%; faucet glowing for close tap.");
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

        void CompleteOnce()
        {
            if (_completed || !IsEntered)
                return;
            _completed = true;
            Services.ProgressBar?.SetProgress(1f);
            Services.StepIcon?.SetState(stepId, active: false, completed: true);
            Services.RequestStageCompletion(this);
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

        void SubscribeFaucetOpen()
        {
            if (Services.Faucet == null)
                return;
            Services.Faucet.PointerHit -= OnFaucetPointerHit;
            Services.Faucet.PointerHit += OnFaucetPointerHit;
        }

        void SubscribeFaucetClose()
        {
            if (Services.Faucet == null)
                return;
            Services.Faucet.PointerHit -= OnFaucetPointerHit;
            Services.Faucet.PointerHit += OnFaucetPointerHit;
        }

        void UnsubscribeFaucet()
        {
            if (Services.Faucet == null)
                return;
            Services.Faucet.PointerHit -= OnFaucetPointerHit;
        }

        public override GameStage CreateRuntime()
        {
            return new RinseSoapStage { stepId = stepId };
        }
    }
}
