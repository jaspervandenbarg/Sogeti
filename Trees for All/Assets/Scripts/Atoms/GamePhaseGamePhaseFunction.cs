using Sogeti.Game;
using UnityAtoms;
using UnityEngine;

namespace Sogeti.Atoms
{
    /// <summary>Function from `GamePhase` to `GamePhase`. The Variable runs these before a write.</summary>
    [EditorIcon("atom-icon-sand")]
    public abstract class GamePhaseGamePhaseFunction : AtomFunction<GamePhase, GamePhase>
    {
    }
}
