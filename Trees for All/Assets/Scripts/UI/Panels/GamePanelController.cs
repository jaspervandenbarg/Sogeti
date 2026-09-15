using Sogeti.Atoms;
using Sogeti.Game;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Sogeti.UI.Panels
{
    /// <summary>
    /// Joins the round to the panel.
    /// The phase decides everything. No button reaches into the session
    /// directly, so the panel can never disagree with the game about what is
    /// happening. Play and Restart leave as commands, which keeps this the
    /// panel's controller rather than the round's second owner.
    /// </summary>
    [DisallowMultipleComponent]
    public class GamePanelController : MonoBehaviour
    {
        [Header("Round")]
        [SerializeField]
        private GamePhaseVariable phase;

        [Tooltip("Carries the final tally. The panel needs the per seed rows, not just the total.")]
        [SerializeField]
        private ScoreTallyEvent roundEnded;

        [Header("Commands")]
        [SerializeField]
        private VoidEvent startRoundRequested;
        [SerializeField]
        private VoidEvent restartRequested;

        [Header("Panel")]
        [SerializeField]
        private GamePanelView panel;
        [SerializeField]
        private WorldPanelPlacer placer;
        [Tooltip("The player camera. The panel spawns in front of it, then stays put.")]
        [SerializeField]
        private Transform playerHead;

        private GamePhase pendingPhase = GamePhase.Ready;
        private ScoreTally finalTally;

        private void OnEnable()
        {
            if (panel == null)
            {
                Debug.LogError($"{name}: no panel is assigned, no screen will show.", this);
                return;
            }

            if (phase != null)
            {
                phase.Changed.Register(OnPhaseChanged);
            }

            if (roundEnded != null)
            {
                roundEnded.Register(OnRoundEnded);
            }

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
            if (phase != null)
            {
                phase.Changed.Unregister(OnPhaseChanged);
            }

            if (roundEnded != null)
            {
                roundEnded.Unregister(OnRoundEnded);
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

        // GameSession raises this before the phase write, so the tally is here by the time
        // the panel shows. The replay buffer hands it over again if this re-enables later.
        private void OnRoundEnded(ScoreTally tally) => finalTally = tally;

        private void OnPhaseChanged(GamePhase next)
        {
            if (GamePhaseRules.PlayerActsIn(next))
            {
                panel.Hide();
                return;
            }

            // Each panel lands where the player stands when it opens, not where the last one did.
            pendingPhase = next;

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
                panel.ShowGameOver(finalTally);
                return;
            }

            panel.ShowStart();
        }

        private void OnPlayPressed() => Raise(startRoundRequested);

        private void OnControlsPressed() => panel.ShowControls();

        // Back always returns to the screen the phase asks for, so it works from either panel.
        private void OnBackPressed() => ShowPending();

        private void OnRestartPressed() => Raise(restartRequested);

        private void Raise(VoidEvent command)
        {
            if (command != null)
            {
                command.Raise();
            }
        }
    }
}
