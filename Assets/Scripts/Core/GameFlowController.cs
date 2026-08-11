using ManosLimpias.Analytics;
using ManosLimpias.Audio;
using ManosLimpias.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ManosLimpias.Core
{
    public enum GameFlowState
    {
        Title,
        Intro,
        Stage,
        Assist,
        Win
    }

    public class GameFlowController : MonoBehaviour
    {
        public FineTuningVariables tuning;
        public StageController stages;
        public HudPresenter hud;
        public AudioPlaceholderPlayer audioPlayer;
        public WafController waf;
        public AssistHijack assist;
        public CameraFocus cameraFocus;
        public PlayfieldFoci playfield;
        public GermPop germs;
        public GameObject winRoot;
        public GameObject playRoot;
        public string titleSceneName = "Title";

        public GameFlowState State { get; private set; } = GameFlowState.Intro;

        float _sessionStart;
        float _introEndsAt;

        void Awake()
        {
            if (stages == null) stages = GetComponent<StageController>();
            if (waf == null) waf = GetComponent<WafController>();
            if (assist == null) assist = GetComponent<AssistHijack>();
        }

        void Start()
        {
            _sessionStart = Time.time;
            AnalyticsStub.SessionStart();
            EnterIntro();
        }

        void Update()
        {
            if (State == GameFlowState.Intro && Time.time >= _introEndsAt)
                EnterStagePlay();
        }

        public void OnReplayPressed()
        {
            SubscribeStages(false);
            waf?.StopTracking();
            assist?.Stop();
            SceneManager.LoadScene(titleSceneName);
        }

        void EnterIntro()
        {
            State = GameFlowState.Intro;
            SetRoots(play: true, win: false);
            hud?.SetHudVisible(false);
            hud?.SetHost(true, false);
            stages?.ResetSession();
            stages.AcceptingInput = false;
            audioPlayer?.Play("vo_welcome");
            hud?.PulseHostSpeak();
            float dur = tuning != null ? tuning.introDurationSeconds : 1.25f;
            _introEndsAt = Time.time + dur;
            cameraFocus?.EaseToStage(0);
            playfield?.SetActiveStage(-1);
            germs?.ResetGerms();
        }

        void EnterStagePlay()
        {
            State = GameFlowState.Stage;
            hud?.SetHudVisible(true);
            hud?.SetHost(false, false);
            stages.Begin(0);
            WireStage(0);
            AnalyticsStub.StageStart(0);
            audioPlayer?.Play("vo_stage_0");
            hud?.PulseHostSpeak();
            SubscribeStages(true);
            waf?.BeginTracking();
        }

        void WireStage(int index)
        {
            playfield?.SetActiveStage(index);
            cameraFocus?.EaseToStage(index);
            hud?.ApplyStage(index, stages.Progress);
            if (index == 2 || index == 3)
                germs?.EnsureGerms();
        }

        void SubscribeStages(bool on)
        {
            if (stages == null) return;
            stages.StageStarted -= OnStageStarted;
            stages.StageCompleted -= OnStageCompleted;
            stages.ProgressChanged -= OnProgressChanged;
            stages.AllStagesCompleted -= OnAllComplete;
            if (!on) return;
            stages.StageStarted += OnStageStarted;
            stages.StageCompleted += OnStageCompleted;
            stages.ProgressChanged += OnProgressChanged;
            stages.AllStagesCompleted += OnAllComplete;
        }

        void OnStageStarted(int index)
        {
            if (State == GameFlowState.Assist)
                ExitAssistKeepStage();
            AnalyticsStub.StageStart(index);
            WireStage(index);
            audioPlayer?.Play($"vo_stage_{index}");
            hud?.PulseHostSpeak();
            waf?.ResetIdle();
        }

        void OnProgressChanged()
        {
            hud?.ApplyStage(stages.StageIndex, stages.Progress);
            waf?.NotifyActivity();
            if (stages.StageIndex is 2 or 3)
                germs?.UpdateFromProgress(stages.Progress);
        }

        void OnStageCompleted(int index, float duration)
        {
            AnalyticsStub.StageComplete(index, duration);
            audioPlayer?.Play("sfx_caf_positive");
            audioPlayer?.Play("vo_caf_praise");
            hud?.PulseStageComplete();
            hud?.PulseHostSpeak();
            hud?.SetHost(true, false);
        }

        void OnAllComplete()
        {
            EnterWin();
        }

        public void EnterAssist()
        {
            if (State != GameFlowState.Stage) return;
            State = GameFlowState.Assist;
            AnalyticsStub.AssistHijack(stages.StageIndex);
            AnalyticsStub.WafTriggered(stages.StageIndex, 3);
            hud?.SetHost(true, true);
            audioPlayer?.Play("vo_waf_assist");
            hud?.PulseHostSpeak();
            assist?.StartHijack(stages);
        }

        void ExitAssistKeepStage()
        {
            State = GameFlowState.Stage;
            assist?.Stop();
            hud?.SetHost(false, false);
        }

        void EnterWin()
        {
            SubscribeStages(false);
            State = GameFlowState.Win;
            waf?.StopTracking();
            assist?.Stop();
            stages.AcceptingInput = false;
            SetRoots(play: true, win: true);
            hud?.SetHudVisible(true);
            hud?.MarkAllIconsComplete();
            hud?.SetHost(true, false);
            audioPlayer?.Play("vo_complete");
            hud?.PulseHostSpeak();
            AnalyticsStub.SessionComplete(Time.time - _sessionStart);
            playfield?.SetActiveStage(-1);
        }

        void SetRoots(bool play, bool win)
        {
            if (playRoot) playRoot.SetActive(play);
            if (winRoot) winRoot.SetActive(win);
        }

        public void TriggerWaf(int level)
        {
            AnalyticsStub.WafTriggered(stages.StageIndex, level);
            if (level == 1)
            {
                hud?.PulseWafHighlight();
            }
            else if (level == 2)
            {
                hud?.SetHost(true, false);
                audioPlayer?.Play("vo_waf_hint");
                hud?.PulseHostSpeak();
            }
            else if (level >= 3)
            {
                EnterAssist();
            }
        }
    }
}
