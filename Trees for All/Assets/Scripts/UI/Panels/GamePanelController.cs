using Sogeti.Game;
using Sogeti.Session;
using UnityEngine;

namespace Sogeti.UI.Panels
{
    /// <summary>
    /// Joins the round to the panel and the player gate.
    /// The phase decides everything. No button reaches into the session directly,
    /// so the panel can never disagree with the game about what is happening.
    /// </summary>
    [DisallowMultipleComponent]
    public class GamePanelController : MonoBehaviour
    {
        [SerializeField]
        private GameSession session;
        [SerializeField]
        private PlayerActionGate gate;
        [SerializeField]
        private GameRestarter restarter;

        [Header("Panel")]
        [SerializeField]
        private GamePanelView panel;
        [SerializeField]
        private WorldPanelPlacer placer;
        [Tooltip("The player camera. The panel spawns in front of it, then stays put.")]
        [SerializeField]
        private Transform playerHead;

        private GamePhase pendingPhase = GamePhase.Ready;

        private void OnEnable()
        {
            if (session == null || panel == null)
            {
                Debug.LogError($"{name}: session or panel is missing, no screen will show.", this);
                return;
            }

            session.PhaseChanged += OnPhaseChanged;
            panel.PlayPressed += OnPlayPressed;
            panel.ControlsPressed += OnControlsPressed;
            panel.BackPressed += OnBackPressed;
            panel.RestartPressed += OnRestartPressed;

            if (placer != null)
            {
                placer.Placed += OnPlaced;
            }
        }

        private void OnDisable()
        {
            if (session != null)
            {
                session.PhaseChanged -= OnPhaseChanged;
            }

            if (panel != null)
            {
                panel.PlayPressed -= OnPlayPressed;
                panel.ControlsPressed -= OnControlsPressed;
                panel.BackPressed -= OnBackPressed;
                panel.RestartPressed -= OnRestartPressed;
            }

            if (placer != null)
            {
                placer.Placed -= OnPlaced;
            }
        }

        private void OnPhaseChanged(GamePhase phase)
        {
            bool playerActs = GamePhaseRules.PlayerActsIn(phase);

            if (gate != null)
            {
                gate.SetPlayerActionsEnabled(playerActs);
            }

            if (playerActs)
            {
                panel.Hide();
                return;
            }

            // Each panel lands where the player stands when it opens, not where the last one did.
            pendingPhase = phase;

            if (placer != null)
            {
                placer.RequestPlacement(playerHead);
                return;
            }

            ShowPending();
        }

        private void OnPlaced() => ShowPending();

        private void ShowPending()
        {
            if (pendingPhase == GamePhase.Ended)
            {
                panel.ShowGameOver(session.Tally);
                return;
            }

            panel.ShowStart();
        }

        private void OnPlayPressed() => session.StartRound();

        private void OnControlsPressed() => panel.ShowControls();

        // Back always returns to the screen the phase asks for, so it works from either panel.
        private void OnBackPressed() => ShowPending();

        private void OnRestartPressed()
        {
            if (restarter != null)
            {
                restarter.Restart();
            }
        }
    }
}
