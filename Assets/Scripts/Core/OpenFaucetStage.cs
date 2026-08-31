using System;
using UnityEngine;

namespace ManosLimpias.Core
{
    [Serializable]
    public sealed class OpenFaucetStage : GameStage
    {
        public int stepId = 1;

        public override string Id => "OpenFaucet";

        protected override void OnEnter()
        {
            Services.ProgressBar?.SetProgress(0f);
            Services.StepIcon?.SetState(stepId, active: true, completed: false);

            if (Services.Faucet == null)
            {
                Debug.LogWarning("[OpenFaucetStage] No Faucet service; cannot subscribe to activation.");
                return;
            }

            Debug.Log("[OpenFaucetStage] Entered; subscribed to Faucet.Activated.");
            Services.Faucet.Activated += OnFaucetActivated;
            Services.Faucet.SetEnabled(true);
        }

        protected override void OnExit()
        {
            if (Services.Faucet == null) return;
            Services.Faucet.Activated -= OnFaucetActivated;
            Services.Faucet.SetEnabled(false);
        }

        void OnFaucetActivated(FaucetSide side)
        {
            Debug.Log($"[OpenFaucetStage] Activated {side} entered={IsEntered}");
            if (!IsEntered) return;
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
