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
        public void AC03_OpenFaucetLocksAtQuarter_OnEitherHandle()
        {
            var stage = new OpenFaucetStage();
            _services.CompletionRequested = _ => { };
            stage.Initialize(_services);
            stage.Enter();

            Assert.That(_services.Icon.StepId, Is.EqualTo(1));
            Assert.That(_services.Icon.Active, Is.True);
            Assert.That(_services.Icon.Completed, Is.False);

            _services.Faucet.Raise(FaucetSide.Left);
            Assert.That(_services.Progress.Progress, Is.EqualTo(OpenFaucetStage.OpenProgress));
            Assert.That(_services.CompletionCount, Is.EqualTo(0));
            Assert.That(_services.Faucet.IsEnabled, Is.False);
            Assert.That(_services.Faucet.IsGlowing, Is.False);
            Assert.That(_services.Hands.IsDraggable, Is.True);
            Assert.That(_services.Hands.IsGlowing, Is.True);
            Assert.That(_services.Icon.Active, Is.True);
            Assert.That(_services.Icon.Completed, Is.False);

            stage.Exit();
            _services.Faucet.Raise(FaucetSide.Right);
            Assert.That(_services.CompletionCount, Is.EqualTo(0));
            Assert.That(_services.Hands.IsDraggable, Is.False);
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
            Assert.That(_services.Faucet.IsGlowing, Is.False);
            Assert.That(_services.Hands.IsDraggable, Is.False);
            Assert.That(_services.Hands.IsGlowing, Is.False);
            _services.Faucet.Raise(FaucetSide.Left);
            Assert.That(_services.CompletionCount, Is.Zero);
        }

        [Test]
        public void OpenFaucet_GlowsFaucetUntilTap_ThenHandsUntilGrab()
        {
            var stage = new OpenFaucetStage();
            stage.Initialize(_services);
            stage.Enter();

            Assert.That(_services.Faucet.IsGlowing, Is.True);
            Assert.That(_services.Hands.IsGlowing, Is.False);

            _services.Faucet.Raise(FaucetSide.Left);
            Assert.That(_services.Faucet.IsGlowing, Is.False);
            Assert.That(_services.Hands.IsGlowing, Is.True);
            Assert.That(_services.Hands.IsDraggable, Is.True);

            _services.Hands.RaiseDragStarted();
            Assert.That(_services.Hands.IsGlowing, Is.False);

            stage.Exit();
            Assert.That(_services.Faucet.IsGlowing, Is.False);
            Assert.That(_services.Hands.IsGlowing, Is.False);
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
        public void OpenFaucet_LocksAtQuarter_WhenIsOpenOnTick()
        {
            var stage = new OpenFaucetStage();
            _services.CompletionRequested = _ => { };
            stage.Initialize(_services);
            stage.Enter();

            _services.Faucet.SetOpen(FaucetSide.Right, true);
            stage.Tick(0.016f);

            Assert.That(_services.Progress.Progress, Is.EqualTo(OpenFaucetStage.OpenProgress));
            Assert.That(_services.Icon.Completed, Is.False);
            Assert.That(_services.Icon.Active, Is.True);
            Assert.That(_services.CompletionCount, Is.EqualTo(0));
            Assert.That(_services.Hands.IsDraggable, Is.True);

            stage.Tick(0.016f);
            Assert.That(_services.CompletionCount, Is.EqualTo(0));
        }

        [Test]
        public void OpenFaucet_LocksAtQuarter_OnPointerHit()
        {
            var stage = new OpenFaucetStage();
            _services.CompletionRequested = _ => { };
            stage.Initialize(_services);
            stage.Enter();

            _services.Faucet.RaisePointerHit();
            Assert.That(_services.Progress.Progress, Is.EqualTo(OpenFaucetStage.OpenProgress));
            Assert.That(_services.Icon.Completed, Is.False);
            Assert.That(_services.CompletionCount, Is.EqualTo(0));
            Assert.That(_services.Faucet.IsEnabled, Is.False);
            Assert.That(_services.Faucet.IsOpen, Is.True);
            Assert.That(_services.Faucet.LeftIsOpen, Is.True);
            Assert.That(_services.Faucet.RightIsOpen, Is.False);

            stage.Exit();
            _services.Faucet.RaisePointerHit();
            Assert.That(_services.CompletionCount, Is.EqualTo(0));
        }

        [Test]
        public void OpenFaucet_PointerHitRight_LocksRightOnly()
        {
            var stage = new OpenFaucetStage();
            _services.CompletionRequested = _ => { };
            stage.Initialize(_services);
            stage.Enter();

            _services.Faucet.RaisePointerHit(FaucetSide.Right);
            Assert.That(_services.Progress.Progress, Is.EqualTo(OpenFaucetStage.OpenProgress));
            Assert.That(_services.Faucet.IsEnabled, Is.False);
            Assert.That(_services.Faucet.LeftIsOpen, Is.False);
            Assert.That(_services.Faucet.RightIsOpen, Is.True);
            Assert.That(_services.CompletionCount, Is.EqualTo(0));
        }

        [Test]
        public void OpenFaucet_LockOnce_IgnoresSecondActivation()
        {
            var stage = new OpenFaucetStage();
            _services.CompletionRequested = _ => { };
            stage.Initialize(_services);
            stage.Enter();

            _services.Faucet.Raise(FaucetSide.Left);
            _services.Faucet.Raise(FaucetSide.Right);

            Assert.That(_services.Progress.Progress, Is.EqualTo(OpenFaucetStage.OpenProgress));
            Assert.That(_services.CompletionCount, Is.EqualTo(0));
        }

        [Test]
        public void OpenFaucet_DoesNotFill_UntilHandsDragStarted()
        {
            var stage = new OpenFaucetStage();
            _services.CompletionRequested = _ => { };
            stage.Initialize(_services);
            stage.Enter();
            _services.Faucet.RaisePointerHit();

            _services.Water.IsOverlapping = true;
            stage.Tick(OpenFaucetStage.FillInterval);
            Assert.That(_services.Progress.Progress, Is.EqualTo(OpenFaucetStage.OpenProgress));

            _services.Hands.RaiseDragStarted();
            stage.Tick(OpenFaucetStage.FillInterval);
            Assert.That(_services.Progress.Progress, Is.EqualTo(OpenFaucetStage.OpenProgress + OpenFaucetStage.FillStep).Within(0.0001f));
        }

        [Test]
        public void OpenFaucet_FillsWhileOverlapping_ThenCompletes()
        {
            var stage = new OpenFaucetStage();
            _services.CompletionRequested = _ => { };
            stage.Initialize(_services);
            stage.Enter();
            _services.Faucet.RaisePointerHit();
            _services.Hands.RaiseDragStarted();

            _services.Water.IsOverlapping = true;
            stage.Tick(OpenFaucetStage.FillInterval);
            Assert.That(_services.Progress.Progress, Is.EqualTo(OpenFaucetStage.OpenProgress + OpenFaucetStage.FillStep).Within(0.0001f));
            Assert.That(_services.CompletionCount, Is.EqualTo(0));

            _services.Water.IsOverlapping = false;
            stage.Tick(OpenFaucetStage.FillInterval);
            Assert.That(_services.Progress.Progress, Is.EqualTo(OpenFaucetStage.OpenProgress + OpenFaucetStage.FillStep).Within(0.0001f));

            _services.Water.IsOverlapping = true;
            stage.Tick(3.8f);
            Assert.That(_services.Progress.Progress, Is.EqualTo(1f));
            Assert.That(_services.Icon.Active, Is.False);
            Assert.That(_services.Icon.Completed, Is.True);
            Assert.That(_services.CompletionCount, Is.EqualTo(1));

            stage.Tick(OpenFaucetStage.FillInterval);
            Assert.That(_services.CompletionCount, Is.EqualTo(1));
        }

        [Test]
        public void ApplySoap_OnEnter_GlowsAndEnablesDrag()
        {
            var stage = new ApplySoapStage();
            stage.Initialize(_services);
            _services.Hands.SetDraggable(true);
            stage.Enter();

            Assert.That(_services.Icon.StepId, Is.EqualTo(2));
            Assert.That(_services.Icon.Active, Is.True);
            Assert.That(_services.Icon.Completed, Is.False);
            Assert.That(_services.Progress.Progress, Is.EqualTo(0f));
            Assert.That(_services.Soap.IsDraggable, Is.True);
            Assert.That(_services.Soap.IsGlowing, Is.True);
            Assert.That(_services.Hands.IsDraggable, Is.False);
            Assert.That(_services.Hands.IsGlowing, Is.False);
        }

        [Test]
        public void ApplySoap_DragStarted_StopsGlow()
        {
            var stage = new ApplySoapStage();
            stage.Initialize(_services);
            stage.Enter();

            Assert.That(_services.Soap.IsGlowing, Is.True);
            _services.Soap.RaiseDragStarted();
            Assert.That(_services.Soap.IsGlowing, Is.False);
            Assert.That(_services.Soap.IsDraggable, Is.True);
        }

        [Test]
        public void ApplySoap_DoesNotFill_UntilDragStarted()
        {
            var stage = new ApplySoapStage();
            _services.CompletionRequested = _ => { };
            stage.Initialize(_services);
            stage.Enter();

            _services.Soap.IsOverlapping = true;
            stage.Tick(ApplySoapStage.FillInterval);
            Assert.That(_services.Progress.Progress, Is.EqualTo(0f));

            _services.Soap.RaiseDragStarted();
            stage.Tick(ApplySoapStage.FillInterval);
            Assert.That(_services.Progress.Progress, Is.EqualTo(ApplySoapStage.FillStep).Within(0.0001f));
        }

        [Test]
        public void ApplySoap_FillsWhileOverlapping_ThenCompletes()
        {
            var stage = new ApplySoapStage();
            _services.CompletionRequested = _ => { };
            stage.Initialize(_services);
            stage.Enter();
            _services.Soap.RaiseDragStarted();

            _services.Soap.IsOverlapping = true;
            stage.Tick(ApplySoapStage.FillInterval);
            Assert.That(_services.Progress.Progress, Is.EqualTo(ApplySoapStage.FillStep).Within(0.0001f));
            Assert.That(_services.CompletionCount, Is.EqualTo(0));

            _services.Soap.IsOverlapping = false;
            stage.Tick(ApplySoapStage.FillInterval);
            Assert.That(_services.Progress.Progress, Is.EqualTo(ApplySoapStage.FillStep).Within(0.0001f));

            _services.Soap.IsOverlapping = true;
            stage.Tick(5f);
            Assert.That(_services.Progress.Progress, Is.EqualTo(1f));
            Assert.That(_services.Icon.Active, Is.False);
            Assert.That(_services.Icon.Completed, Is.True);
            Assert.That(_services.CompletionCount, Is.EqualTo(1));
            Assert.That(_services.Soap.IsDraggable, Is.False);
            Assert.That(_services.Soap.IsGlowing, Is.False);
            Assert.That(_services.Soap.ReturnHomeCount, Is.EqualTo(1));

            stage.Tick(ApplySoapStage.FillInterval);
            Assert.That(_services.CompletionCount, Is.EqualTo(1));
        }

        [Test]
        public void ApplySoap_OnExit_ReturnsHomeAndDisables()
        {
            var stage = new ApplySoapStage();
            stage.Initialize(_services);
            stage.Enter();
            stage.Exit();

            Assert.That(_services.Soap.IsDraggable, Is.False);
            Assert.That(_services.Soap.IsGlowing, Is.False);
            Assert.That(_services.Soap.ReturnHomeCount, Is.EqualTo(1));
            _services.Soap.RaiseDragStarted();
            Assert.That(_services.CompletionCount, Is.Zero);
        }

        [Test]
        public void ApplySoap_CoverageTracksFill_ScrubbingOnlyWhileOverlapping()
        {
            var stage = new ApplySoapStage();
            _services.CompletionRequested = _ => { };
            stage.Initialize(_services);
            stage.Enter();

            Assert.That(_services.Foam.Coverage, Is.EqualTo(0f));
            Assert.That(_services.Foam.IsScrubbing, Is.False);

            _services.Soap.IsOverlapping = true;
            stage.Tick(ApplySoapStage.FillInterval);
            Assert.That(_services.Foam.Coverage, Is.EqualTo(0f));
            Assert.That(_services.Foam.IsScrubbing, Is.False);

            _services.Soap.RaiseDragStarted();
            stage.Tick(ApplySoapStage.FillInterval);
            Assert.That(_services.Progress.Progress, Is.EqualTo(ApplySoapStage.FillStep).Within(0.0001f));
            Assert.That(_services.Foam.Coverage, Is.EqualTo(ApplySoapStage.FillStep).Within(0.0001f));
            Assert.That(_services.Foam.IsScrubbing, Is.True);

            _services.Soap.IsOverlapping = false;
            stage.Tick(ApplySoapStage.FillInterval);
            Assert.That(_services.Foam.Coverage, Is.EqualTo(ApplySoapStage.FillStep).Within(0.0001f));
            Assert.That(_services.Foam.IsScrubbing, Is.False);

            _services.Soap.IsOverlapping = true;
            stage.Tick(5f);
            Assert.That(_services.Foam.Coverage, Is.EqualTo(1f));
            Assert.That(_services.Foam.IsScrubbing, Is.False);
            Assert.That(_services.CompletionCount, Is.EqualTo(1));
        }

        [Test]
        public void ApplySoap_OnExit_KeepsCoverage_StopsScrubbing()
        {
            var stage = new ApplySoapStage();
            _services.CompletionRequested = _ => { };
            stage.Initialize(_services);
            stage.Enter();
            _services.Soap.RaiseDragStarted();
            _services.Soap.IsOverlapping = true;
            stage.Tick(ApplySoapStage.FillInterval);
            Assert.That(_services.Foam.Coverage, Is.EqualTo(ApplySoapStage.FillStep).Within(0.0001f));
            Assert.That(_services.Foam.IsScrubbing, Is.True);

            stage.Exit();
            Assert.That(_services.Foam.Coverage, Is.EqualTo(ApplySoapStage.FillStep).Within(0.0001f));
            Assert.That(_services.Foam.IsScrubbing, Is.False);
        }

        [Test]
        public void Intro_ResetsFoamCoverage()
        {
            _flow.stageConfigurations = new List<GameStage> { new ApplySoapStage() };
            _flow.StartSession();
            Assert.That(_services.Foam.ResetCount, Is.GreaterThanOrEqualTo(1));

            _services.Foam.SetCoverage(0.4f);
            _services.Foam.SetScrubbing(true);
            _flow.StartSession();
            Assert.That(_services.Foam.Coverage, Is.EqualTo(0f));
            Assert.That(_services.Foam.IsScrubbing, Is.False);
        }

        [Test]
        public void ApplySoap_FollowsOpenFaucet_InConfiguredOrder()
        {
            _flow.stageConfigurations = new List<GameStage>
            {
                new OpenFaucetStage(),
                new ApplySoapStage()
            };
            _services.CompletionRequested = stage => _flow.RequestStageCompletion(stage);

            _flow.StartSession();
            _flow.DismissIntro();
            Assert.That(_flow.State, Is.EqualTo(GameFlowState.Stage));
            Assert.That(_flow.ActiveStage, Is.InstanceOf<OpenFaucetStage>());

            _flow.RequestStageCompletion(_flow.ActiveStage);

            Assert.That(_flow.State, Is.EqualTo(GameFlowState.Stage));
            Assert.That(_flow.ActiveStage, Is.InstanceOf<ApplySoapStage>());
            Assert.That(_flow.ActiveStage.IsEntered, Is.True);
        }

        [Test]
        public void RinseSoap_OnEnter_GlowsAndEnablesHandsDrag()
        {
            var stage = new RinseSoapStage();
            stage.Initialize(_services);
            _services.Soap.SetDraggable(true);
            _services.Faucet.SetEnabled(true);
            stage.Enter();

            Assert.That(_services.Icon.StepId, Is.EqualTo(3));
            Assert.That(_services.Icon.Active, Is.True);
            Assert.That(_services.Icon.Completed, Is.False);
            Assert.That(_services.Progress.Progress, Is.EqualTo(0f));
            Assert.That(_services.Hands.IsDraggable, Is.True);
            Assert.That(_services.Hands.IsGlowing, Is.True);
            Assert.That(_services.Soap.IsDraggable, Is.False);
            Assert.That(_services.Soap.IsGlowing, Is.False);
            Assert.That(_services.Faucet.IsEnabled, Is.False);
            Assert.That(_services.Faucet.IsGlowing, Is.False);
        }

        [Test]
        public void RinseSoap_DragStarted_StopsGlow()
        {
            var stage = new RinseSoapStage();
            stage.Initialize(_services);
            stage.Enter();

            Assert.That(_services.Hands.IsGlowing, Is.True);
            _services.Hands.RaiseDragStarted();
            Assert.That(_services.Hands.IsGlowing, Is.False);
            Assert.That(_services.Hands.IsDraggable, Is.True);
        }

        [Test]
        public void RinseSoap_DoesNotFill_UntilHandsDragStarted()
        {
            var stage = new RinseSoapStage();
            _services.CompletionRequested = _ => { };
            stage.Initialize(_services);
            stage.Enter();

            _services.Water.IsOverlapping = true;
            stage.Tick(RinseSoapStage.FillInterval);
            Assert.That(_services.Progress.Progress, Is.EqualTo(0f));

            _services.Hands.RaiseDragStarted();
            stage.Tick(RinseSoapStage.FillInterval);
            Assert.That(_services.Progress.Progress, Is.EqualTo(RinseSoapStage.FillStep).Within(0.0001f));
        }

        [Test]
        public void RinseSoap_FillsWhileOverlapping_StopsAt75AndEnablesFaucet()
        {
            var stage = new RinseSoapStage();
            _services.CompletionRequested = _ => { };
            stage.Initialize(_services);
            stage.Enter();
            _services.Hands.RaiseDragStarted();

            _services.Water.IsOverlapping = true;
            stage.Tick(RinseSoapStage.FillInterval);
            Assert.That(_services.Progress.Progress, Is.EqualTo(RinseSoapStage.FillStep).Within(0.0001f));
            Assert.That(_services.CompletionCount, Is.EqualTo(0));

            _services.Water.IsOverlapping = false;
            stage.Tick(RinseSoapStage.FillInterval);
            Assert.That(_services.Progress.Progress, Is.EqualTo(RinseSoapStage.FillStep).Within(0.0001f));

            _services.Water.IsOverlapping = true;
            stage.Tick(3.8f);
            Assert.That(_services.Progress.Progress, Is.EqualTo(RinseSoapStage.RinseProgress));
            Assert.That(_services.CompletionCount, Is.EqualTo(0));
            Assert.That(_services.Hands.IsDraggable, Is.False);
            Assert.That(_services.Hands.IsGlowing, Is.False);
            Assert.That(_services.Hands.ReturnHomeCount, Is.EqualTo(1));
            Assert.That(_services.Faucet.IsEnabled, Is.True);
            Assert.That(_services.Faucet.IsGlowing, Is.True);
            Assert.That(_services.Icon.Active, Is.True);
            Assert.That(_services.Icon.Completed, Is.False);
            Assert.That(_services.Foam.Coverage, Is.EqualTo(0f));

            stage.Tick(RinseSoapStage.FillInterval);
            Assert.That(_services.Progress.Progress, Is.EqualTo(RinseSoapStage.RinseProgress));
            Assert.That(_services.CompletionCount, Is.EqualTo(0));
        }

        [Test]
        public void RinseSoap_OnEnter_KeepsFoamCoverage()
        {
            var stage = new RinseSoapStage();
            stage.Initialize(_services);
            _services.Foam.SetCoverage(1f);
            stage.Enter();
            Assert.That(_services.Foam.Coverage, Is.EqualTo(1f));
        }

        [Test]
        public void RinseSoap_CoverageFallsAsRinseFills()
        {
            var stage = new RinseSoapStage();
            _services.CompletionRequested = _ => { };
            stage.Initialize(_services);
            _services.Foam.SetCoverage(1f);
            stage.Enter();
            _services.Hands.RaiseDragStarted();
            _services.Water.IsOverlapping = true;

            stage.Tick(RinseSoapStage.FillInterval);
            Assert.That(
                _services.Foam.Coverage,
                Is.EqualTo(RinseSoapStage.FoamCoverageForProgress(RinseSoapStage.FillStep)).Within(0.0001f));
            Assert.That(_services.Foam.Coverage, Is.LessThan(1f));

            _services.Water.IsOverlapping = false;
            stage.Tick(RinseSoapStage.FillInterval);
            Assert.That(
                _services.Foam.Coverage,
                Is.EqualTo(RinseSoapStage.FoamCoverageForProgress(RinseSoapStage.FillStep)).Within(0.0001f));

            _services.Water.IsOverlapping = true;
            stage.Tick(3.8f);
            Assert.That(_services.Foam.Coverage, Is.EqualTo(0f));
        }

        [Test]
        public void RinseSoap_PointerHit_EitherSide_ClosesAndCompletes()
        {
            var stage = new RinseSoapStage();
            _services.CompletionRequested = _ => { };
            stage.Initialize(_services);
            stage.Enter();
            _services.Hands.RaiseDragStarted();
            _services.Water.IsOverlapping = true;
            stage.Tick(3.8f);

            _services.Faucet.SetOpen(FaucetSide.Left, true);
            _services.Faucet.RaisePointerHit(FaucetSide.Right);

            Assert.That(_services.Faucet.LockClosedCount, Is.EqualTo(1));
            Assert.That(_services.Faucet.IsOpen, Is.False);
            Assert.That(_services.Faucet.IsEnabled, Is.False);
            Assert.That(_services.Faucet.IsGlowing, Is.False);
            Assert.That(_services.Progress.Progress, Is.EqualTo(1f));
            Assert.That(_services.Icon.Active, Is.False);
            Assert.That(_services.Icon.Completed, Is.True);
            Assert.That(_services.CompletionCount, Is.EqualTo(1));

            _services.Faucet.SetEnabled(true);
            _services.Faucet.RaisePointerHit(FaucetSide.Left);
            Assert.That(_services.CompletionCount, Is.EqualTo(1));
            Assert.That(_services.Faucet.LockClosedCount, Is.EqualTo(1));
        }

        [Test]
        public void RinseSoap_OnExit_CleansUp()
        {
            var stage = new RinseSoapStage();
            stage.Initialize(_services);
            stage.Enter();
            stage.Exit();

            Assert.That(_services.Hands.IsDraggable, Is.False);
            Assert.That(_services.Hands.IsGlowing, Is.False);
            Assert.That(_services.Hands.ReturnHomeCount, Is.EqualTo(1));
            Assert.That(_services.Faucet.IsEnabled, Is.False);
            Assert.That(_services.Faucet.IsGlowing, Is.False);
            _services.Hands.RaiseDragStarted();
            Assert.That(_services.CompletionCount, Is.Zero);
        }

        [Test]
        public void RinseSoap_FollowsApplySoap_InConfiguredOrder()
        {
            _flow.stageConfigurations = new List<GameStage>
            {
                new OpenFaucetStage(),
                new ApplySoapStage(),
                new RinseSoapStage()
            };
            _services.CompletionRequested = stage => _flow.RequestStageCompletion(stage);

            _flow.StartSession();
            _flow.DismissIntro();
            Assert.That(_flow.ActiveStage, Is.InstanceOf<OpenFaucetStage>());

            _flow.RequestStageCompletion(_flow.ActiveStage);
            Assert.That(_flow.ActiveStage, Is.InstanceOf<ApplySoapStage>());

            _flow.RequestStageCompletion(_flow.ActiveStage);
            Assert.That(_flow.State, Is.EqualTo(GameFlowState.Stage));
            Assert.That(_flow.ActiveStage, Is.InstanceOf<RinseSoapStage>());
            Assert.That(_flow.ActiveStage.IsEntered, Is.True);
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
        public void StepIcon_LatchesPreviousComplete_WhenNextStageActivates()
        {
            var binderObject = new GameObject("M07b Hud Binder Latch");
            var binder = binderObject.AddComponent<ManosLimpias.UI.RiveHudBinder>();

            binder.SetState(1, active: true, completed: false);
            Assert.That(binder.IsStepCompleted(1), Is.False);

            binder.SetState(1, active: false, completed: true);
            Assert.That(binder.IsStepCompleted(1), Is.True);

            binder.SetState(2, active: true, completed: false);
            Assert.That(binder.IsStepCompleted(1), Is.True);
            Assert.That(binder.IsStepCompleted(2), Is.False);
            Assert.That(binder.StepId, Is.EqualTo(2));
            Assert.That(binder.StepActive, Is.True);
            Assert.That(binder.StepCompleted, Is.False);

            binder.SetState(1, active: false, completed: false);
            Assert.That(binder.IsStepCompleted(1), Is.False);
            Assert.That(binder.IsStepCompleted(2), Is.False);

            UnityEngine.Object.DestroyImmediate(binderObject);
        }

        [Test]
        public void StepIcon_IsLatchedComplete_CoversCurrentAndPriorIds()
        {
            Assert.That(ManosLimpias.UI.Rive.StepIcon.IsLatchedComplete(1, 2, false), Is.True);
            Assert.That(ManosLimpias.UI.Rive.StepIcon.IsLatchedComplete(2, 2, false), Is.False);
            Assert.That(ManosLimpias.UI.Rive.StepIcon.IsLatchedComplete(2, 2, true), Is.True);
            Assert.That(ManosLimpias.UI.Rive.StepIcon.IsLatchedComplete(3, 2, true), Is.False);
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
            public readonly FakeHands Hands = new();
            public readonly FakeWater Water = new();
            public readonly FakeSoap Soap = new();
            public readonly FakeFoam Foam = new();
            public readonly FakeProgress Progress = new();
            public readonly FakeIcon Icon = new();
            public Action<GameStage> CompletionRequested;
            public int CompletionCount { get; private set; }

            IFaucetControl IGameFlowServices.Faucet => Faucet;
            IHandsControl IGameFlowServices.Hands => Hands;
            IWaterContactControl IGameFlowServices.WaterContact => Water;
            ISoapControl IGameFlowServices.Soap => Soap;
            ISoapFoamControl IGameFlowServices.SoapFoam => Foam;
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
            public event Action<FaucetSide> PointerHit;
            public bool IsEnabled { get; private set; }
            public bool IsGlowing { get; private set; }
            public bool LeftIsOpen { get; private set; }
            public bool RightIsOpen { get; private set; }
            public bool IsOpen => LeftIsOpen || RightIsOpen;
            public int LockClosedCount { get; private set; }

            public void SetEnabled(bool enabled)
            {
                IsEnabled = enabled;
                if (!enabled)
                {
                    LeftIsOpen = false;
                    RightIsOpen = false;
                }
            }

            public void SetGlow(bool on)
            {
                IsGlowing = on;
            }

            public void LockOpen(FaucetSide side)
            {
                if (side == FaucetSide.Left)
                    LeftIsOpen = true;
                else
                    RightIsOpen = true;
                IsEnabled = false;
            }

            public void LockClosed()
            {
                LeftIsOpen = false;
                RightIsOpen = false;
                IsEnabled = false;
                LockClosedCount++;
            }

            public void SetOpen(FaucetSide side, bool open)
            {
                if (side == FaucetSide.Left)
                    LeftIsOpen = open;
                else
                    RightIsOpen = open;
            }

            public void Raise(FaucetSide side)
            {
                if (!IsEnabled) return;
                SetOpen(side, true);
                Activated?.Invoke(side);
            }

            public void RaisePointerHit(FaucetSide side = FaucetSide.Left)
            {
                if (IsEnabled)
                    PointerHit?.Invoke(side);
            }
        }

        sealed class FakeHands : IHandsControl
        {
            public event Action DragStarted;
            public bool IsDraggable { get; private set; }
            public bool IsGlowing { get; private set; }
            public int ReturnHomeCount { get; private set; }

            public void SetDraggable(bool draggable)
            {
                IsDraggable = draggable;
            }

            public void SetGlow(bool on)
            {
                IsGlowing = on;
            }

            public void ReturnHome()
            {
                ReturnHomeCount++;
            }

            public void RaiseDragStarted()
            {
                DragStarted?.Invoke();
            }
        }

        sealed class FakeSoap : ISoapControl
        {
            public event Action DragStarted;
            public bool IsDraggable { get; private set; }
            public bool IsGlowing { get; private set; }
            public bool IsOverlapping { get; set; }
            public int ReturnHomeCount { get; private set; }

            public void SetDraggable(bool draggable)
            {
                IsDraggable = draggable;
            }

            public void SetGlow(bool on)
            {
                IsGlowing = on;
            }

            public void ReturnHome()
            {
                ReturnHomeCount++;
            }

            public void RaiseDragStarted()
            {
                DragStarted?.Invoke();
            }
        }

        sealed class FakeFoam : ISoapFoamControl
        {
            public float Coverage { get; private set; }
            public bool IsScrubbing { get; private set; }
            public int ResetCount { get; private set; }

            public void SetCoverage(float progress01)
            {
                Coverage = Mathf.Clamp01(progress01);
            }

            public void SetScrubbing(bool scrubbing)
            {
                IsScrubbing = scrubbing;
            }

            public void ResetFoam()
            {
                Coverage = 0f;
                IsScrubbing = false;
                ResetCount++;
            }
        }

        sealed class FakeWater : IWaterContactControl
        {
            public bool IsOverlapping { get; set; }
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
