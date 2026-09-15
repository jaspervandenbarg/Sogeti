using UnityEngine;

namespace Sogeti.Watering
{
    /// <summary>
    /// Turns the tilt of a watering can into a pour throttle.
    /// Static and pure, so the gesture that drives the whole watering step is
    /// verifiable in EditMode tests on a PC with no headset.
    /// </summary>
    public static class PourFlow
    {
        /// <summary>Below this tilt the can holds its water. A resting hand is never level.</summary>
        public const float DefaultStartDegrees = 45f;

        /// <summary>At this tilt the can pours at full rate.</summary>
        public const float DefaultFullDegrees = 90f;

        /// <summary>
        /// The tilt that pours, in degrees.
        /// Water leaves through the spout only while the mouth sits below the body of
        /// the can, so a can tilted up reads as 0 and holds its water.
        /// </summary>
        /// <param name="canUp">The up axis of the can. It points out of the top.</param>
        /// <param name="spoutHeight">World height of the mouth of the can.</param>
        /// <param name="bodyHeight">World height of the body of the can.</param>
        public static float SpoutTiltDegrees(Vector3 canUp, float spoutHeight, float bodyHeight)
        {
            if (spoutHeight >= bodyHeight)
            {
                return 0f;
            }

            return Vector3.Angle(canUp, Vector3.up);
        }

        /// <summary>
        /// Maps the angle between the can's up axis and world up to a 0 to 1 throttle.
        /// </summary>
        /// <returns>0 while the can is upright, ramping to 1 at the full angle.</returns>
        public static float For(float tiltDegrees, float startDegrees, float fullDegrees)
        {
            // A broken transform must never open the tap.
            if (float.IsNaN(tiltDegrees) || float.IsNaN(startDegrees) || float.IsNaN(fullDegrees))
            {
                return 0f;
            }

            float tilt = Mathf.Clamp(tiltDegrees, 0f, 180f);
            float start = Mathf.Clamp(startDegrees, 0f, 180f);
            float full = Mathf.Clamp(fullDegrees, 0f, 180f);

            if (tilt <= start)
            {
                return 0f;
            }

            // An inspector that puts the full angle at or below the start angle leaves no ramp.
            if (full <= start)
            {
                return 1f;
            }

            return Mathf.Clamp01((tilt - start) / (full - start));
        }
    }
}
