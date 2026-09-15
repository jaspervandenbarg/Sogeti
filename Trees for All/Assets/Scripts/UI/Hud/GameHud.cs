using Sogeti.Game;
using Sogeti.Session;
using TMPro;
using UnityEngine;

namespace Sogeti.UI.Hud
{
    /// <summary>
    /// The round clock and the score.
    /// It writes text only when the session says a value moved. The session raises
    /// the clock on whole seconds, so this never rebuilds a TMP mesh per frame.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameHud : MonoBehaviour
    {
        [SerializeField]
        private GameSession session;
        [SerializeField]
        private TMP_Text timeText;
        [SerializeField]
        private TMP_Text scoreText;

        private void OnEnable()
        {
            if (session == null)
            {
                Debug.LogError($"{name}: no session assigned, the HUD stays empty.", this);
                return;
            }

            session.SecondsRemainingChanged += ShowTime;
            session.ScoreChanged += ShowScore;

            // The gate hides the HUD between rounds, so it misses every change it was away for.
            ShowTime(session.SecondsRemaining);
            ShowScore(session.Tally != null ? session.Tally.Total : 0);
        }

        private void OnDisable()
        {
            if (session == null)
            {
                return;
            }

            session.SecondsRemainingChanged -= ShowTime;
            session.ScoreChanged -= ShowScore;
        }

        private void ShowTime(float secondsRemaining)
        {
            if (timeText != null)
            {
                timeText.text = CountdownDisplay.Format(secondsRemaining);
            }
        }

        private void ShowScore(int total)
        {
            if (scoreText != null)
            {
                scoreText.text = total.ToString();
            }
        }
    }
}
