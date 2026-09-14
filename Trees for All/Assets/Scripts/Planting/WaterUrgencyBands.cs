using UnityEngine;

namespace Sogeti.Planting
{
    /// <summary>
    /// Maps a water level to an urgency band.
    /// The band exists so the meter swaps its material only when the band changes.
    /// A material change every frame is too expensive on the Quest 3.
    /// </summary>
    public static class WaterUrgencyBands
    {
        // A stage opens at half a meter. Both defaults sit below that, so a fresh plant reads healthy.
        public const float DefaultLowThreshold = 0.4f;
        public const float DefaultCriticalThreshold = 0.2f;

        public static WaterUrgency For(float water01) =>
            For(water01, DefaultLowThreshold, DefaultCriticalThreshold);

        public static WaterUrgency For(float water01, float lowThreshold, float criticalThreshold)
        {
            // A broken number reads as urgent. A silent "healthy" would hide the fault from the player.
            if (float.IsNaN(water01))
            {
                return WaterUrgency.Critical;
            }

            float water = Mathf.Clamp01(water01);
            float critical = Mathf.Clamp01(criticalThreshold);

            // A low band under the critical band would leave no room for Low at all.
            float low = Mathf.Max(Mathf.Clamp01(lowThreshold), critical);

            if (water <= critical)
            {
                return WaterUrgency.Critical;
            }

            return water <= low ? WaterUrgency.Low : WaterUrgency.Healthy;
        }
    }
}
