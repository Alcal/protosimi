using UnityEngine;

namespace ManosLimpias.Core
{
    public class CameraFocus : MonoBehaviour
    {
        public FineTuningVariables tuning;
        public Camera targetCamera;
        public Transform[] stageFocusPoints = new Transform[6];

        Vector3 _from;
        Vector3 _to;
        float _t = 1f;
        float _duration = 0.6f;

        void Awake()
        {
            if (targetCamera == null) targetCamera = Camera.main;
        }

        public void EaseToStage(int stageIndex)
        {
            if (targetCamera == null) return;
            if (stageIndex < 0 || stageFocusPoints == null || stageIndex >= stageFocusPoints.Length)
                return;
            var focus = stageFocusPoints[stageIndex];
            if (focus == null) return;

            _from = targetCamera.transform.position;
            float z = tuning != null ? tuning.cameraFocusZ : -10f;
            _to = new Vector3(focus.position.x, focus.position.y, z);
            _duration = tuning != null ? Mathf.Max(0.05f, tuning.cameraEaseSeconds) : 0.6f;
            _t = 0f;
        }

        void LateUpdate()
        {
            if (_t >= 1f || targetCamera == null) return;
            _t += Time.deltaTime / _duration;
            float a = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(_t));
            targetCamera.transform.position = Vector3.Lerp(_from, _to, a);
        }
    }
}
