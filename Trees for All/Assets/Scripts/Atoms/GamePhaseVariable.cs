using Sogeti.Game;
using UnityAtoms;
using UnityEngine;

namespace Sogeti.Atoms
{
    /// <summary>
    /// The phase the round is in, broadcast to the panel and the player gate.
    /// GameSession still owns the phase machine and writes this after every
    /// accepted transition. Nothing may write it back.
    /// </summary>
    [EditorIcon("atom-icon-lush")]
    [CreateAssetMenu(fileName = "GamePhaseVariable", menuName = "Sogeti/Trees for All/Atoms/Variables/Game Phase")]
    public sealed class GamePhaseVariable : AtomVariable<GamePhase, GamePhasePair, GamePhaseEvent, GamePhasePairEvent, GamePhaseGamePhaseFunction>
    {
        protected override bool ValueEquals(GamePhase other) => _value == other;
    }
}
