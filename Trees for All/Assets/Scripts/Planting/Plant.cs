using System;
using UnityEngine;

namespace Sogeti.Planting
{
    /// <summary>
    /// One planted seed in the scene.
    /// The root owns the collider, the layer and the growth state. The stage prefab
    /// is a child visual only, because the Polygon Trees grass prefabs ship with no
    /// collider and could never serve as the root.
    /// </summary>
    [DisallowMultipleComponent]
    public class Plant : MonoBehaviour
    {
        [SerializeField]
        private Transform visualAnchor;

        private SeedDefinition seed;
        private PlantGrowth growth;
        private GameObject currentVisual;
        private float waterFlow01;
        private Quaternion visualHeading = Quaternion.identity;

        /// <summary>The planted plant, and the points it earned on the way in.</summary>
        public event Action<Plant, int> Planted;

        /// <summary>The plant, and the points earned for reaching the new stage.</summary>
        public event Action<Plant, int> StageAdvanced;

        public event Action<Plant> Died;

        public SeedDefinition Seed => seed;
        public PlantGrowth Growth => growth;
        public bool IsDead => growth != null && growth.IsDead;
        public bool IsFullyGrown => growth != null && growth.IsFullyGrown;

        /// <summary>True while the stage still drains, so the water meter UI has something to show.</summary>
        public bool HasWaterMeter => growth != null && growth.HasWaterMeter;

        public float Water01 => growth == null ? 0f : growth.Water01;

        /// <summary>Call once, right after the planter spawns the root.</summary>
        public void Initialize(SeedDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (growth != null)
            {
                throw new InvalidOperationException($"{name} is already planted.");
            }

            seed = definition;
            growth = definition.CreateGrowth();

            // One heading for the life of the plant. Re-rolling it would spin the tree on every stage swap.
            visualHeading = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);

            growth.StageAdvanced += OnStageAdvanced;
            growth.Died += OnDied;

            ShowStageVisual(growth.CurrentStageIndex);
            enabled = growth.HasWaterMeter;

            Planted?.Invoke(this, definition.PointsForPlanting);
        }

        /// <summary>
        /// The watering can calls this every frame it pours on this plant.
        /// The value is a 0 to 1 throttle, not a rate. The seed owns the fill rate,
        /// so one can cannot change how fast every seed type grows.
        /// </summary>
        public void ApplyWaterFlow(float flow01)
        {
            if (float.IsNaN(flow01))
            {
                return;
            }

            waterFlow01 = Mathf.Max(waterFlow01, Mathf.Clamp01(flow01));
        }

        private void Update()
        {
            if (growth == null)
            {
                return;
            }

            growth.Tick(Time.deltaTime, waterFlow01);
            waterFlow01 = 0f;

            // A dead or fully grown plant has no meter left to drain. Quest 3 is mobile hardware.
            if (!growth.HasWaterMeter)
            {
                enabled = false;
            }
        }

        private void OnStageAdvanced(int stageIndex, float secondsInPreviousStage)
        {
            int award = GrowthPoints.Award(seed.GetStage(stageIndex - 1), secondsInPreviousStage);

            ShowStageVisual(stageIndex);
            StageAdvanced?.Invoke(this, award);
        }

        private void OnDied(int stageIndex)
        {
            ShowVisual(seed.GetDeathVisualPrefab(stageIndex), StageScale(stageIndex));
            Died?.Invoke(this);
        }

        private void ShowStageVisual(int stageIndex)
        {
            GrowthStage stage = seed.GetStage(stageIndex);
            ShowVisual(stage?.VisualPrefab, StageScale(stageIndex));
        }

        private float StageScale(int stageIndex)
        {
            GrowthStage stage = seed.GetStage(stageIndex);
            return stage == null ? 1f : stage.VisualScale;
        }

        private void ShowVisual(GameObject prefab, float scale)
        {
            if (currentVisual != null)
            {
                Destroy(currentVisual);
                currentVisual = null;
            }

            if (prefab == null)
            {
                return;
            }

            Transform parent = visualAnchor != null ? visualAnchor : transform;
            currentVisual = Instantiate(prefab, parent);
            currentVisual.transform.localPosition = Vector3.zero;
            currentVisual.transform.localRotation = visualHeading;
            currentVisual.transform.localScale = Vector3.one * scale;
        }

        private void OnDestroy()
        {
            if (growth == null)
            {
                return;
            }

            growth.StageAdvanced -= OnStageAdvanced;
            growth.Died -= OnDied;
        }
    }
}
