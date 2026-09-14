using System.Collections.Generic;
using UnityEngine;

namespace Sogeti.Planting
{
    /// <summary>
    /// Runs the physics for one planting spot, then hands the numbers to PlantingRules.
    /// Plain C# and reusable, so the planting ray and any later tool share one answer.
    /// Buffers are allocated once, because this runs every frame while the player aims.
    /// </summary>
    public class PlantPlacementQuery
    {
        // A 5 m sphere at the tightest spacing holds about 30 plants. The headroom
        // matters: colliders past this cap are dropped and would let a seed land too close.
        private const int MaxNeighbours = 64;

        private readonly Collider[] overlapBuffer = new Collider[MaxNeighbours];
        private readonly List<NeighbourPlant> neighbours = new List<NeighbourPlant>(MaxNeighbours);

        private readonly LayerMask occluderMask;
        private readonly LayerMask plantableMask;
        private readonly LayerMask blockerMask;
        private readonly float maxSurfaceAngle;
        private readonly float obstacleClearance;

        private IReadOnlyList<SeedDefinition> seedCatalog;
        private bool warnedAboutFullBuffer;

        /// <param name="occluderMask">Everything the ray can stop on. Refusing a spot needs the nearest hit, not the nearest legal hit.</param>
        /// <param name="plantableMask">The allow list. Only these layers accept a seed.</param>
        /// <param name="blockerMask">Layers that carry plants and scenery obstacles.</param>
        public PlantPlacementQuery(
            LayerMask occluderMask,
            LayerMask plantableMask,
            LayerMask blockerMask,
            float maxSurfaceAngle,
            float obstacleClearance,
            IReadOnlyList<SeedDefinition> seedCatalog)
        {
            this.occluderMask = occluderMask;
            this.plantableMask = plantableMask;
            this.blockerMask = blockerMask;
            this.maxSurfaceAngle = maxSurfaceAngle;
            this.obstacleClearance = Mathf.Max(obstacleClearance, 0f);
            this.seedCatalog = seedCatalog;
        }

        /// <summary>The tool menu step feeds the seeds the player can pick, so spacing stays mutual.</summary>
        public void SetSeedCatalog(IReadOnlyList<SeedDefinition> catalog) => seedCatalog = catalog;

        public PlacementResult Evaluate(SeedDefinition seed, Ray ray, float maxDistance)
        {
            if (seed == null)
            {
                return PlacementResult.NoSeed();
            }

            // Triggers are ignored everywhere in gameplay. The pond refill trigger overlaps the ground.
            if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance, occluderMask, QueryTriggerInteraction.Ignore))
            {
                return PlacementResult.NoSurface();
            }

            if (!PlantingRules.IsPlantableSurface(hit.collider.gameObject.layer, plantableMask))
            {
                return PlacementResult.Rejected(PlacementStatus.NotPlantableSurface, hit.point, hit.normal);
            }

            if (!PlantingRules.IsGroundFlatEnough(hit.normal, maxSurfaceAngle))
            {
                return PlacementResult.Rejected(PlacementStatus.GroundTooSteep, hit.point, hit.normal);
            }

            return EvaluateSurroundings(seed, hit.point, hit.normal);
        }

        private PlacementResult EvaluateSurroundings(SeedDefinition seed, Vector3 point, Vector3 normal)
        {
            float radius = PlantingRules.NeighbourSearchRadius(seed, seedCatalog, obstacleClearance);

            neighbours.Clear();
            int found = Physics.OverlapSphereNonAlloc(
                point,
                radius,
                overlapBuffer,
                blockerMask,
                QueryTriggerInteraction.Ignore);

            WarnOnceIfBufferIsFull(found);

            for (int i = 0; i < found; i++)
            {
                Collider collider = overlapBuffer[i];
                if (collider == null)
                {
                    continue;
                }

                Plant plant = collider.GetComponentInParent<Plant>();
                if (plant != null)
                {
                    neighbours.Add(new NeighbourPlant(plant.Seed, plant.transform.position));
                    continue;
                }

                // Scenery. An axis aligned box test is conservative, so a seed never lands inside a rock.
                if (collider.bounds.SqrDistance(point) < obstacleClearance * obstacleClearance)
                {
                    return PlacementResult.Rejected(PlacementStatus.BlockedByObstacle, point, normal);
                }
            }

            return PlantingRules.CheckSpacing(seed, point, normal, neighbours);
        }

        // A full buffer drops colliders, and a dropped neighbour lets a seed land too close.
        // Once per query object is enough. This runs every frame while the player aims.
        private void WarnOnceIfBufferIsFull(int found)
        {
            if (found < MaxNeighbours || warnedAboutFullBuffer)
            {
                return;
            }

            warnedAboutFullBuffer = true;
            Debug.LogWarning($"Planting found {MaxNeighbours} colliders, the buffer cap. A neighbour may be missed.");
        }
    }
}
