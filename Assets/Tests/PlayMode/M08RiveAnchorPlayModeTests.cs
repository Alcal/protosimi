using ManosLimpias.Core;
using NUnit.Framework;
using UnityEngine;

namespace ManosLimpias.Tests
{
    public class M08RiveAnchorPlayModeTests
    {
        [Test]
        public void AC06_IntroDismiss_StillEntersStage()
        {
            var flowObject = new GameObject("M08 PlayMode Flow");
            var flow = flowObject.AddComponent<GameFlowController>();
            flow.stageConfigurations.Add(new OpenFaucetStage());

            flow.StartSession();
            Assert.That(flow.State, Is.EqualTo(GameFlowState.Intro));

            flow.DismissIntro();

            Assert.That(flow.State, Is.EqualTo(GameFlowState.Stage));
            Assert.That(flow.ActiveStage, Is.InstanceOf<OpenFaucetStage>());
            Assert.That(flow.ActiveStage.IsEntered, Is.True);

            Object.DestroyImmediate(flowObject);
        }
    }
}
