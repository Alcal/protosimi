using System;
using UnityEngine;

namespace ManosLimpias.Core
{
    [Serializable]
    public sealed class OpenFaucetStage : GameStage
    {
        public int stepId = 1;

        bool _completed;

        public override string Id => "OpenFaucet";

        protected override void OnEnter()
        {
            _completed = false;
            Services.ProgressBar?.SetProgress(0f);
            Services.StepIcon?.SetState(stepId, active: true, completed: false);

            if (Services.Faucet == null)
            {
                Debug.LogWarning("[OpenFaucetStage] No Faucet service; cannot subscribe to activation.");
                return;
            }

            Debug.Log("[OpenFaucetStage] Entered; subscribed to Faucet pointer hit.");
            Services.Faucet.Activated += OnFaucetActivated;
            Services.Faucet.PointerHit += OnFaucetPointerHit;
            Services.Faucet.SetEnabled(true);
            if (Services.Faucet.IsOpen)
                CompleteOnce();
        }

        protected override void OnTick(float deltaTime)
        {
            if (Services.Faucet != null && Services.Faucet.IsOpen)
                CompleteOnce();
        }

        protected override void OnExit()
        {
            if (Services.Faucet == null) return;
            Services.Faucet.Activated -= OnFaucetActivated;
            Services.Faucet.PointerHit -= OnFaucetPointerHit;
            Services.Faucet.SetEnabled(false);
        }

        void OnFaucetActivated(FaucetSide side)
        {
            Debug.Log($"[OpenFaucetStage] Activated {side} entered={IsEntered}");
            CompleteOnce();
        }

        void OnFaucetPointerHit()
        {
            Debug.Log($"[OpenFaucetStage] PointerHit entered={IsEntered}");
            CompleteOnce();
        }

        void CompleteOnce()
        {
            if (_completed || !IsEntered) return;
            _completed = true;
            Services.ProgressBar?.SetProgress(1f);
            Services.StepIcon?.SetState(stepId, active: false, completed: true);
            Services.RequestStageCompletion(this);
        }

        public override GameStage CreateRuntime()
        {
            return new OpenFaucetStage { stepId = stepId };
        }
    }
}
