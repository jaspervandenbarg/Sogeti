using UnityEngine;

namespace Sogeti.Watering
{
    /// <summary>
    /// The charge inside a watering can.
    /// Capacity is measured in seconds of full pour, so the tuning number in the
    /// inspector reads as "how long does one can last". It takes delta time as an
    /// argument, the rule PlantGrowth follows, so EditMode tests reach it.
    /// </summary>
    public class WaterTank
    {
        // A zero capacity would divide by zero. Clamping keeps a mistyped field harmless.
        private const float MinimumCapacitySeconds = 0.01f;

        private readonly float capacitySeconds;

        public WaterTank(float capacitySeconds, float startFill01 = 1f)
        {
            this.capacitySeconds = Mathf.Max(capacitySeconds, MinimumCapacitySeconds);
            Fill01 = SafeFill(startFill01);
        }

        public float Fill01 { get; private set; }

        public bool IsEmpty => Fill01 <= 0f;

        public bool IsFull => Fill01 >= 1f;

        public float CapacitySeconds => capacitySeconds;

        /// <summary>
        /// Spends water for one frame.
        /// </summary>
        /// <returns>
        /// The throttle the tank could actually serve. A can with a splash left
        /// pours less than the tilt asks for, and an empty can pours nothing.
        /// </returns>
        public float Drain(float flow01, float deltaTime)
        {
            float requested = SafeFlow(flow01);
            if (requested <= 0f || !IsUsableDeltaTime(deltaTime))
            {
                return 0f;
            }

            float cost = requested * deltaTime / capacitySeconds;
            if (cost <= Fill01)
            {
                Fill01 -= cost;
                return requested;
            }

            // The flow that empties the tank exactly over this frame.
            float served = Fill01 * capacitySeconds / deltaTime;
            Fill01 = 0f;
            return Mathf.Min(served, requested);
        }

        /// <summary>Adds water while the spout sits in the pond. The tank never passes full.</summary>
        public void Refill(float fillPerSecond, float deltaTime)
        {
            if (fillPerSecond <= 0f || float.IsNaN(fillPerSecond) || !IsUsableDeltaTime(deltaTime))
            {
                return;
            }

            Fill01 = Mathf.Min(1f, Fill01 + fillPerSecond * deltaTime);
        }

        public void SetFill(float fill01) => Fill01 = SafeFill(fill01);

        private static float SafeFill(float value) => float.IsNaN(value) ? 0f : Mathf.Clamp01(value);

        private static float SafeFlow(float value) => float.IsNaN(value) ? 0f : Mathf.Clamp01(value);

        // A dropped frame or a paused editor must not throw inside a VR scene.
        private static bool IsUsableDeltaTime(float deltaTime)
        {
            return !float.IsNaN(deltaTime) && !float.IsInfinity(deltaTime) && deltaTime > 0f;
        }
    }
}
