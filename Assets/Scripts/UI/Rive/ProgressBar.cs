using UnityEngine;

namespace ManosLimpias.UI.Rive
{
    /// <summary>
    /// Overlay HUD artboard <c>progressBar</c>.
    /// <c>progress_StateMachine</c> has no SM inputs; fill is a 1D Blend
    /// driven by ViewModel number <c>progressNum</c> in 0–100.
    /// </summary>
    public static class ProgressBar
    {
        public const string Artboard = SimiPrototypeArtboards.ProgressBar;
        public const string StateMachine = "progress_StateMachine";
        public const string WidgetName = "ProgressBarRive";
        public const string ViewModelName = "ProgressBar";
        public const string ProgressNum = "progressNum";
        public const string ProgressNumFallback = "progress_num";
        public const float BlendMax = 100f;

        public static float ToBlend(float normalized) => Mathf.Clamp01(normalized) * BlendMax;
    }
}
