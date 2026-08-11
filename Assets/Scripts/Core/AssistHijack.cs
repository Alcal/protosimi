using UnityEngine;

namespace ManosLimpias.Core
{
    public class AssistHijack : MonoBehaviour
    {
        public FineTuningVariables tuning;
        StageController _stages;
        bool _active;

        public void StartHijack(StageController stages)
        {
            _stages = stages;
            _active = true;
            if (_stages != null)
                _stages.AcceptingInput = true;
        }

        public void Stop()
        {
            _active = false;
            _stages = null;
        }

        void Update()
        {
            if (!_active || _stages == null || tuning == null) return;
            int before = _stages.StageIndex;
            _stages.AddProgress(tuning.hijackProgressPerSecond * Time.deltaTime);
            // Stop after CAF advances or win — StageController handles completion.
            if (_stages.StageIndex != before || !_stages.AcceptingInput)
                Stop();
        }
    }
}
