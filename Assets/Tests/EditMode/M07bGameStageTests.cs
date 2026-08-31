using System;
using System.Collections.Generic;
using ManosLimpias.Core;
using NUnit.Framework;
using UnityEngine;

namespace ManosLimpias.Tests
{
    public class M07bGameStageTests
    {
        GameObject _flowObject;
        GameFlowController _flow;
        FakeServices _services;

        [SetUp]
        public void SetUp()
        {
            _flowObject = new GameObject("M07b Flow Test");
            _flow = _flowObject.AddComponent<GameFlowController>();
            _services = new FakeServices();
            _flow.ServicesOverride = _services;
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_flowObject);
        }

        [Test]
        public void AC01_FirstStageReceivesServices_OnIntroDismissed()
        {
            var stage = new RecordingStage("first");
            _flow.stageConfigurations = new List<GameStage> { stage };

            _flow.StartSession();
            _flow.DismissIntro();

            Assert.That(_flow.State, Is.EqualTo(GameFlowState.Stage));
            Assert.That(_flow.ActiveStage.Id, Is.EqualTo("first"));
            var runtimeStage = (RecordingStage)_flow.ActiveStage;
            Assert.That(runtimeStage.Entered, Is.EqualTo(1));
            Assert.That(runtimeStage.ReceivedServices, Is.SameAs(_services));
        }

        [Test]
        public void AC02_FlowEntersStages_InConfiguredOrder()
        {
            var order = new List<string>();
            var first = new RecordingStage("first", order);
            var second = new RecordingStage("second", order);
            _flow.stageConfigurations = new List<GameStage> { first, second };
            _services.CompletionRequested = stage => _flow.RequestStageCompletion(stage);

            _flow.StartSession();
            _flow.DismissIntro();
            ((RecordingStage)_flow.ActiveStage).Complete();
            ((RecordingStage)_flow.ActiveStage).Complete();

            Assert.That(order, Is.EqualTo(new[] { "enter:first", "exit:first", "enter:second", "exit:second" }));
            Assert.That(_flow.State, Is.EqualTo(GameFlowState.Outro));
        }

        [Test]
        public void AC03_OpenFaucetCompletes_OnEitherHandle()
        {
            var stage = new OpenFaucetStage();
            _services.CompletionRequested = _ => { };
            stage.Initialize(_services);
            stage.Enter();

            Assert.That(_services.Icon.StepId, Is.EqualTo(1));
            Assert.That(_services.Icon.Active, Is.True);
            Assert.That(_services.Icon.Completed, Is.False);

            _services.Faucet.Raise(FaucetSide.Left);
            Assert.That(_services.Progress.Progress, Is.EqualTo(1f));
            Assert.That(_services.CompletionCount, Is.EqualTo(1));
            Assert.That(_services.Icon.Active, Is.False);
            Assert.That(_services.Icon.Completed, Is.True);

            stage.Exit();
            _services.Faucet.Raise(FaucetSide.Right);
            Assert.That(_services.CompletionCount, Is.EqualTo(1));
        }

        [Test]
        public void OpenFaucet_SetsStepIconActive_WhenIntroDismissed()
        {
            _flow.stageConfigurations = new List<GameStage> { new OpenFaucetStage() };

            _flow.StartSession();
            Assert.That(_services.Icon.Active, Is.False);
            Assert.That(_services.Icon.Completed, Is.False);

            _flow.DismissIntro();

            Assert.That(_flow.ActiveStage, Is.InstanceOf<OpenFaucetStage>());
            Assert.That(_services.Icon.StepId, Is.EqualTo(1));
            Assert.That(_services.Icon.Active, Is.True);
            Assert.That(_services.Icon.Completed, Is.False);
        }

        [Test]
        public void AC04_OpenFaucetCleansUp_OnExit()
        {
            var stage = new OpenFaucetStage();
            stage.Initialize(_services);
            stage.Enter();
            stage.Exit();

            Assert.That(_services.Faucet.IsEnabled, Is.False);
            _services.Faucet.Raise(FaucetSide.Left);
            Assert.That(_services.CompletionCount, Is.Zero);
        }

        [Test]
        public void AC05_InactiveStageIgnoresFaucetEvents()
        {
            var stage = new OpenFaucetStage();
            stage.Initialize(_services);
            stage.Enter();
            stage.Exit();

            _services.Faucet.Raise(FaucetSide.Right);

            Assert.That(_services.CompletionCount, Is.Zero);
        }

