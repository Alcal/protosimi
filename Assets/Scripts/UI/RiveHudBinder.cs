using ManosLimpias.UI;
using UnityEngine;

namespace ManosLimpias.UI
{
    /// <summary>
    /// Bridges HudPresenter values to a Rive ViewModel when a .riv + RiveWidget are present.
    /// Safe no-op without the Rive runtime component wired.
    /// Export open protosimi file as Assets/Art/Rive/ManosLimpias_HUD.riv (AB_HUD_Root / VM_HUD).
    /// </summary>
    public class RiveHudBinder : MonoBehaviour
    {
        public HudPresenter hud;
        public bool logBindings;

        // Optional: assign RiveWidget via reflection-friendly object reference to avoid hard compile
        // dependency if package resolve fails on some platforms.
        public Component riveWidget;

        void LateUpdate()
        {
            if (hud == null) return;
            if (logBindings)
            {
                Debug.Log(
                    $"[RiveHudBinder] stageProgress={hud.StageProgress:F2} stageIndex={hud.StageIndex} " +
                    $"icons={hud.IconStates[0]},{hud.IconStates[1]},{hud.IconStates[2]},{hud.IconStates[3]} " +
                    $"hostVisible={hud.HostVisible} hostAssist={hud.HostAssistMode} hudVisible={hud.HudVisible}");
            }

            // When Rive package is imported and widget assigned, user can extend this binder
            // to set VM_HUD properties by name (stageProgress, stageIndex, iconNState, ...).
            if (riveWidget == null) return;
            TryPushToRive();
        }

        void TryPushToRive()
        {
            // Soft binding via SendMessage to avoid compile break if API differs by version.
            riveWidget.SendMessage("SetNumber", new object[] { "stageProgress", hud.StageProgress }, SendMessageOptions.DontRequireReceiver);
            riveWidget.SendMessage("SetNumber", new object[] { "stageIndex", (float)hud.StageIndex }, SendMessageOptions.DontRequireReceiver);
            riveWidget.SendMessage("SetNumber", new object[] { "icon0State", (float)hud.IconStates[0] }, SendMessageOptions.DontRequireReceiver);
            riveWidget.SendMessage("SetNumber", new object[] { "icon1State", (float)hud.IconStates[1] }, SendMessageOptions.DontRequireReceiver);
            riveWidget.SendMessage("SetNumber", new object[] { "icon2State", (float)hud.IconStates[2] }, SendMessageOptions.DontRequireReceiver);
            riveWidget.SendMessage("SetNumber", new object[] { "icon3State", (float)hud.IconStates[3] }, SendMessageOptions.DontRequireReceiver);
            riveWidget.SendMessage("SetBoolean", new object[] { "hostVisible", hud.HostVisible }, SendMessageOptions.DontRequireReceiver);
            riveWidget.SendMessage("SetBoolean", new object[] { "hostAssistMode", hud.HostAssistMode }, SendMessageOptions.DontRequireReceiver);
            riveWidget.SendMessage("SetBoolean", new object[] { "hudVisible", hud.HudVisible }, SendMessageOptions.DontRequireReceiver);
        }
    }
}
