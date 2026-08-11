using ManosLimpias.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ManosLimpias.UI
{
    public class WinController : MonoBehaviour
    {
        public GameFlowController flow;
        public Button replayButton;

        void Awake()
        {
            if (replayButton != null)
                replayButton.onClick.AddListener(OnReplay);
        }

        public void OnReplay()
        {
            flow?.OnReplayPressed();
        }
    }
}
