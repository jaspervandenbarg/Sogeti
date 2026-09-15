using UnityAtoms;
using UnityEngine;

namespace Sogeti.Atoms
{
    /// <summary>Event of type `GamePhasePair`, the new phase and the one before it.</summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(fileName = "GamePhasePairEvent", menuName = "Sogeti/Trees for All/Atoms/Events/Game Phase Pair")]
    public sealed class GamePhasePairEvent : AtomEvent<GamePhasePair>
    {
    }
}
