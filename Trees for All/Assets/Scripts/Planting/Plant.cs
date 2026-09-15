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
        // Layer 2 Ignore Raycast sits outside the planting occluder mask, the planting
        // blocker mask and the teleport mask, and the player camera still draws it.
        private const int IgnoreRaycastLayer = 2;

        [SerializeField]
        private Transform visualAnchor;

        [Header("Death")]
        [SerializeField]
        [Tooltip("Tick to keep a dead plant refusing nearby spots. The value applies at death, so restart Play Mode after a change.")]
        private bool blocksPlacementWhenDead;

        [SerializeField]
        [Tooltip("Layer a dead plant moves to when it stops blocking.")]
        private int deadPlantLayer = IgnoreRaycastLayer;

        [Header("Feedback")]
        [SerializeField]
        [Tooltip("Plays once on planting and once on every stage advance.")]
        private AudioSource growAudioSource;

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
            PlayGrowSound();

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
            PlayGrowSound();
            StageAdvanced?.Invoke(this, award);
        }

        private void PlayGrowSound()
        {
            if (growAudioSource != null)
            {
                growAudioSource.Play();
            }
        }

        private void OnDied(int stageIndex)
        {
            ShowVisual(seed.GetDeathVisualPrefab(stageIndex), StageScale(stageIndex));

            if (!blocksPlacementWhenDead)
            {
                MoveToDeadLayer();
            }

            Died?.Invoke(this);
        }

        /// <summary>
        /// Frees the spot a dead plant stands on.
        /// The planting ray stops on any collider inside its occluder mask, so a husk
        /// on the plant layer refuses its own spot before any spacing rule runs. The
        /// teleport mask covers the same layers, so a freed husk also stops blocking
        /// teleport. A dead stage 1 or stage 2 plant is grass or a shrub, so that reads fine.
        /// </summary>
        private void MoveToDeadLayer()
        {
            // The death visual is already a child, and it can carry its own colliders on layer 0.
            SetLayerRecursively(transform, Mathf.Clamp(deadPlantLayer, 0, 31));
        }

        private static void SetLayerRecursively(Transform root, int layer)
        {
            root.gameObject.layer = layer;

            for (int i = 0; i < root.childCount; i++)
            {
                SetLayerRecursively(root.GetChild(i), layer);
            }
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
