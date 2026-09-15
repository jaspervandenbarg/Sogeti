using Sogeti.Planting;
using UnityAtoms;
using UnityEngine;

namespace Sogeti.Atoms
{
    /// <summary>
    /// The seed the player plants next.
    /// This asset owns the choice rather than broadcasting someone else's. The
    /// menu and the planter both switch off, so neither can hold it. Initial
    /// Value names the seed the hand starts with.
    /// </summary>
    [EditorIcon("atom-icon-lush")]
    [CreateAssetMenu(fileName = "SeedDefinitionVariable", menuName = "Sogeti/Trees for All/Atoms/Variables/Seed Definition")]
    public sealed class SeedDefinitionVariable : AtomVariable<SeedDefinition, SeedDefinitionPair, SeedDefinitionEvent, SeedDefinitionPairEvent, SeedDefinitionSeedDefinitionFunction>
    {
        protected override bool ValueEquals(SeedDefinition other) => _value == other;
    }
}
