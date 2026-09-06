using UnityEngine;

namespace ManosLimpias.Core
{
    [System.Serializable]
    public sealed class RinseSoapStage : GameStage
    {
        public const float RinseProgress = 0.75f;
        public const float FillStep = 0.02f;
        public const float FillInterval = 0.1f;

        public int stepId = 3;

        bool _completed;
        bool _closePhase;
        bool _handsGrabbed;
        float _fillTimer;

        public override string Id => "RinseSoap";

        public static float FoamCoverageForProgress(float progress)
        {
            return Mathf.Clamp01(1f - progress / RinseProgress);
        }

        protected override void OnEnter()
        {
            _completed = false;
            _closePhase = false;
            _handsGrabbed = false;
            _fillTimer = 0f;
            Services.ProgressBar?.SetProgress(0f);
            Services.StepIcon?.SetState(stepId, active: true, completed: false);
            Services.Soap?.SetDraggable(false);
            Services.Soap?.SetGlow(false);
            if (Services.Faucet != null)
            {
                Services.Faucet.SetGlow(false);
                Services.Faucet.SetEnabled(false);
            }

            if (Services.Hands == null)
            {
                Debug.LogWarning("[RinseSoapStage] No Hands service; cannot enable drag.");
                return;
            }

            Debug.Log("[RinseSoapStage] Entered; hands glowing and draggable.");
            SubscribeHands();
            Services.Hands.SetDraggable(true);
            Services.Hands.SetGlow(true);
        }

        protected override void OnTick(float deltaTime)
        {
            if (_completed || !IsEntered || _closePhase)
                return;

            if (!_handsGrabbed || Services.WaterContact == null || !Services.WaterContact.IsOverlapping)
            {
                _fillTimer = 0f;
                return;
            }

            _fillTimer += deltaTime;
            var progress = Services.ProgressBar != null ? Services.ProgressBar.Progress : 0f;
            while (_fillTimer >= FillInterval && progress < RinseProgress)
            {
                _fillTimer -= FillInterval;
                progress = Mathf.Min(RinseProgress, progress + FillStep);
                Services.ProgressBar?.SetProgress(progress);
                Services.SoapFoam?.SetCoverage(FoamCoverageForProgress(progress));
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
            CloseFaucetAndComplete();
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
            Services.Hands?.SetGlow(false);
            Services.Hands?.SetDraggable(false);
            Services.Hands?.ReturnHome();

            if (Services.Faucet == null)
            {
                Debug.LogWarning("[RinseSoapStage] No Faucet service; cannot enable close tap.");
                return;
            }

            Debug.Log("[RinseSoapStage] Rinse filled to 75%; faucet glowing for close tap.");
            SubscribeFaucet();
            Services.Faucet.SetEnabled(true);
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

        void SubscribeFaucet()
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
