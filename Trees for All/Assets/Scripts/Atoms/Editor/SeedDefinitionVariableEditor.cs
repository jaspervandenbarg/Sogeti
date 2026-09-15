using Sogeti.Planting;
using UnityAtoms.Editor;
using UnityEditor;

namespace Sogeti.Atoms.Editor
{
    /// <summary>
    /// Inspector for the selected seed Variable.
    /// Without it Unity draws the default inspector, where Value looks editable
    /// and Initial Value looks optional. Play Mode then wipes Value back to
    /// Initial Value, so a seed set in the wrong field vanishes on the first frame.
    /// </summary>
    [CustomEditor(typeof(SeedDefinitionVariable))]
    public sealed class SeedDefinitionVariableEditor : AtomVariableEditor<SeedDefinition, SeedDefinitionPair>
    {
    }
}
