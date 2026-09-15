using System;
using Sogeti.Game;
using Sogeti.Planting;
using UnityEngine;

namespace Sogeti.Session
{
    /// <summary>
    /// One round. It owns the clock and the score, and nothing else.
    /// Every award arrives through a guarded Record call, so the "is the round
    /// running" test lives here once instead of once per caller.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameSession : MonoBehaviour
    {
        [Tooltip("Round length in seconds. The assignment asks for a limited time.")]
        [SerializeField]
        private float roundSeconds = 180f;

        [Tooltip("Tick to skip the start panel and play at once. Off by default, so the player reads the goal first.")]
        [SerializeField]
        private bool autoStartOnLoad;

        private GameCountdown countdown;
        private ScoreTally tally;
        private int lastWholeSecond = -1;

        public event Action<GamePhase> PhaseChanged;

        /// <summary>The new total.</summary>
        public event Action<int> ScoreChanged;

        /// <summary>
        /// The seconds left, raised only when the whole second changes.
        /// A per-frame raise would rebuild the TMP mesh 90 times a second on
        /// mobile hardware for a clock that shows one decimal place of nothing.
        /// </summary>
        public event Action<float> SecondsRemainingChanged;

        public GamePhase Phase { get; private set; } = GamePhase.Ready;

        public ScoreTally Tally => tally;

        public float SecondsRemaining => countdown == null ? roundSeconds : countdown.SecondsRemaining;

        public bool PlayerMayAct => GamePhaseRules.PlayerActsIn(Phase);

        /// <summary>
        /// Resets before it announces.
        /// PhaseChanged un-hides the HUD, and a HUD that reads the old score for one
        /// frame looks like a failed restart.
        /// </summary>
        public void StartRound()
        {
            if (!GamePhaseRules.CanTransition(Phase, GamePhase.Playing))
            {
                return;
            }

            tally.Reset();
            countdown.Start();
            RaiseTime(force: true);
            SetPhase(GamePhase.Playing);
        }

        /// <summary>Ends the round early. The expiring clock calls the same path.</summary>
        public void EndRound()
        {
            countdown.Stop();
            SetPhase(GamePhase.Ended);
        }

        public void RecordPlanted(SeedDefinition seed, int points)
        {
            if (PlayerMayAct)
            {
                tally.RecordPlanted(seed, points);
            }
        }

        public void RecordStageReached(SeedDefinition seed, int points, bool fullyGrown)
        {
            if (PlayerMayAct)
            {
                tally.RecordStageReached(seed, points, fullyGrown);
            }
        }

        public void RecordLost()
        {
            if (PlayerMayAct)
            {
                tally.RecordLost();
            }
        }

        private void Awake()
        {
            tally = new ScoreTally();
            countdown = new GameCountdown(roundSeconds);

            tally.TotalChanged += OnTotalChanged;
            countdown.Expired += OnCountdownExpired;
        }

        // Consumers subscribe in OnEnable, which Unity runs before every Start.
        private void Start()
        {
            RaiseTime(force: true);
            PhaseChanged?.Invoke(Phase);

            if (autoStartOnLoad)
            {
                StartRound();
            }
        }

        private void Update()
        {
            if (Phase != GamePhase.Playing)
            {
                return;
            }

            countdown.Tick(Time.deltaTime);
            RaiseTime(force: false);
        }

        private void OnDestroy()
        {
            if (tally != null)
            {
                tally.TotalChanged -= OnTotalChanged;
            }

            if (countdown != null)
            {
                countdown.Expired -= OnCountdownExpired;
            }
        }

        private void OnTotalChanged(int total) => ScoreChanged?.Invoke(total);

        // The clock owns the end of the round. Nothing else may call EndRound on time.
        private void OnCountdownExpired() => SetPhase(GamePhase.Ended);

        private bool SetPhase(GamePhase next)
        {
            if (!GamePhaseRules.CanTransition(Phase, next))
            {
                return false;
            }

            Phase = next;
            PhaseChanged?.Invoke(Phase);
            return true;
        }

        private void RaiseTime(bool force)
        {
            int whole = Mathf.CeilToInt(Mathf.Max(0f, countdown.SecondsRemaining));
            if (!force && whole == lastWholeSecond)
            {
                return;
            }

            lastWholeSecond = whole;
            SecondsRemainingChanged?.Invoke(countdown.SecondsRemaining);
        }

        private void OnValidate() => roundSeconds = Mathf.Max(roundSeconds, 1f);
    }
}
