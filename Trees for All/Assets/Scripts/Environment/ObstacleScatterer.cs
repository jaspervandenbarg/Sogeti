using System.Collections.Generic;
using UnityEngine;

namespace Sogeti.Environment
{
    /// <summary>
    /// Scatters rock and branch prefabs across a terrain tile at scene start.
    /// Density is a slider, so the designer can tune obstacle count without touching code.
    /// </summary>
    [DisallowMultipleComponent]
    public class ObstacleScatterer : MonoBehaviour
    {
        [Header("Terrain")]
        [Tooltip("The terrain tile to scatter obstacles on.")]
        [SerializeField]
        private Terrain terrain;

        [Header("Obstacles")]
        [Tooltip("Rock and branch prefabs. One is picked at random for each spot.")]
        [SerializeField]
        private GameObject[] obstaclePrefabs;

        [Header("Density")]
        [Tooltip("Obstacles per 100 square metres of terrain. Raise or lower this to tune scatter density.")]
        [SerializeField]
        [Range(0f, 20f)]
        private float densityPerHundredSqm = 3f;

        [Tooltip("Minimum gap between two placed obstacles, in metres.")]
        [SerializeField]
        private float minSpacing = 3f;

        [Tooltip("Placement tries per obstacle before that slot is skipped.")]
        [SerializeField]
        private int maxAttemptsPerObstacle = 10;

        [Header("Avoidance")]
        [Tooltip("A candidate spot inside these layers is skipped. Covers the pond and any obstacle already placed.")]
        [SerializeField]
        private LayerMask avoidMask;

        [Tooltip("Radius used to test a candidate spot against the avoid mask.")]
        [SerializeField]
        private float avoidCheckRadius = 1f;

        private readonly List<Vector3> placedPositions = new List<Vector3>();

        private void Start()
        {
            Scatter();
        }

        private void Scatter()
        {
            if (terrain == null || obstaclePrefabs == null || obstaclePrefabs.Length == 0)
            {
                Debug.LogWarning("ObstacleScatterer has no terrain or no obstacle prefabs assigned.", this);
                return;
            }

            placedPositions.Clear();
            int count = CalculateObstacleCount();

            for (int i = 0; i < count; i++)
            {
                if (TryFindSpot(out Vector3 point))
                {
                    PlaceObstacle(point);
                }
            }
        }

        private int CalculateObstacleCount()
        {
            Vector3 size = terrain.terrainData.size;
            float areaSqm = size.x * size.z;
            return Mathf.RoundToInt(areaSqm / 100f * densityPerHundredSqm);
        }

        private bool TryFindSpot(out Vector3 point)
        {
            Vector3 size = terrain.terrainData.size;
            Vector3 origin = terrain.transform.position;

            for (int attempt = 0; attempt < maxAttemptsPerObstacle; attempt++)
            {
                float x = Random.Range(0f, size.x);
                float z = Random.Range(0f, size.z);
                Vector3 flatPoint = origin + new Vector3(x, 0f, z);
                float y = terrain.SampleHeight(flatPoint) + origin.y;
                Vector3 candidate = new Vector3(flatPoint.x, y, flatPoint.z);

                if (IsSpotFree(candidate))
                {
                    point = candidate;
                    return true;
                }
            }

            point = default;
            return false;
        }

        private bool IsSpotFree(Vector3 point)
        {
            if (Physics.CheckSphere(point, avoidCheckRadius, avoidMask, QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            float minSqr = minSpacing * minSpacing;
            foreach (Vector3 placed in placedPositions)
            {
                if ((placed - point).sqrMagnitude < minSqr)
                {
                    return false;
                }
            }

            return true;
        }

        private void PlaceObstacle(Vector3 point)
        {
            GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
            Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            Instantiate(prefab, point, rotation, transform);
            placedPositions.Add(point);
        }

        private void OnValidate()
        {
            densityPerHundredSqm = Mathf.Max(0f, densityPerHundredSqm);
            minSpacing = Mathf.Max(0.1f, minSpacing);
            maxAttemptsPerObstacle = Mathf.Max(1, maxAttemptsPerObstacle);
            avoidCheckRadius = Mathf.Max(0f, avoidCheckRadius);
        }
    }
}
