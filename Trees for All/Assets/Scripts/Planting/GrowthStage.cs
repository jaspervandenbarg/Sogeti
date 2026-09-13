using System;
using UnityEngine;

namespace Sogeti.Planting
{
    /// <summary>
    /// One growth step of a seed type.
    /// Serialized inside SeedDefinition, so a new stage needs no new asset.
    /// Every field here reads against the time spent inside this one stage.
    /// </summary>
    [Serializable]
    public class GrowthStage
    {
        [SerializeField]
        private GameObject visualPrefab;
        [SerializeField]
        private GameObject deathVisualPrefab;
        [SerializeField]
        private float visualScale = 1f;
        [SerializeField]
        private float secondsToEmpty = 30f;
        [SerializeField]
        private int pointsForNextStage = 20;
        [SerializeField]
        private float pointsGraceSeconds = 10f;
        [SerializeField]
        private float pointsDecaySeconds = 30f;
        [SerializeField, Range(0f, 1f)]
        private float minimumPointsFraction = 0.25f;

        /// <summary>The child visual of the plant root. The root owns the collider and the layer.</summary>
        public GameObject VisualPrefab => visualPrefab;

        /// <summary>Optional. Overrides the seed definition default when the plant dies in this stage.</summary>
        public GameObject DeathVisualPrefab => deathVisualPrefab;

        /// <summary>Lets one prefab serve two seed types at different sizes.</summary>
        public float VisualScale => visualScale;

        public float SecondsToEmpty => secondsToEmpty;

        /// <summary>The award for leaving this stage and reaching the next one.</summary>
        public int PointsForNextStage => pointsForNextStage;

        public float PointsGraceSeconds => pointsGraceSeconds;
        public float PointsDecaySeconds => pointsDecaySeconds;
        public float MinimumPointsFraction => minimumPointsFraction;

        /// <summary>Meter units lost per second. Zero when the stage has no drain.</summary>
        public float DrainPerSecond => secondsToEmpty > 0f ? 1f / secondsToEmpty : 0f;

        /// <summary>True when watering can never outpace the drain, so the stage is unwinnable.</summary>
        public bool CanFill(float waterFillPerSecond) => waterFillPerSecond > DrainPerSecond;

        // Unity deserialization needs this. Declaring the test constructor removes the implicit one.
        public GrowthStage()
        {
        }

#if UNITY_EDITOR
        internal GrowthStage(
            float secondsToEmpty,
            int pointsForNextStage,
            float pointsGraceSeconds,
            float pointsDecaySeconds,
            float minimumPointsFraction)
        {
            this.secondsToEmpty = secondsToEmpty;
            this.pointsForNextStage = pointsForNextStage;
            this.pointsGraceSeconds = pointsGraceSeconds;
            this.pointsDecaySeconds = pointsDecaySeconds;
            this.minimumPointsFraction = minimumPointsFraction;
        }

        internal void SetVisualsForTests(GameObject visual, GameObject death)
        {
            visualPrefab = visual;
            deathVisualPrefab = death;
        }
#endif

        internal void ClampValues()
        {
            secondsToEmpty = Mathf.Max(secondsToEmpty, 0f);
            pointsForNextStage = Mathf.Max(pointsForNextStage, 0);
            pointsGraceSeconds = Mathf.Max(pointsGraceSeconds, 0f);
            pointsDecaySeconds = Mathf.Max(pointsDecaySeconds, 0f);
            minimumPointsFraction = Mathf.Clamp01(minimumPointsFraction);
            visualScale = Mathf.Max(visualScale, 0.01f);
        }
    }
}
