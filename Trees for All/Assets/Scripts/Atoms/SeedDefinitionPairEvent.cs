using UnityAtoms;
using UnityEngine;

namespace Sogeti.Atoms
{
    /// <summary>Event of type `SeedDefinitionPair`, the new seed and the one before it.</summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(fileName = "SeedDefinitionPairEvent", menuName = "Sogeti/Trees for All/Atoms/Events/Seed Definition Pair")]
    public sealed class SeedDefinitionPairEvent : AtomEvent<SeedDefinitionPair>
    {
    }
}
