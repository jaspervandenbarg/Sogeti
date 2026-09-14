using System.Collections.Generic;
using UnityEngine;

namespace Sogeti.Planting
{
    /// <summary>
    /// A seed type as data. A new seed type needs a new asset, never new code.
    /// </summary>
    [CreateAssetMenu(fileName = "SeedDefinition", menuName = "Trees for All/Seed Definition")]
    public class SeedDefinition : ScriptableObject
    {
        [SerializeField]
        private string displayName = "Seed";
        [SerializeField]
        private Sprite menuIcon;
        [SerializeField]
        private GrowthStage[] stages = new GrowthStage[3];
        [SerializeField]
        private GameObject defaultDeathVisualPrefab;
        [SerializeField]
        private int pointsForPlanting = 10;
        [SerializeField]
        private float waterFillPerSecond = 0.25f;
        [SerializeField]
        private float minimumSpacingToSameType = 3f;
        [SerializeField]
        private float minimumSpacingToOtherTypes = 1.5f;

        public string DisplayName => displayName;
        public Sprite MenuIcon => menuIcon;
        public IReadOnlyList<GrowthStage> Stages => stages;
        public int StageCount => stages == null ? 0 : stages.Length;

        /// <summary>Granted at plant time with no decay. The stage awards cover the two transitions.</summary>
        public int PointsForPlanting => pointsForPlanting;

        public float WaterFillPerSecond => waterFillPerSecond;
        public float MinimumSpacingToSameType => minimumSpacingToSameType;
        public float MinimumSpacingToOtherTypes => minimumSpacingToOtherTypes;

        /// <summary>Radius for the single OverlapSphere that answers every spacing question.</summary>
        public float LargestSpacing => Mathf.Max(minimumSpacingToSameType, minimumSpacingToOtherTypes);

        public GrowthStage GetStage(int stageIndex)
        {
            if (stages == null || stageIndex < 0 || stageIndex >= stages.Length)
            {
                return null;
            }

            return stages[stageIndex];
        }

        /// <summary>Stage override first, then the definition default. Dying as grass must not spawn a dry tree.</summary>
        public GameObject GetDeathVisualPrefab(int stageIndex)
        {
            GrowthStage stage = GetStage(stageIndex);
            if (stage != null && stage.DeathVisualPrefab != null)
            {
                return stage.DeathVisualPrefab;
            }

            return defaultDeathVisualPrefab;
        }

        /// <summary>
        /// Spacing between two seed types is the larger of the two demands.
        /// Without this the planting order would change the result.
        /// </summary>
        public float RequiredSpacingTo(SeedDefinition other)
        {
            if (other == null)
            {
                return minimumSpacingToOtherTypes;
            }

            if (other == this)
            {
                return minimumSpacingToSameType;
            }

            return Mathf.Max(minimumSpacingToOtherTypes, other.minimumSpacingToOtherTypes);
        }

        public PlantGrowth CreateGrowth() => new PlantGrowth(stages, waterFillPerSecond);

        private void OnValidate()
        {
            pointsForPlanting = Mathf.Max(pointsForPlanting, 0);
            waterFillPerSecond = Mathf.Max(waterFillPerSecond, 0f);
            minimumSpacingToSameType = Mathf.Max(minimumSpacingToSameType, 0f);
            minimumSpacingToOtherTypes = Mathf.Max(minimumSpacingToOtherTypes, 0f);

            if (stages == null)
            {
                return;
            }

            if (stages.Length != 3)
            {
                Debug.LogWarning($"{name}: the assignment asks for 3 growth stages, this seed has {stages.Length}.", this);
            }

            for (int i = 0; i < stages.Length; i++)
            {
                GrowthStage stage = stages[i];
                if (stage == null)
                {
                    continue;
                }

                stage.ClampValues();

                bool isLastStage = i == stages.Length - 1;
                if (!isLastStage && !stage.CanFill(waterFillPerSecond))
                {
                    Debug.LogWarning(
                        $"{name}: stage {i + 1} can never fill. Watering adds {waterFillPerSecond}/s but it drains {stage.DrainPerSecond}/s.",
                        this);
                }
            }
        }

#if UNITY_EDITOR
        internal static SeedDefinition CreateForTests(
            string displayName,
            GrowthStage[] stages,
            float waterFillPerSecond,
            float minimumSpacingToSameType,
            float minimumSpacingToOtherTypes,
            GameObject defaultDeathVisualPrefab = null)
        {
            SeedDefinition seed = CreateInstance<SeedDefinition>();
            seed.displayName = displayName;
            seed.name = displayName;
            seed.stages = stages;
            seed.waterFillPerSecond = waterFillPerSecond;
            seed.minimumSpacingToSameType = minimumSpacingToSameType;
            seed.minimumSpacingToOtherTypes = minimumSpacingToOtherTypes;
            seed.defaultDeathVisualPrefab = defaultDeathVisualPrefab;
            return seed;
        }

        internal void ValidateNow() => OnValidate();
#endif
    }
}
