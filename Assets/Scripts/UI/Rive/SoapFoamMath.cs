using System;

namespace ManosLimpias.UI.Rive
{
    /// <summary>
    /// Pure helpers for on-hand foam wake thresholds, grow, and rinse shrink.
    /// EditMode tests cover this without a live .riv.
    /// </summary>
    public static class SoapFoamMath
    {
        public const float SpeedMin = 0.95f;
        public const float SpeedMax = 1.05f;

        public static float[] CreateThresholds(int count, Random rng)
        {
            var thresholds = new float[Math.Max(0, count)];
            for (int i = 0; i < thresholds.Length; i++)
                thresholds[i] = NextOpenUnit(rng);
            return thresholds;
        }

        /// <summary>Uniform sample in (0, 1].</summary>
        public static float NextOpenUnit(Random rng)
        {
            if (rng == null)
                return 1f;
            return 1f - (float)rng.NextDouble();
        }

        public static bool ShouldWake(float threshold, float coverage)
        {
            return coverage >= threshold;
        }

        public static float GrowScale(float elapsed, float duration)
        {
            if (duration <= 0f)
                return 1f;
            if (elapsed <= 0f)
                return 0f;
            if (elapsed >= duration)
                return 1f;
            return elapsed / duration;
        }

        public static float ShrinkScale(float elapsed, float duration)
        {
            return 1f - GrowScale(elapsed, duration);
        }

        /// <summary>
        /// Elapsed shrink time whose <see cref="ShrinkScale"/> matches <paramref name="scale"/>.
        /// </summary>
        public static float ShrinkElapsedFromScale(float scale, float duration)
        {
            if (duration <= 0f)
                return 0f;
            if (scale >= 1f)
                return 0f;
            if (scale <= 0f)
                return duration;
            return (1f - scale) * duration;
        }

        public static float RandomSpeed(Random rng)
        {
            if (rng == null)
                return 1f;
            return SpeedMin + (float)rng.NextDouble() * (SpeedMax - SpeedMin);
        }

        /// <summary>
        /// <c>game_bubbles</c> in simi_prototype.riv has no state machine, and
        /// RiveWidget.Load fails without one. Use the singular <c>game_bubble</c>
        /// artboard until the cluster ships an SM.
        /// </summary>
        public static string FoamArtboard(bool gameBubblesHasStateMachine)
        {
            return gameBubblesHasStateMachine ? GameBubbles.Artboard : GameBubble.Artboard;
        }
    }
}
