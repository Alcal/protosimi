using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ManosLimpias.UI
{
    public class TitleController : MonoBehaviour
    {
        public Button playButton;
        public string gameplaySceneName = "Gameplay";

        void Awake()
        {
            if (playButton != null)
                playButton.onClick.AddListener(OnPlay);
        }

        public void OnPlay()
        {
            ManosLimpias.Analytics.AnalyticsStub.PlayPressed();
            SceneManager.LoadScene(gameplaySceneName);
        }
    }
}
