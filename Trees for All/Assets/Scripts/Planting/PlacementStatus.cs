namespace Sogeti.Planting
{
    /// <summary>
    /// Why a planting spot is legal or refused.
    /// The preview turns this into a colour and a short player-facing line,
    /// so a novice player learns the spacing rule instead of guessing.
    /// </summary>
    public enum PlacementStatus
    {
        // Valid is deliberately not zero. A default PlacementResult must never read as a legal spot.
        NoSurface = 0,
        Valid = 1,
        NoSeedSelected = 2,
        NotPlantableSurface = 3,
        GroundTooSteep = 4,
        BlockedByObstacle = 5,
        TooCloseToPlant = 6,
    }
}
