using Sogeti.Atoms;
using Sogeti.Game;
using Sogeti.Planting;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Sogeti.Session
{
    /// <summary>
    /// One round. It owns the clock and the score, and nothing else.
    /// Every award arrives through a guarded Record call, so the "is the round
    /// running" test lives here once instead of once per caller.
    /// The Atoms are an outbound channel. ScoreTally, GameCountdown and the
    /// phase field stay the source of truth, so a missing Atom costs the
    /// broadcast and never the round.
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

        [Header("Broadcast")]
        [SerializeField]
        private IntVariable score;

        [Tooltip("Whole seconds. Atoms drops a repeat write, so the HUD text rebuilds once per second.")]
        [SerializeField]
        private IntVariable secondsRemaining;

        [SerializeField]
        private GamePhaseVariable phase;

        [Tooltip("The final tally. The game over panel needs the per seed rows, which the score Variable cannot carry.")]
        [SerializeField]
        private ScoreTallyEvent roundEnded;

        [Header("Commands")]
        [SerializeField]
        private VoidEvent startRoundRequested;

        private GameCountdown countdown;
        private ScoreTally tally;

        public GamePhase Phase { get; private set; } = GamePhase.Ready;

        public ScoreTally Tally => tally;

        public float SecondsRemaining => countdown == null ? roundSeconds : countdown.SecondsRemaining;

        public bool PlayerMayAct => GamePhaseRules.PlayerActsIn(Phase);

        /// <summary>
        /// Resets before it announces.
        /// The phase write un-hides the HUD, and a HUD that reads the old score
        /// for one frame looks like a failed restart.
        /// </summary>
        public void StartRound()
        {
            if (!GamePhaseRules.CanTransition(Phase, GamePhase.Playing))
            {
                return;
            }

            tally.Reset();
            countdown.Start();
            PublishTime();
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

        // The no argument overload never replays, so a late subscriber cannot restart a round.
        private void OnEnable()
        {
            if (startRoundRequested != null)
            {
                startRoundRequested.Register(StartRound);
            }
        }

        private void OnDisable()
        {
            if (startRoundRequested != null)
            {
                startRoundRequested.Unregister(StartRound);
            }
        }

        /// <summary>
        /// Forces the opening values out even when they equal the Initial Value.
        /// A plain write would change nothing, leave the replay buffer empty,
        /// and a consumer enabled later would read a blank channel.
        /// </summary>
        private void Start()
        {
            if (score != null)
            {
                score.SetValue(tally.Total, forceEvent: true);
            }

            if (secondsRemaining != null)
            {
                secondsRemaining.SetValue(WholeSecondsLeft(), forceEvent: true);
            }

            if (phase != null)
            {
                phase.SetValue(Phase, forceEvent: true);
            }

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
            PublishTime();
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

        private void OnTotalChanged(int total)
        {
            if (score != null)
            {
                score.Value = total;
            }
        }

        // The clock owns the end of the round. Nothing else may call EndRound on time.
        private void OnCountdownExpired() => SetPhase(GamePhase.Ended);

        private bool SetPhase(GamePhase next)
        {
            if (!GamePhaseRules.CanTransition(Phase, next))
            {
                return false;
            }

            Phase = next;

            // The tally goes first. The phase write opens the game over panel, and a
            // panel that reads the tally in the same call would find nothing there yet.
            if (Phase == GamePhase.Ended && roundEnded != null)
            {
                roundEnded.Raise(tally);
            }

            if (phase != null)
            {
                phase.Value = Phase;
            }

            return true;
        }

        // Atoms skips a write that changes nothing, which throttles the clock to whole seconds.
        private void PublishTime()
        {
            if (secondsRemaining != null)
            {
                secondsRemaining.Value = WholeSecondsLeft();
            }
        }

        private int WholeSecondsLeft() => Mathf.CeilToInt(Mathf.Max(0f, SecondsRemaining));

        private void OnValidate() => roundSeconds = Mathf.Max(roundSeconds, 1f);
    }
}
