using Sogeti.Game;
using UnityAtoms.Editor;
using UnityEditor;

namespace Sogeti.Atoms.Editor
{
    /// <summary>Inspector for the round phase Variable. Locks Value outside Play Mode.</summary>
    [CustomEditor(typeof(GamePhaseVariable))]
    public sealed class GamePhaseVariableEditor : AtomVariableEditor<GamePhase, GamePhasePair>
    {
    }
}
