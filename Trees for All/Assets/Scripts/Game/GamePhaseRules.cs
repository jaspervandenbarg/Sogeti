namespace Sogeti.Game
{
    /// <summary>
    /// The legal moves between phases, and who may act in each.
    /// Pure, so a test reaches the rule without a scene. GameSession asks before
    /// every transition, which keeps the guard in one place instead of one if
    /// per caller.
    /// </summary>
    public static class GamePhaseRules
    {
        /// <summary>True while the player may teleport, plant, water and open the menu.</summary>
        public static bool PlayerActsIn(GamePhase phase) => phase == GamePhase.Playing;

        /// <summary>True while a panel owns the view and the HUD stays hidden.</summary>
        public static bool PanelShowsIn(GamePhase phase) => !PlayerActsIn(phase);

        /// <summary>
        /// Ready to Playing starts a round. Playing to Ended is the timer.
        /// Ended to Ready exists for a soft reset. The scene reload path never uses
        /// it, because the reload lands on a fresh session already in Ready.
        /// </summary>
        public static bool CanTransition(GamePhase from, GamePhase to)
        {
            if (from == to)
            {
                return false;
            }

            switch (from)
            {
                case GamePhase.Ready:
                    return to == GamePhase.Playing;
                case GamePhase.Playing:
                    return to == GamePhase.Ended;
                case GamePhase.Ended:
                    return to == GamePhase.Ready;
                default:
                    return false;
            }
        }
    }
}
