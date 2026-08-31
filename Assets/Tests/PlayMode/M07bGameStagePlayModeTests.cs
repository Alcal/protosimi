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
    }
}
