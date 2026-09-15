using Sogeti.Game;
using UnityAtoms;
using UnityEngine;

namespace Sogeti.Atoms
{
    /// <summary>
    /// The final tally, raised once when the round ends.
    /// The score Variable carries the running total. The game over panel needs
    /// the per seed rows too, which no base Atom type can hold.
    /// </summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(fileName = "ScoreTallyEvent", menuName = "Sogeti/Trees for All/Atoms/Events/Score Tally")]
    public sealed class ScoreTallyEvent : AtomEvent<ScoreTally>
    {
    }
}
