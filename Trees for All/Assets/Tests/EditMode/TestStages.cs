using UnityEngine;

namespace Sogeti.Planting.Tests
{
    /// <summary>
    /// Builds growth data for tests. No [Test] members live here.
    /// The defaults match the shipped seed assets: 30 s then 45 s to empty.
    /// </summary>
    internal static class TestStages
    {
        internal const float DefaultFillPerSecond = 0.25f;

        internal static GrowthStage Stage(
            float secondsToEmpty = 30f,
            int points = 20,
            float grace = 10f,
            float decay = 30f,
            float floor = 0.25f)
        {
            return new GrowthStage(secondsToEmpty, points, grace, decay, floor);
        }

        /// <summary>Seed, sprout, tree. The last stage has no meter, so its values are unused.</summary>
        internal static GrowthStage[] ThreeStages()
        {
            return new[]
            {
                Stage(secondsToEmpty: 30f, points: 20, grace: 10f, decay: 30f),
                Stage(secondsToEmpty: 45f, points: 40, grace: 15f, decay: 45f),
                Stage(secondsToEmpty: 0f, points: 0, grace: 0f, decay: 0f)
            };
        }

        internal static PlantGrowth Growth(float fillPerSecond = DefaultFillPerSecond)
        {
            return new PlantGrowth(ThreeStages(), fillPerSecond);
        }

        /// <summary>Runs many small ticks, the way a frame loop would.</summary>
        internal static void TickFor(PlantGrowth growth, float seconds, float flow = 0f, float step = 0.02f)
        {
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                float delta = Mathf.Min(step, seconds - elapsed);
                growth.Tick(delta, flow);
                elapsed += delta;
            }
        }
    }
}