        [Test]
        public void AC07_ReplayResetsStageLifecycle()
        {
            _flow.stageConfigurations = new List<GameStage> { new RecordingStage("first") };
            _services.CompletionRequested = stage => _flow.RequestStageCompletion(stage);

            _flow.StartSession();
            _flow.DismissIntro();
            ((RecordingStage)_flow.ActiveStage).Complete();
            _services.Icon.SetState(1, active: false, completed: true);
            _flow.StartSession();

            Assert.That(_flow.State, Is.EqualTo(GameFlowState.Intro));
            Assert.That(_flow.ActiveStageIndex, Is.EqualTo(-1));
            Assert.That(_flow.RuntimeStages.Count, Is.EqualTo(1));
            Assert.That(_flow.RuntimeStages[0].IsEntered, Is.False);
            Assert.That(_services.Icon.Active, Is.False);
            Assert.That(_services.Icon.Completed, Is.False);
        }

        [Test]
        public void StepIconBind_PreservesPendingStageState()
        {
            var binderObject = new GameObject("M07b Hud Binder");
            var binder = binderObject.AddComponent<ManosLimpias.UI.RiveHudBinder>();

            binder.SetState(1, active: true, completed: false);
            binder.Bind(null, null);

            Assert.That(binder.HasStepState, Is.True);
            Assert.That(binder.StepId, Is.EqualTo(1));
            Assert.That(binder.StepActive, Is.True);
            Assert.That(binder.StepCompleted, Is.False);

            UnityEngine.Object.DestroyImmediate(binderObject);
        }

        [Test]
        public void NullStageConfig_DefaultsToOpenFaucet()
        {
            _flow.stageConfigurations = new List<GameStage> { null };

            _flow.StartSession();
            _flow.DismissIntro();

            Assert.That(_flow.State, Is.EqualTo(GameFlowState.Stage));
            Assert.That(_flow.ActiveStage, Is.InstanceOf<OpenFaucetStage>());
            Assert.That(_services.Icon.Active, Is.True);
        }

        sealed class RecordingStage : GameStage
        {
            readonly List<string> _order;
            public override string Id { get; }
            public int Entered { get; private set; }
            public IGameFlowServices ReceivedServices => Services;

            public RecordingStage(string id, List<string> order = null)
            {
                Id = id;
                _order = order;
            }

            public void Complete()
            {
                Services.RequestStageCompletion(this);
            }

            protected override void OnEnter()
            {
                Entered++;
                _order?.Add($"enter:{Id}");
            }

            protected override void OnExit()
            {
                _order?.Add($"exit:{Id}");
            }

            public override GameStage CreateRuntime()
            {
                return new RecordingStage(Id, _order);
            }
        }

        sealed class FakeServices : IGameFlowServices
        {
            public readonly FakeFaucet Faucet = new();
            public readonly FakeProgress Progress = new();
            public readonly FakeIcon Icon = new();
            public Action<GameStage> CompletionRequested;
            public int CompletionCount { get; private set; }

            IFaucetControl IGameFlowServices.Faucet => Faucet;
            IProgressBarControl IGameFlowServices.ProgressBar => Progress;
            IStepIconControl IGameFlowServices.StepIcon => Icon;

            public void RequestStageCompletion(GameStage stage)
            {
                CompletionCount++;
                CompletionRequested?.Invoke(stage);
            }
        }

        sealed class FakeFaucet : IFaucetControl
        {
            public event Action<FaucetSide> Activated;
            public bool IsEnabled { get; private set; }

            public void SetEnabled(bool enabled)
            {
                IsEnabled = enabled;
            }

            public void Raise(FaucetSide side)
            {
                if (IsEnabled)
                    Activated?.Invoke(side);
            }
        }

        sealed class FakeProgress : IProgressBarControl
        {
            public float Progress { get; private set; }

            public void SetProgress(float progress)
            {
                Progress = Mathf.Clamp01(progress);
            }
        }

        sealed class FakeIcon : IStepIconControl
        {
            public int StepId { get; private set; }
            public bool Active { get; private set; }
            public bool Completed { get; private set; }

            public void SetState(int stepId, bool active, bool completed)
            {
                StepId = stepId;
                Active = active;
                Completed = completed;
            }
        }
    }
}
