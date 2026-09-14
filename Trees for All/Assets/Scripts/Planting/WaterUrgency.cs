namespace Sogeti.Planting
{
    /// <summary>
    /// How urgent one plant's water level is.
    /// The meter UI draws a band, not a gradient, so a novice player reads one of
    /// three clear states instead of judging a shade.
    /// </summary>
    public enum WaterUrgency
    {
        Healthy = 0,
        Low = 1,
        Critical = 2,
    }
}
