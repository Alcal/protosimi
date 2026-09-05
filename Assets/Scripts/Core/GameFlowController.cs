using System;
using System.Collections.Generic;
using ManosLimpias.Analytics;
using ManosLimpias.Audio;
using ManosLimpias.UI;
using ManosLimpias.UI.Rive;
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
        Outro,
        Win
    }

    public class GameFlowController : MonoBehaviour
    {
        public FineTuningVariables tuning;
        // Retained for legacy WAF/input components during the migration.
        public StageController stages;
        public HudPresenter hud;
        public RiveHudBinder riveHud;
        public Faucet faucet;
        public Hands hands;
        public Soap soap;
        public AudioPlaceholderPlayer audioPlayer;
        public WafController waf;
        public AssistHijack assist;
        public CameraFocus cameraFocus;
        public PlayfieldFoci playfield;
        public GermPop germs;
        public GameObject winRoot;
        public GameObject playRoot;
        public string titleSceneName = "Title";

        [SerializeField, SerializeReference]
        public List<GameStage> stageConfigurations = new();

        public GameFlowState State { get; private set; } = GameFlowState.Intro;
        public int ActiveStageIndex { get; private set; } = -1;
        public IReadOnlyList<GameStage> RuntimeStages => _runtimeStages;
        public GameStage ActiveStage =>
            ActiveStageIndex >= 0 && ActiveStageIndex < _runtimeStages.Count
                ? _runtimeStages[ActiveStageIndex]
                : null;

        [NonSerialized] public IGameFlowServices ServicesOverride;

        readonly List<GameStage> _runtimeStages = new();
        IGameFlowServices _services;
        float _sessionStart;
        float _stageStart;

        void Awake()
        {
            if (waf == null) waf = GetComponent<WafController>();
            if (assist == null) assist = GetComponent<AssistHijack>();
            _services = new FlowServices(this);
        }

        void Start()
        {
            StartSession();
        }

        public void StartSession()
        {
            if (_services == null || ServicesOverride != null)
                _services = ServicesOverride ?? new FlowServices(this);
            _sessionStart = Time.time;
            AnalyticsStub.SessionStart();
            EnterIntro();
        }

        public void DismissIntro()
        {
            if (State != GameFlowState.Intro) return;
            EnterStage(0);
        }

        public void OnReplayPressed()
        {
            StopActiveStage();
            waf?.StopTracking();
            assist?.Stop();
            SceneManager.LoadScene(titleSceneName);
        }

        void EnterIntro()
        {
            StopActiveStage();
            BuildRuntimeStages();
            State = GameFlowState.Intro;
            ActiveStageIndex = -1;
            SetRoots(play: true, win: false);
            hud?.SetHudVisible(false);
            hud?.SetStageCount(_runtimeStages.Count);
            hud?.SetHost(true, false);
            if (stages != null) stages.AcceptingInput = false;
            audioPlayer?.Play("vo_welcome");
            hud?.PulseHostSpeak();
            cameraFocus?.EaseToStage(0);
            playfield?.SetActiveStage(-1);
            germs?.ResetGerms();
            _services?.StepIcon?.SetState(1, active: false, completed: false);
            _services?.ProgressBar?.SetProgress(0f);
        }

        void EnterStage(int index)
        {
            if (index < 0 || index >= _runtimeStages.Count)
            {
                EnterOutro();
                return;
            }

            State = GameFlowState.Stage;
            ActiveStageIndex = index;
            _stageStart = Time.time;
            hud?.SetHudVisible(true);
            hud?.SetHost(false, false);
            hud?.ApplyStage(index, 0f);
            playfield?.SetActiveStage(index);
            cameraFocus?.EaseToStage(index);
            _runtimeStages[index].Initialize(_services);
            _runtimeStages[index].Enter();
            AnalyticsStub.StageStart(index);
            audioPlayer?.Play($"vo_stage_{index}");
            hud?.PulseHostSpeak();
        }

        void Update()
        {
            if (State == GameFlowState.Stage)
                ActiveStage?.Tick(Time.deltaTime);
        }

        void StopActiveStage()
        {
            ActiveStage?.Exit();
            ActiveStageIndex = -1;
        }

        public void RequestStageCompletion(GameStage stage)
        {
            if (State != GameFlowState.Stage || stage == null || stage != ActiveStage)
                return;

            stage.Exit();
            AnalyticsStub.StageComplete(ActiveStageIndex, Time.time - _stageStart);
            audioPlayer?.Play("sfx_caf_positive");
            audioPlayer?.Play("vo_caf_praise");
            hud?.PulseStageComplete();
            hud?.PulseHostSpeak();
            hud?.SetHost(true, false);

            int nextIndex = ActiveStageIndex + 1;
            if (nextIndex >= _runtimeStages.Count)
            {
                EnterOutro();
                return;
            }

            EnterStage(nextIndex);
        }

        void EnterOutro()
        {
            StopActiveStage();
            State = GameFlowState.Outro;
            waf?.StopTracking();
            assist?.Stop();
            SetRoots(play: true, win: true);
            hud?.SetHudVisible(true);
            hud?.SetHost(true, false);
            audioPlayer?.Play("vo_complete");
            hud?.PulseHostSpeak();
            AnalyticsStub.SessionComplete(Time.time - _sessionStart);
            playfield?.SetActiveStage(-1);
        }

        public void EnterAssist()
        {
            if (State != GameFlowState.Stage) return;
            State = GameFlowState.Assist;
            AnalyticsStub.AssistHijack(ActiveStageIndex);
            AnalyticsStub.WafTriggered(ActiveStageIndex, 3);
            hud?.SetHost(true, true);
            audioPlayer?.Play("vo_waf_assist");
            hud?.PulseHostSpeak();
        }

        public void ResumeActiveStage()
        {
            if (State != GameFlowState.Assist || ActiveStage == null) return;
            State = GameFlowState.Stage;
            hud?.SetHost(false, false);
        }

        public void TriggerWaf(int level)
        {
            AnalyticsStub.WafTriggered(ActiveStageIndex, level);
            if (level == 1)
                hud?.PulseWafHighlight();
            else if (level == 2)
            {
                hud?.SetHost(true, false);
                audioPlayer?.Play("vo_waf_hint");
                hud?.PulseHostSpeak();
            }
            else if (level >= 3)
                EnterAssist();
        }

        void SetRoots(bool play, bool win)
        {
            if (playRoot) playRoot.SetActive(play);
            if (winRoot) winRoot.SetActive(win);
        }

        void BuildRuntimeStages()
        {
            _runtimeStages.Clear();
            if (stageConfigurations != null)
            {
                for (int i = 0; i < stageConfigurations.Count; i++)
                {
                    var configuration = stageConfigurations[i];
                    if (configuration == null)
                    {
                        Debug.LogWarning($"[GameFlowController] stageConfigurations[{i}] is null (SerializeReference lost). Skipping.");
                        continue;
                    }

                    _runtimeStages.Add(configuration.CreateRuntime());
                }
            }

            if (_runtimeStages.Count == 0)
            {
                Debug.LogWarning("[GameFlowController] No usable stages configured; defaulting to OpenFaucetStage.");
                _runtimeStages.Add(new OpenFaucetStage());
            }
        }

        sealed class FlowServices : IGameFlowServices
        {
            readonly GameFlowController _flow;
            readonly IProgressBarControl _fallbackProgress;
            readonly IStepIconControl _fallbackStepIcon;
            readonly NullHands _nullHands = new();
            readonly NullSoap _nullSoap = new();

            public FlowServices(GameFlowController flow)
            {
                _flow = flow;
                _fallbackProgress = new HudProgressAdapter(flow.hud);
                _fallbackStepIcon = new NullStepIcon();
            }

            public IFaucetControl Faucet => _flow.faucet;
            public IHandsControl Hands => _flow.hands != null ? _flow.hands : _nullHands;
            public IWaterContactControl WaterContact => _flow.hands != null ? _flow.hands : _nullHands;
            public ISoapControl Soap => _flow.soap != null ? _flow.soap : _nullSoap;
            public IProgressBarControl ProgressBar => _flow.riveHud ?? _fallbackProgress;
            public IStepIconControl StepIcon => _flow.riveHud ?? _fallbackStepIcon;

            public void RequestStageCompletion(GameStage stage)
            {
                _flow.RequestStageCompletion(stage);
            }
        }

        sealed class HudProgressAdapter : IProgressBarControl
        {
            readonly HudPresenter _hud;
            public float Progress { get; private set; }

            public HudProgressAdapter(HudPresenter hud)
            {
                _hud = hud;
            }

            public void SetProgress(float progress)
            {
                Progress = Mathf.Clamp01(progress);
                _hud?.ApplyStage(_hud.StageIndex, Progress);
            }
        }

        sealed class NullStepIcon : IStepIconControl
        {
            public void SetState(int stepId, bool active, bool completed) { }
        }

        sealed class NullHands : IHandsControl, IWaterContactControl
        {
#pragma warning disable CS0067
            public event Action DragStarted;
#pragma warning restore CS0067
            public bool IsDraggable { get; private set; }
            public bool IsGlowing { get; private set; }
            public bool IsOverlapping => false;

            public void SetDraggable(bool draggable)
            {
                IsDraggable = draggable;
            }

            public void SetGlow(bool on)
            {
                IsGlowing = on;
            }
        }

        sealed class NullSoap : ISoapControl
        {
#pragma warning disable CS0067
            public event Action DragStarted;
#pragma warning restore CS0067
            public bool IsDraggable { get; private set; }
            public bool IsGlowing { get; private set; }
            public bool IsOverlapping => false;

            public void SetDraggable(bool draggable)
            {
                IsDraggable = draggable;
            }

            public void SetGlow(bool on)
            {
                IsGlowing = on;
            }

            public void ReturnHome() { }
        }
    }
}
