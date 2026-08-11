using UnityEngine;
using UnityEngine.UI;

namespace ManosLimpias.UI
{
    /// <summary>
    /// Unity uGUI presenter mirroring RIVE_INTERFACES ViewModel names.
    /// Primary HUD for WebGL; RiveHudBinder can mirror the same values into a .riv when available.
    /// </summary>
    public class HudPresenter : MonoBehaviour
    {
        public GameObject hudRoot;
        public Image progressFill;
        public Image[] icons = new Image[4];
        public GameObject hostRoot;
        public Outline hostAssistOutline;
        public Text stageLabel;

        public float StageProgress { get; private set; }
        public int StageIndex { get; private set; }
        public int[] IconStates { get; private set; } = { 0, 0, 0, 0 };
        public bool HostVisible { get; private set; }
        public bool HostAssistMode { get; private set; }
        public bool HudVisible { get; private set; }

        static readonly Color Pending = new(0.69f, 0.75f, 0.77f);
        static readonly Color Active = new(0.56f, 0.79f, 0.98f);
        static readonly Color Complete = new(0.3f, 0.69f, 0.31f);

        public void SetHudVisible(bool visible)
        {
            HudVisible = visible;
            if (hudRoot) hudRoot.SetActive(visible);
        }

        public void SetHost(bool visible, bool assist)
        {
            HostVisible = visible;
            HostAssistMode = assist;
            if (hostRoot) hostRoot.SetActive(visible);
            if (hostAssistOutline) hostAssistOutline.enabled = assist;
        }

        public void ApplyStage(int stageIndex, float progress)
        {
            StageIndex = stageIndex;
            StageProgress = progress;
            if (progressFill) progressFill.fillAmount = progress;
            if (stageLabel) stageLabel.text = $"Etapa {stageIndex + 1}/6  {progress:P0}";
            RecomputeIcons(stageIndex);
            PaintIcons();
        }

        public void MarkAllIconsComplete()
        {
            for (int i = 0; i < 4; i++) IconStates[i] = 2;
            if (progressFill) progressFill.fillAmount = 1f;
            PaintIcons();
        }

        public void PulseStageComplete() { /* visual juice hook */ }
        public void PulseHostSpeak() { }
        public void PulseWafHighlight()
        {
            if (icons == null) return;
            int active = ActiveIconFor(StageIndex);
            if (active >= 0 && active < icons.Length && icons[active] != null)
            {
                icons[active].transform.localScale = Vector3.one * 1.15f;
            }
        }

        void LateUpdate()
        {
            if (icons == null) return;
            foreach (var icon in icons)
            {
                if (icon == null) continue;
                icon.transform.localScale = Vector3.Lerp(icon.transform.localScale, Vector3.one, Time.deltaTime * 6f);
            }
        }

        void RecomputeIcons(int stageIndex)
        {
            // pending=0 active=1 complete=2 — shared icon reactivation rules
            IconStates[0] = stageIndex switch
            {
                0 => 1,
                >= 4 => stageIndex == 4 ? 1 : 2,
                > 0 => 2,
                _ => 0
            };
            IconStates[1] = stageIndex switch
            {
                1 => 1,
                3 => 1,
                >= 4 => 2,
                2 => 2,
                _ => 0
            };
            IconStates[2] = stageIndex switch
            {
                2 => 1,
                >= 3 => 2,
                _ => 0
            };
            IconStates[3] = stageIndex switch
            {
                5 => 1,
                > 5 => 2,
                _ => stageIndex > 5 ? 2 : 0
            };
            if (stageIndex > 5)
            {
                for (int i = 0; i < 4; i++) IconStates[i] = 2;
            }
        }

        static int ActiveIconFor(int stageIndex) => stageIndex switch
        {
            0 or 4 => 0,
            1 or 3 => 1,
            2 => 2,
            5 => 3,
            _ => -1
        };

        void PaintIcons()
        {
            if (icons == null) return;
            for (int i = 0; i < icons.Length; i++)
            {
                if (icons[i] == null) continue;
                icons[i].color = IconStates[i] switch
                {
                    1 => Active,
                    2 => Complete,
                    _ => Pending
                };
            }
        }
    }
}
