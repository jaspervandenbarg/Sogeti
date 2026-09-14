using System;
using Sogeti.Planting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Sogeti.Interaction
{
    /// <summary>
    /// The planting ray on the right hand.
    /// Locomotion stays on the left hand and planting stays on the right, so the
    /// player never has to switch a mode mid aim. Every frame it re-tests the spot
    /// under the ray and feeds the preview, then plants on the trigger.
    /// </summary>
    public class SeedPlanter : MonoBehaviour
    {
        [Header("Ray")]
        [SerializeField]
        private Transform rayOrigin;
        [SerializeField]
        private float maxDistance = 8f;
        [SerializeField]
        private LineRenderer rayVisual;

        [Header("Rules")]
        [Tooltip("Everything the ray can stop on. A rock must hide the terrain behind it.")]
        [SerializeField]
        private LayerMask occluderMask;
        [Tooltip("The allow list. Terrain only.")]
        [SerializeField]
        private LayerMask plantableMask;
        [Tooltip("Layers that carry planted plants and scenery obstacles.")]
        [SerializeField]
        private LayerMask blockerMask;
        [SerializeField]
        private float maxSurfaceAngle = 30f;
        [SerializeField]
        private float obstacleClearance = 0.4f;

        [Header("Seeds")]
        [Tooltip("The seeds the player can pick. The tool menu reads the same asset.")]
        [SerializeField]
        private SeedCatalog seedCatalog;
        [SerializeField]
        private GameObject plantPrefab;
        [Tooltip("Optional parent for planted plants, so the Hierarchy stays readable.")]
        [SerializeField]
        private Transform plantParent;

        [Header("Feedback")]
        [SerializeField]
        private PlantingPreview preview;
        [SerializeField]
        private Color validRayColor = new Color(0.35f, 1f, 0.45f, 1f);
        [SerializeField]
        private Color blockedRayColor = new Color(1f, 0.35f, 0.3f, 1f);

        [Header("Input")]
        [SerializeField]
        private InputActionProperty plantAction;

        private PlantPlacementQuery query;
        private PlacementResult currentResult;

        /// <summary>The new plant, and the points it earned on the way in.</summary>
        public event Action<Plant, int> PlantPlaced;

        /// <summary>Raised on a refused trigger press, so a later step can add a sound or a hint.</summary>
        public event Action<PlacementResult> PlantRefused;

        /// <summary>The new seed. The menu highlight and the hand readout both follow this, never the click.</summary>
        public event Action<SeedDefinition> SeedChanged;

        public SeedDefinition SelectedSeed { get; private set; }

        public PlacementResult CurrentResult => currentResult;

        /// <summary>The only way to change the seed. Null puts the ray back in an idle state.</summary>
        public void SelectSeed(SeedDefinition seed)
        {
            if (SelectedSeed == seed)
            {
                return;
            }

            SelectedSeed = seed;
            if (seed == null && preview != null)
            {
                preview.Hide();
            }

            SeedChanged?.Invoke(SelectedSeed);
        }

        private void Awake()
        {
            if (rayOrigin == null)
            {
                rayOrigin = transform;
            }

            if (rayVisual != null)
            {
                rayVisual.useWorldSpace = true;
            }

            query = new PlantPlacementQuery(
                occluderMask,
                plantableMask,
                blockerMask,
                maxSurfaceAngle,
                obstacleClearance,
                seedCatalog != null ? seedCatalog.Seeds : null);

            // The menu opens on a seed the player never picked, so the hand is never empty.
            if (SelectedSeed == null && seedCatalog != null)
            {
                SelectSeed(seedCatalog.FirstSeed);
            }
        }

        private void OnEnable()
        {
            InputAction action = plantAction.action;
            if (action == null)
            {
                return;
            }

            action.performed += OnPlantPressed;
            action.Enable();
        }

        private void OnDisable()
        {
            InputAction action = plantAction.action;
            if (action == null)
            {
                return;
            }

            action.performed -= OnPlantPressed;

            // A shared action reference belongs to the rig. Only a locally owned action is safe to disable.
            if (plantAction.reference == null)
            {
                action.Disable();
            }

            if (preview != null)
            {
                preview.Hide();
            }

            ShowRay(false);
        }

        private void Update()
        {
            currentResult = query.Evaluate(SelectedSeed, new Ray(rayOrigin.position, rayOrigin.forward), maxDistance);

            if (preview != null)
            {
                preview.Show(SelectedSeed, currentResult);
            }

            UpdateRayVisual();
        }

        private void OnPlantPressed(InputAction.CallbackContext context)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            if (!currentResult.IsValid)
            {
                PlantRefused?.Invoke(currentResult);
                return;
            }

            Plant plant = SpawnPlant(currentResult);
            if (plant != null)
            {
                PlantPlaced?.Invoke(plant, SelectedSeed.PointsForPlanting);
            }
        }

        private Plant SpawnPlant(in PlacementResult result)
        {
            if (plantPrefab == null)
            {
                Debug.LogError($"{name}: no plant prefab assigned, nothing was planted.", this);
                return null;
            }

            GameObject instance = Instantiate(plantPrefab, result.Position, Quaternion.identity, plantParent);
            instance.name = $"{SelectedSeed.DisplayName} Plant";

            Plant plant = instance.GetComponent<Plant>();
            if (plant == null)
            {
                Debug.LogError($"{name}: the plant prefab has no Plant component.", this);
                Destroy(instance);
                return null;
            }

            plant.Initialize(SelectedSeed);
            return plant;
        }

        private void UpdateRayVisual()
        {
            if (rayVisual == null)
            {
                return;
            }

            if (SelectedSeed == null)
            {
                ShowRay(false);
                return;
            }

            ShowRay(true);

            Vector3 start = rayOrigin.position;
            Vector3 end = currentResult.HasSurface ? currentResult.Position : start + rayOrigin.forward * maxDistance;

            rayVisual.positionCount = 2;
            rayVisual.SetPosition(0, start);
            rayVisual.SetPosition(1, end);

            Color color = currentResult.IsValid ? validRayColor : blockedRayColor;
            rayVisual.startColor = color;
            rayVisual.endColor = color;
        }

        private void ShowRay(bool visible)
        {
            if (rayVisual != null && rayVisual.enabled != visible)
            {
                rayVisual.enabled = visible;
            }
        }

        private void OnValidate()
        {
            maxDistance = Mathf.Max(maxDistance, 0.1f);
            obstacleClearance = Mathf.Max(obstacleClearance, 0f);
            maxSurfaceAngle = Mathf.Clamp(maxSurfaceAngle, 0f, 89f);
        }
    }
}
