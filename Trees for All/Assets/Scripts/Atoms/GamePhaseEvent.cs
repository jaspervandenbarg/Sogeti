using Sogeti.Game;
using UnityAtoms;
using UnityEngine;

namespace Sogeti.Atoms
{
    /// <summary>Event of type `GamePhase`.</summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(fileName = "GamePhaseEvent", menuName = "Sogeti/Trees for All/Atoms/Events/Game Phase")]
    public sealed class GamePhaseEvent : AtomEvent<GamePhase>
    {
    }
}
