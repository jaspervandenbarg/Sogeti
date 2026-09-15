using System;
using UnityEngine;

namespace Sogeti.Game
{
    /// <summary>
    /// The round clock.
    /// It takes delta time as an argument, the rule PlantGrowth and WaterTank both
    /// follow, so EditMode tests run it with no scene and no headset.
    /// </summary>
    public class GameCountdown
    {
        // A zero round would expire on the frame it starts. Clamping keeps a mistyped field playable.
        private const float MinimumDurationSeconds = 1f;

        private readonly float durationSeconds;

        /// <summary>Raised once, on the tick that reaches zero.</summary>
        public event Action Expired;

        public GameCountdown(float durationSeconds)
        {
            this.durationSeconds = Mathf.Max(durationSeconds, MinimumDurationSeconds);
            SecondsRemaining = this.durationSeconds;
        }

        public float DurationSeconds => durationSeconds;

        public float SecondsRemaining { get; private set; }

        /// <summary>True only between Start and the tick that empties the clock.</summary>
        public bool IsRunning { get; private set; }

        public bool HasExpired { get; private set; }

        /// <summary>Puts the clock back to full and runs it.</summary>
        public void Start()
        {
            SecondsRemaining = durationSeconds;
            HasExpired = false;
            IsRunning = true;
        }

        /// <summary>Freezes the clock without expiring it. The remaining time survives.</summary>
        public void Stop() => IsRunning = false;

        /// <summary>
        /// Spends one frame.
        /// A hitch longer than the whole round still expires once and never drives
        /// the clock past zero, the rule PlantGrowth.Tick follows for stages.
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (!IsRunning || !IsUsableDeltaTime(deltaTime))
            {
                return;
            }

            SecondsRemaining = Mathf.Max(0f, SecondsRemaining - deltaTime);
            if (SecondsRemaining > 0f)
            {
                return;
            }

            IsRunning = false;
            HasExpired = true;
            Expired?.Invoke();
        }

        // A dropped frame or a paused editor must not throw inside a VR scene.
        private static bool IsUsableDeltaTime(float deltaTime)
        {
            return !float.IsNaN(deltaTime) && !float.IsInfinity(deltaTime) && deltaTime > 0f;
        }
    }
}
