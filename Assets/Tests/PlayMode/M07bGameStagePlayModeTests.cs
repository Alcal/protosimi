using System.Collections.Generic;
using ManosLimpias.Core;
using NUnit.Framework;
using UnityEngine;

namespace ManosLimpias.Tests
{
    public class M07bGameStagePlayModeTests
    {
        [Test]
        public void AC07_ReplayResetsStageLifecycle()
        {
            var flowObject = new GameObject("M07b PlayMode Flow");
            var flow = flowObject.AddComponent<GameFlowController>();
            flow.stageConfigurations = new List<GameStage>
            {
                new OpenFaucetStage()
            };

            flow.StartSession();
            flow.DismissIntro();
            Assert.That(flow.State, Is.EqualTo(GameFlowState.Stage));
            Assert.That(flow.ActiveStageIndex, Is.EqualTo(0));

            flow.StartSession();

            Assert.That(flow.State, Is.EqualTo(GameFlowState.Intro));
            Assert.That(flow.ActiveStageIndex, Is.EqualTo(-1));
            Assert.That(flow.RuntimeStages.Count, Is.EqualTo(1));
            Assert.That(flow.RuntimeStages[0].IsEntered, Is.False);

            Object.DestroyImmediate(flowObject);
        }

        [Test]
        public void OpenFaucet_ActivateHandle_LocksFaucetAndStaysInStage()
        {
            var flowObject = new GameObject("OpenFaucet PlayMode Flow");
            var faucet = flowObject.AddComponent<ManosLimpias.UI.Rive.Faucet>();
            var flow = flowObject.AddComponent<GameFlowController>();
            flow.faucet = faucet;
            flow.stageConfigurations = new List<GameStage> { new OpenFaucetStage() };

            flow.StartSession();
            flow.DismissIntro();
            Assert.That(flow.State, Is.EqualTo(GameFlowState.Stage));
            Assert.That(faucet.IsEnabled, Is.True);

            faucet.Activate(FaucetSide.Left);

            Assert.That(flow.State, Is.EqualTo(GameFlowState.Stage));
            Assert.That(faucet.IsEnabled, Is.False);

            Object.DestroyImmediate(flowObject);
        }

        [Test]
        public void OpenFaucet_PointerHit_LocksFaucetAndStaysInStage()
        {
            var flowObject = new GameObject("OpenFaucet PointerHit Flow");
            var faucet = flowObject.AddComponent<ManosLimpias.UI.Rive.Faucet>();
            var flow = flowObject.AddComponent<GameFlowController>();
            flow.faucet = faucet;
            flow.stageConfigurations = new List<GameStage> { new OpenFaucetStage() };

            flow.StartSession();
            flow.DismissIntro();
            Assert.That(faucet.IsEnabled, Is.True);

            faucet.NotifyPointerHit();

            Assert.That(flow.State, Is.EqualTo(GameFlowState.Stage));
            Assert.That(faucet.IsEnabled, Is.False);

            Object.DestroyImmediate(flowObject);
        }

        [Test]
        public void ApplySoap_FollowsOpenFaucet_InConfiguredOrder()
        {
            var flowObject = new GameObject("ApplySoap PlayMode Flow");
            var flow = flowObject.AddComponent<GameFlowController>();
            flow.stageConfigurations = new List<GameStage>
            {
                new OpenFaucetStage(),
                new ApplySoapStage()
            };

            flow.StartSession();
            flow.DismissIntro();
            Assert.That(flow.State, Is.EqualTo(GameFlowState.Stage));
            Assert.That(flow.ActiveStage, Is.InstanceOf<OpenFaucetStage>());

            flow.RequestStageCompletion(flow.ActiveStage);

            Assert.That(flow.State, Is.EqualTo(GameFlowState.Stage));
            Assert.That(flow.ActiveStage, Is.InstanceOf<ApplySoapStage>());
            Assert.That(flow.ActiveStage.IsEntered, Is.True);

            Object.DestroyImmediate(flowObject);
        }

        [Test]
        public void RinseSoap_FollowsApplySoap_InConfiguredOrder()
        {
            var flowObject = new GameObject("RinseSoap PlayMode Flow");
            var flow = flowObject.AddComponent<GameFlowController>();
            flow.stageConfigurations = new List<GameStage>
            {
                new OpenFaucetStage(),
                new ApplySoapStage(),
                new RinseSoapStage()
            };

            flow.StartSession();
            flow.DismissIntro();
            Assert.That(flow.State, Is.EqualTo(GameFlowState.Stage));
            Assert.That(flow.ActiveStage, Is.InstanceOf<OpenFaucetStage>());

            flow.RequestStageCompletion(flow.ActiveStage);
            Assert.That(flow.ActiveStage, Is.InstanceOf<ApplySoapStage>());

            flow.RequestStageCompletion(flow.ActiveStage);

            Assert.That(flow.State, Is.EqualTo(GameFlowState.Stage));
            Assert.That(flow.ActiveStage, Is.InstanceOf<RinseSoapStage>());
            Assert.That(flow.ActiveStage.IsEntered, Is.True);

            Object.DestroyImmediate(flowObject);
        }

        [Test]
        public void DryHands_FollowsRinseSoap_InConfiguredOrder()
        {
            var flowObject = new GameObject("DryHands PlayMode Flow");
            var flow = flowObject.AddComponent<GameFlowController>();
            flow.stageConfigurations = new List<GameStage>
            {
                new OpenFaucetStage(),
                new ApplySoapStage(),
                new RinseSoapStage(),
                new DryHandsStage()
            };

            flow.StartSession();
            flow.DismissIntro();
            Assert.That(flow.State, Is.EqualTo(GameFlowState.Stage));
            Assert.That(flow.ActiveStage, Is.InstanceOf<OpenFaucetStage>());

            flow.RequestStageCompletion(flow.ActiveStage);
            Assert.That(flow.ActiveStage, Is.InstanceOf<ApplySoapStage>());

            flow.RequestStageCompletion(flow.ActiveStage);
            Assert.That(flow.ActiveStage, Is.InstanceOf<RinseSoapStage>());

            flow.RequestStageCompletion(flow.ActiveStage);

            Assert.That(flow.State, Is.EqualTo(GameFlowState.Stage));
            Assert.That(flow.ActiveStage, Is.InstanceOf<DryHandsStage>());
            Assert.That(flow.ActiveStage.IsEntered, Is.True);

            Object.DestroyImmediate(flowObject);
        }
    }
}
