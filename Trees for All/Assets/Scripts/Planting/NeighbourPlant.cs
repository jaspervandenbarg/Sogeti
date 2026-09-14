using UnityEngine;

namespace Sogeti.Planting
{
    /// <summary>
    /// One already planted plant near a candidate spot.
    /// A struct of plain data, so the spacing rules stay testable without a scene.
    /// </summary>
    public readonly struct NeighbourPlant
    {
        public readonly SeedDefinition Seed;
        public readonly Vector3 Position;

        public NeighbourPlant(SeedDefinition seed, Vector3 position)
        {
            Seed = seed;
            Position = position;
        }
    }
}
