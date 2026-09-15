using UnityEngine;

namespace Sogeti.Game
{
    /// <summary>
    /// The round clock as text.
    /// Static and stateless, the GrowthPoints pattern, so the format carries tests
    /// without a scene.
    /// </summary>
    public static class CountdownDisplay
    {
        /// <summary>
        /// Formats seconds as M:SS.
        /// It rounds up, so the clock shows 0:01 until the round is truly over. A
        /// clock that reads 0:00 while the player can still plant looks broken.
        /// The caller must only ask once per whole second. String formatting
        /// allocates, and this feeds a TMP field on mobile hardware.
        /// </summary>
        public static string Format(float seconds)
        {
            int total = SafeWholeSeconds(seconds);
            return $"{total / 60}:{total % 60:00}";
        }

        private static int SafeWholeSeconds(float seconds)
        {
            if (float.IsNaN(seconds) || seconds <= 0f)
            {
                return 0;
            }

            // Infinity would overflow CeilToInt into a negative int.
            return float.IsInfinity(seconds) ? int.MaxValue : Mathf.CeilToInt(seconds);
        }
    }
}
