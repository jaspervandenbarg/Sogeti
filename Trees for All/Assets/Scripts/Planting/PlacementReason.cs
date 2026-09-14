namespace Sogeti.Planting
{
    /// <summary>
    /// Turns a refused spot into one short line for the player.
    /// The target player has little game experience, so a red ghost alone does not
    /// teach the spacing rule. The text names the neighbour that blocks the spot.
    /// </summary>
    public static class PlacementReason
    {
        /// <summary>Empty for a legal spot, so the preview can hide the label.</summary>
        public static string Describe(in PlacementResult result)
        {
            switch (result.Status)
            {
                case PlacementStatus.Valid:
                    return string.Empty;
                case PlacementStatus.NoSeedSelected:
                    return "Select a seed first";
                case PlacementStatus.NoSurface:
                case PlacementStatus.NotPlantableSurface:
                    return "You can only plant on soil";
                case PlacementStatus.GroundTooSteep:
                    return "This ground is too steep";
                case PlacementStatus.BlockedByObstacle:
                    return "No room here";
                case PlacementStatus.TooCloseToPlant:
                    return DescribeSpacing(result);
                default:
                    return "You cannot plant here";
            }
        }

        private static string DescribeSpacing(in PlacementResult result)
        {
            if (result.BlockingSeed == null)
            {
                return "Another plant is too close";
            }

            return $"{result.BlockingSeed.DisplayName} is too close";
        }
    }
}
