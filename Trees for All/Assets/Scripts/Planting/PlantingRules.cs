using System.Collections.Generic;
using UnityEngine;

namespace Sogeti.Planting
{
    /// <summary>
    /// The planting spot rules, as pure functions.
    /// The physics queries live in PlantPlacementQuery. Splitting them keeps every
    /// rule verifiable in EditMode on a PC with no headset.
    /// </summary>
    public static class PlantingRules
    {
        /// <summary>True when the layer is on the planting allow list.</summary>
        public static bool IsPlantableSurface(int layer, LayerMask plantableMask) =>
            (plantableMask.value & (1 << layer)) != 0;

        /// <summary>Mirrors the teleport slope filter, so ground you cannot stand on is ground you cannot plant on.</summary>
        public static bool IsGroundFlatEnough(Vector3 surfaceNormal, float maxAngleDegrees)
        {
            if (surfaceNormal == Vector3.zero)
            {
                return false;
            }

            return Vector3.Angle(surfaceNormal, Vector3.up) <= Mathf.Max(maxAngleDegrees, 0f);
        }

        /// <summary>
        /// Radius for the single OverlapSphere that answers every spacing question.
        /// Spacing is mutual, so a neighbour type that demands more than this seed
        /// does still has to be found. Asking the seed catalog covers that case.
        /// </summary>
        public static float NeighbourSearchRadius(
            SeedDefinition seed,
            IReadOnlyList<SeedDefinition> seedCatalog,
            float obstacleClearance)
        {
            float radius = Mathf.Max(obstacleClearance, 0f);
            if (seed == null)
            {
                return radius;
            }

            radius = Mathf.Max(radius, seed.LargestSpacing);

            if (seedCatalog == null)
            {
                return radius;
            }

            for (int i = 0; i < seedCatalog.Count; i++)
            {
                SeedDefinition other = seedCatalog[i];
                if (other != null)
                {
                    radius = Mathf.Max(radius, seed.RequiredSpacingTo(other));
                }
            }

            return radius;
        }

        /// <summary>
        /// Checks a spot against every nearby plant.
        /// Reports the worst offender, not the first one found, so the player reads
        /// the neighbour that actually decides the spot.
        /// </summary>
        public static PlacementResult CheckSpacing(
            SeedDefinition seed,
            Vector3 position,
            Vector3 surfaceNormal,
            IReadOnlyList<NeighbourPlant> neighbours)
        {
            if (seed == null)
            {
                return PlacementResult.NoSeed();
            }

            if (neighbours == null)
            {
                return PlacementResult.Valid(position, surfaceNormal);
            }

            float worstShortfall = 0f;
            SeedDefinition worstSeed = null;
            float worstRequired = 0f;
            float worstDistance = 0f;

            for (int i = 0; i < neighbours.Count; i++)
            {
                NeighbourPlant neighbour = neighbours[i];
                float required = seed.RequiredSpacingTo(neighbour.Seed);
                float distance = GroundDistance(position, neighbour.Position);
                float shortfall = required - distance;

                if (shortfall > worstShortfall)
                {
                    worstShortfall = shortfall;
                    worstSeed = neighbour.Seed;
                    worstRequired = required;
                    worstDistance = distance;
                }
            }

            if (worstShortfall <= 0f)
            {
                return PlacementResult.Valid(position, surfaceNormal);
            }

            return PlacementResult.TooClose(position, surfaceNormal, worstSeed, worstRequired, worstDistance);
        }

        /// <summary>
        /// Spacing measures the ground footprint, so height is ignored.
        /// A sphere query of the same radius still finds every plant this can reject.
        /// </summary>
        public static float GroundDistance(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x;
            float dz = a.z - b.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }
    }
}
