using UnityEngine;

namespace ManosLimpias.Core
{
    [CreateAssetMenu(fileName = "FineTuningVariables", menuName = "ManosLimpias/FineTuningVariables")]
    public class FineTuningVariables : ScriptableObject
    {
        [Header("Stage advance rates (progress / second of intent)")]
        public float tapOpenCloseRate = 0.55f;
        public float handsUnderWaterRate = 0.35f;
        public float rubOnHandsRate = 0.4f;

        [Header("Intent thresholds")]
        public float tapRadiusWorld = 1.2f;
        public float minDragDistance = 0.15f;
        public float rubDistancePerPulse = 0.35f;

        [Header("WAF idle timers (seconds)")]
        public float waf1IdleSeconds = 4f;
        public float waf2IdleSeconds = 8f;
        public float waf3IdleSeconds = 12f;

        [Header("Camera")]
        public float cameraEaseSeconds = 0.6f;
        public float cameraFocusZ = -10f;

        [Header("Assist hijack")]
        public float hijackProgressPerSecond = 0.45f;

        [Header("Intro")]
        public float introDurationSeconds = 1.25f;
    }
}
