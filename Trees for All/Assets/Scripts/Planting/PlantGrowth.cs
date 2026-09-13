using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sogeti.Planting
{
    /// <summary>
    /// Water meter and stage machine for one planted seed.
    /// Pure C# and it takes delta time as an argument, so the whole growth rule
    /// set is verifiable in EditMode tests on a PC with no headset.
    /// </summary>
    public class PlantGrowth
    {
        /// <summary>Every stage except the last opens at half a meter, so the player has a head start.</summary>
        public const float StageStartWater = 0.5f;

        private readonly IReadOnlyList<GrowthStage> stages;
        private readonly float waterFillPerSecond;

        /// <summary>New stage index, and the seconds spent in the stage just left.</summary>
        public event Action<int, float> StageAdvanced;

        /// <summary>The stage index the plant died in.</summary>
        public event Action<int> Died;

        public PlantGrowth(IReadOnlyList<GrowthStage> stages, float waterFillPerSecond)
        {
            if (stages == null)
            {
                throw new ArgumentNullException(nameof(stages));
            }

            if (stages.Count == 0)
            {
                throw new ArgumentException("A plant needs at least one growth stage.", nameof(stages));
            }

            this.stages = stages;
            this.waterFillPerSecond = Mathf.Max(waterFillPerSecond, 0f);

            CurrentStageIndex = 0;
            Water01 = IsFullyGrown ? 0f : StageStartWater;
        }

        public int CurrentStageIndex { get; private set; }

        public GrowthStage CurrentStage => stages[CurrentStageIndex];

        public int StageCount => stages.Count;

        public float Water01 { get; private set; }

        public float SecondsInCurrentStage { get; private set; }

        public float WaterFillPerSecond => waterFillPerSecond;

        public bool IsDead { get; private set; }

        /// <summary>True once the plant reaches the last stage alive. The last stage has no meter.</summary>
        public bool IsFullyGrown => !IsDead && CurrentStageIndex >= stages.Count - 1;

        public bool HasWaterMeter => !IsDead && !IsFullyGrown;

        /// <summary>
        /// Advances the meter by one frame.
        /// <paramref name="waterFlow01"/> is a throttle, not a rate. The plant owns
        /// its fill rate, so a watering can cannot change how fast every seed grows.
        /// </summary>
        public void Tick(float deltaTime, float waterFlow01 = 0f)
        {
            if (!HasWaterMeter)
            {
                return;
            }

            // A dropped frame or a paused editor must not throw inside a VR scene.
            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime <= 0f)
            {
                return;
            }

            SecondsInCurrentStage += deltaTime;

            float flow = float.IsNaN(waterFlow01) ? 0f : Mathf.Clamp01(waterFlow01);
            float netPerSecond = waterFillPerSecond * flow - CurrentStage.DrainPerSecond;
            float water = Water01 + netPerSecond * deltaTime;

            if (water <= 0f)
            {
                Water01 = 0f;
                IsDead = true;
                Died?.Invoke(CurrentStageIndex);
                return;
            }

            if (water >= 1f)
            {
                AdvanceStage();
                return;
            }

            Water01 = water;
        }

        // One advance per tick. A long frame hitch must not skip a whole stage.
        private void AdvanceStage()
        {
            float secondsInPreviousStage = SecondsInCurrentStage;

            CurrentStageIndex++;
            SecondsInCurrentStage = 0f;
            Water01 = IsFullyGrown ? 0f : StageStartWater;

            StageAdvanced?.Invoke(CurrentStageIndex, secondsInPreviousStage);
        }
    }
}
