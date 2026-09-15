using Sogeti.Game;
using TMPro;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Sogeti.UI.Hud
{
    /// <summary>
    /// The round clock and the score.
    /// Both values arrive as whole numbers, and Atoms drops a write that changes
    /// nothing, so this never rebuilds a TMP mesh per frame. The gate hides the
    /// HUD between rounds; the Variables replay their current value on the way
    /// back, so no resync is needed here.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameHud : MonoBehaviour
    {
        [SerializeField]
        private IntVariable secondsRemaining;
        [SerializeField]
        private IntVariable score;
        [SerializeField]
        private TMP_Text timeText;
        [SerializeField]
        private TMP_Text scoreText;

        private void OnEnable()
        {
            if (secondsRemaining != null)
            {
                secondsRemaining.Changed.Register(ShowTime);
            }

            if (score != null)
            {
                score.Changed.Register(ShowScore);
            }
        }

        private void OnDisable()
        {
            if (secondsRemaining != null)
            {
                secondsRemaining.Changed.Unregister(ShowTime);
            }

            if (score != null)
            {
                score.Changed.Unregister(ShowScore);
            }
        }

        private void ShowTime(int wholeSeconds)
        {
            if (timeText != null)
            {
                timeText.text = CountdownDisplay.Format(wholeSeconds);
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
