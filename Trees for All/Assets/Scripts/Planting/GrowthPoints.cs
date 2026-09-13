using UnityEngine;

namespace Sogeti.Planting
{
    /// <summary>
    /// Turns time spent in a stage into a score award.
    /// Slow care is worth fewer points, but a grace window keeps the first
    /// visit fair for a novice player who is still learning the controls.
    /// Static and stateless, so it is not global state.
    /// </summary>
    public static class GrowthPoints
    {
        /// <summary>Award for leaving <paramref name="completedStage"/> after the given time.</summary>
        public static int Award(GrowthStage completedStage, float secondsInStage)
        {
            if (completedStage == null)
            {
                return 0;
            }

            return Award(
                completedStage.PointsForNextStage,
                secondsInStage,
                completedStage.PointsGraceSeconds,
                completedStage.PointsDecaySeconds,
                completedStage.MinimumPointsFraction);
        }

        /// <summary>
        /// Full points up to the grace window, then linear down to
        /// basePoints * minimumFraction over the decay window, then flat.
        /// </summary>
        public static int Award(
            int basePoints,
            float secondsInStage,
            float graceSeconds,
            float decaySeconds,
            float minimumFraction)
        {
            if (basePoints <= 0)
            {
                return 0;
            }

            minimumFraction = Mathf.Clamp01(minimumFraction);
            graceSeconds = Mathf.Max(graceSeconds, 0f);
            decaySeconds = Mathf.Max(decaySeconds, 0f);

            float overGrace = secondsInStage - graceSeconds;
            if (float.IsNaN(overGrace) || overGrace <= 0f)
            {
                return basePoints;
            }

            // A zero-length ramp means the award drops to the floor the moment grace ends.
            float decayProgress = decaySeconds > 0f ? Mathf.Clamp01(overGrace / decaySeconds) : 1f;
            float fraction = Mathf.Lerp(1f, minimumFraction, decayProgress);

            return Mathf.Max(Mathf.RoundToInt(basePoints * fraction), 0);
        }
    }
}
