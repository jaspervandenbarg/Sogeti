namespace Sogeti.Game
{
    /// <summary>
    /// Where one round stands.
    /// Ready is zero, so a fresh session reads as "waiting for the player" with no
    /// constructor call. That is the opposite of PlacementStatus, where a default
    /// must never read as a legal spot.
    /// </summary>
    public enum GamePhase
    {
        Ready = 0,
        Playing = 1,
        Ended = 2,
    }
}
