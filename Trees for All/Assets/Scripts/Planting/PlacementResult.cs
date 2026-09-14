using UnityEngine;

namespace Sogeti.Planting
{
    /// <summary>
    /// The verdict on one candidate planting spot, plus the numbers behind it.
    /// The preview needs the reason, not only the yes or no, so it carries the
    /// blocking seed type and the two distances that failed the spacing test.
    /// </summary>
    public readonly struct PlacementResult
    {
        public readonly PlacementStatus Status;
        public readonly Vector3 Position;
        public readonly Vector3 Normal;

        /// <summary>The seed type of the plant that blocked the spot. Null for every other status.</summary>
        public readonly SeedDefinition BlockingSeed;

        public readonly float RequiredSpacing;
        public readonly float ActualDistance;

        private PlacementResult(
            PlacementStatus status,
            Vector3 position,
            Vector3 normal,
            SeedDefinition blockingSeed,
            float requiredSpacing,
            float actualDistance)
        {
            Status = status;
            Position = position;
            Normal = normal;
            BlockingSeed = blockingSeed;
            RequiredSpacing = requiredSpacing;
            ActualDistance = actualDistance;
        }

        public bool IsValid => Status == PlacementStatus.Valid;

        /// <summary>True when the ray found ground, so the preview has a place to stand.</summary>
        public bool HasSurface => Status != PlacementStatus.NoSurface && Status != PlacementStatus.NoSeedSelected;

        public static PlacementResult Valid(Vector3 position, Vector3 normal) =>
            new PlacementResult(PlacementStatus.Valid, position, normal, null, 0f, 0f);

        public static PlacementResult Rejected(PlacementStatus status, Vector3 position, Vector3 normal) =>
            new PlacementResult(status, position, normal, null, 0f, 0f);

        public static PlacementResult NoSurface() =>
            new PlacementResult(PlacementStatus.NoSurface, Vector3.zero, Vector3.up, null, 0f, 0f);

        public static PlacementResult NoSeed() =>
            new PlacementResult(PlacementStatus.NoSeedSelected, Vector3.zero, Vector3.up, null, 0f, 0f);

        public static PlacementResult TooClose(
            Vector3 position,
            Vector3 normal,
            SeedDefinition blockingSeed,
            float requiredSpacing,
            float actualDistance) =>
            new PlacementResult(
                PlacementStatus.TooCloseToPlant,
                position,
                normal,
                blockingSeed,
                requiredSpacing,
                actualDistance);
    }
}
