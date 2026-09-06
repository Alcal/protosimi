using System;
using ManosLimpias.Core;
using ManosLimpias.UI.Rive;
using NUnit.Framework;

namespace ManosLimpias.Tests
{
    public class SoapFoamMathTests
    {
        [Test]
        public void CreateThresholds_AreInOpenUnitInterval()
        {
            var thresholds = SoapFoamMath.CreateThresholds(16, new Random(7));
            Assert.That(thresholds.Length, Is.EqualTo(16));
            for (int i = 0; i < thresholds.Length; i++)
            {
                Assert.That(thresholds[i], Is.GreaterThan(0f));
                Assert.That(thresholds[i], Is.LessThanOrEqualTo(1f));
            }
        }

        [Test]
        public void ShouldWake_WhenCoverageReachesThreshold()
        {
            Assert.That(SoapFoamMath.ShouldWake(0.25f, 0.24f), Is.False);
            Assert.That(SoapFoamMath.ShouldWake(0.25f, 0.25f), Is.True);
            Assert.That(SoapFoamMath.ShouldWake(1f, 1f), Is.True);
        }

        [Test]
        public void GrowScale_LerpsZeroToOne()
        {
            Assert.That(SoapFoamMath.GrowScale(0f, 0.5f), Is.EqualTo(0f));
            Assert.That(SoapFoamMath.GrowScale(0.25f, 0.5f), Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(SoapFoamMath.GrowScale(0.5f, 0.5f), Is.EqualTo(1f));
            Assert.That(SoapFoamMath.GrowScale(2f, 0.5f), Is.EqualTo(1f));
            Assert.That(SoapFoamMath.GrowScale(1f, 0f), Is.EqualTo(1f));
        }

        [Test]
        public void ShrinkScale_IsInverseOfGrow()
        {
            Assert.That(SoapFoamMath.ShrinkScale(0f, 0.5f), Is.EqualTo(1f));
            Assert.That(SoapFoamMath.ShrinkScale(0.25f, 0.5f), Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(SoapFoamMath.ShrinkScale(0.5f, 0.5f), Is.EqualTo(0f));
            Assert.That(SoapFoamMath.ShrinkScale(2f, 0.5f), Is.EqualTo(0f));
            Assert.That(SoapFoamMath.ShrinkScale(1f, 0f), Is.EqualTo(0f));
        }

        [Test]
        public void ShrinkElapsedFromScale_ResumesFromCurrentSize()
        {
            Assert.That(SoapFoamMath.ShrinkElapsedFromScale(1f, 0.5f), Is.EqualTo(0f));
            Assert.That(SoapFoamMath.ShrinkElapsedFromScale(0.5f, 0.5f), Is.EqualTo(0.25f).Within(0.0001f));
            Assert.That(SoapFoamMath.ShrinkElapsedFromScale(0f, 0.5f), Is.EqualTo(0.5f));
            Assert.That(
                SoapFoamMath.ShrinkScale(SoapFoamMath.ShrinkElapsedFromScale(0.25f, 0.5f), 0.5f),
                Is.EqualTo(0.25f).Within(0.0001f));
        }

        [Test]
        public void RandomSpeed_StaysInAuthoredRange()
        {
            var rng = new Random(3);
            for (int i = 0; i < 32; i++)
            {
                float speed = SoapFoamMath.RandomSpeed(rng);
                Assert.That(speed, Is.GreaterThanOrEqualTo(SoapFoamMath.SpeedMin));
                Assert.That(speed, Is.LessThanOrEqualTo(SoapFoamMath.SpeedMax));
            }
        }

        [Test]
        public void FoamArtboard_UsesClusterWhenItHasAStateMachine()
        {
            Assert.That(SoapFoamMath.FoamArtboard(true), Is.EqualTo(GameBubbles.Artboard));
        }

        [Test]
        public void FoamArtboard_FallsBackToSingularWhenClusterHasNoStateMachine()
        {
            Assert.That(SoapFoamMath.FoamArtboard(false), Is.EqualTo(GameBubble.Artboard));
        }

        [Test]
        public void RinseFoamCoverage_IsOneMinusNormalizedProgress()
        {
            Assert.That(RinseSoapStage.FoamCoverageForProgress(0f), Is.EqualTo(1f));
            Assert.That(
                RinseSoapStage.FoamCoverageForProgress(RinseSoapStage.RinseProgress * 0.5f),
                Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(RinseSoapStage.FoamCoverageForProgress(RinseSoapStage.RinseProgress), Is.EqualTo(0f));
            Assert.That(RinseSoapStage.FoamCoverageForProgress(1f), Is.EqualTo(0f));
        }
    }
}
