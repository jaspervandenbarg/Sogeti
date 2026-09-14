using Sogeti.Planting;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Sogeti.UI.WorldUI
{
    /// <summary>
    /// The water meter that floats above one plant.
    /// It reads Plant.Water01 and never owns the number. Plant.HasWaterMeter is the
    /// only visibility switch, so a dead or fully grown plant shows nothing.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlantWaterMeter : MonoBehaviour
    {
        [Header("Source")]
        [SerializeField]
        private Plant plant;

        [SerializeField]
        [Tooltip("The player camera position, written every frame by ShareCameraPosition.")]
        private Vector3Variable cameraPosition;

        [Header("Parts")]
        [SerializeField]
        [Tooltip("Sits at the left edge of the bar. Scaling it on X drains the fill to the left.")]
        private Transform fillPivot;

        [SerializeField]
        private Renderer fillRenderer;

        [SerializeField]
        private Transform droplet;

        [Header("Urgency")]
        [SerializeField]
        private Material healthyMaterial;

        [SerializeField]
        private Material lowMaterial;

        [SerializeField]
        private Material criticalMaterial;

        [SerializeField]
        [Range(0f, 1f)]
        private float lowThreshold = WaterUrgencyBands.DefaultLowThreshold;

        [SerializeField]
        [Range(0f, 1f)]
        private float criticalThreshold = WaterUrgencyBands.DefaultCriticalThreshold;

        [Header("Range")]
        [SerializeField]
        [Tooltip("The meter appears inside this distance.")]
        private float showDistance = 15f;

        [SerializeField]
        [Tooltip("The meter hides past this distance. Keep it above the show distance to stop flicker.")]
        private float hideDistance = 16f;

        [SerializeField]
        [Tooltip("Optional. Grows the meter with distance so a far plant stays readable.")]
        private WorldUIScaleAtDistance scaleAtDistance;

        [Header("Droplet pulse")]
        [SerializeField]
        private float pulseAmount = 0.25f;

        [SerializeField]
        private float pulseSpeed = 5f;

        private WorldUILookAtPlayer lookAtPlayer;
        private Renderer[] meterRenderers;
        private Vector3 fillBaseScale = Vector3.one;
        private Vector3 dropletBaseScale = Vector3.one;
        private WaterUrgency urgency = WaterUrgency.Healthy;
        private bool hasUrgency;
        private bool isVisible;
        private bool hasVisibility;

        private void Awake()
        {
            if (plant == null)
            {
                plant = GetComponentInParent<Plant>();
            }

            lookAtPlayer = GetComponent<WorldUILookAtPlayer>();
            meterRenderers = GetComponentsInChildren<Renderer>(true);

            if (fillPivot != null)
            {
                fillBaseScale = fillPivot.localScale;
            }

            if (droplet != null)
            {
                dropletBaseScale = droplet.localScale;
            }

            if (plant != null)
            {
                // A child Awake runs inside Instantiate, and the planter calls Initialize after
                // that returns, so the Planted event still arrives here.
                plant.Planted += OnPlantChanged;
                plant.StageAdvanced += OnPlantChanged;
                plant.Died += OnPlantDied;
            }

            SetVisible(false);
            enabled = false;
        }

        private void OnDestroy()
        {
            if (plant == null)
            {
                return;
            }

            plant.Planted -= OnPlantChanged;
            plant.StageAdvanced -= OnPlantChanged;
            plant.Died -= OnPlantDied;
        }

        private void OnPlantChanged(Plant changed, int points) => RefreshLifecycle();

        private void OnPlantDied(Plant dead) => RefreshLifecycle();

        /// <summary>Stops the meter for good once the plant has no meter left to show.</summary>
        private void RefreshLifecycle()
        {
            // Only HasWaterMeter may disable this. A stage advance from 0 to 1 keeps the meter alive.
            bool draining = plant != null && plant.HasWaterMeter;
            if (!draining)
            {
                SetVisible(false);
            }

            enabled = draining;
        }

        private void Update()
        {
            if (plant == null || !plant.HasWaterMeter)
            {
                RefreshLifecycle();
                return;
            }

            if (!TryGetPlayerPosition(out Vector3 player))
            {
                return;
            }

            SetVisible(IsInRange(player));
            if (!isVisible)
            {
                return;
            }

            UpdateFill();
            UpdateDroplet();

            if (lookAtPlayer != null)
            {
                lookAtPlayer.LookAtPlayer(player);
            }

            if (scaleAtDistance != null)
            {
                scaleAtDistance.SetScaleAtDistance(player);
            }
        }

        /// <summary>
        /// ShareCameraPosition writes in Update and script order is undefined, so frame one
        /// can still read the stored origin. One skipped frame beats a meter that pops.
        /// </summary>
        private bool TryGetPlayerPosition(out Vector3 player)
        {
            player = cameraPosition == null ? Vector3.zero : cameraPosition.Value;
            return player.sqrMagnitude > 0.0001f;
        }

        // Two distances, not one. A single threshold flickers while the player stands on the edge.
        private bool IsInRange(Vector3 player)
        {
            float limit = isVisible ? Mathf.Max(hideDistance, showDistance) : showDistance;
            return Vector3.Distance(player, transform.position) <= limit;
        }

        private void UpdateFill()
        {
            float water = Mathf.Clamp01(plant.Water01);

            if (fillPivot != null)
            {
                Vector3 scale = fillBaseScale;
                scale.x = fillBaseScale.x * water;
                fillPivot.localScale = scale;
            }

            ApplyUrgency(WaterUrgencyBands.For(water, lowThreshold, criticalThreshold));
        }

        private void ApplyUrgency(WaterUrgency next)
        {
            if (hasUrgency && next == urgency)
            {
                return;
            }

            urgency = next;
            hasUrgency = true;

            Material material = MaterialFor(next);
            if (fillRenderer != null && material != null)
            {
                // sharedMaterial, not material. An instance leaks one material per plant, and a
                // MaterialPropertyBlock would drop this renderer out of the SRP batcher.
                fillRenderer.sharedMaterial = material;
            }
        }

        private Material MaterialFor(WaterUrgency value)
        {
            switch (value)
            {
                case WaterUrgency.Critical:
                    return criticalMaterial;
                case WaterUrgency.Low:
                    return lowMaterial;
                default:
                    return healthyMaterial;
            }
        }

        private void UpdateDroplet()
        {
            if (droplet == null)
            {
                return;
            }

            if (urgency != WaterUrgency.Critical)
            {
                droplet.localScale = dropletBaseScale;
                return;
            }

            // A pulse on the last band only. A meter that always moves stops reading as a warning.
            float pulse = 1f + Mathf.Abs(Mathf.Sin(Time.time * pulseSpeed)) * pulseAmount;
            droplet.localScale = dropletBaseScale * pulse;
        }

        private void SetVisible(bool visible)
        {
            if (hasVisibility && isVisible == visible)
            {
                return;
            }

            isVisible = visible;
            hasVisibility = true;

            for (int i = 0; i < meterRenderers.Length; i++)
            {
                if (meterRenderers[i] != null)
                {
                    meterRenderers[i].enabled = visible;
                }
            }
        }
    }
}
