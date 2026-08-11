using UnityEngine;

namespace ManosLimpias.Core
{
    public class WafController : MonoBehaviour
    {
        public FineTuningVariables tuning;
        public GameFlowController flow;

        bool _tracking;
        float _idleSeconds;
        int _firedLevel;

        public void BeginTracking()
        {
            _tracking = true;
            ResetIdle();
        }

        public void StopTracking()
        {
            _tracking = false;
        }

        public void ResetIdle()
        {
            _idleSeconds = 0f;
            _firedLevel = 0;
        }

        public void NotifyActivity()
        {
            if (flow != null && flow.State == GameFlowState.Assist) return;
            ResetIdle();
        }

        void Update()
        {
            if (!_tracking || tuning == null || flow == null) return;
            if (flow.State != GameFlowState.Stage) return;
            if (flow.stages == null || !flow.stages.AcceptingInput) return;

            _idleSeconds += Time.deltaTime;
            if (_firedLevel < 1 && _idleSeconds >= tuning.waf1IdleSeconds)
            {
                _firedLevel = 1;
                flow.TriggerWaf(1);
            }
            else if (_firedLevel < 2 && _idleSeconds >= tuning.waf2IdleSeconds)
            {
                _firedLevel = 2;
                flow.TriggerWaf(2);
            }
            else if (_firedLevel < 3 && _idleSeconds >= tuning.waf3IdleSeconds)
            {
                _firedLevel = 3;
                flow.TriggerWaf(3);
            }
        }
    }
}
