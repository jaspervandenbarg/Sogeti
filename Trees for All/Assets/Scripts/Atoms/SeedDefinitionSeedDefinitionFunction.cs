using Sogeti.Planting;
using UnityAtoms;
using UnityEngine;

namespace Sogeti.Atoms
{
    /// <summary>Function from `SeedDefinition` to `SeedDefinition`. The Variable runs these before a write.</summary>
    [EditorIcon("atom-icon-sand")]
    public abstract class SeedDefinitionSeedDefinitionFunction : AtomFunction<SeedDefinition, SeedDefinition>
    {
    }
}
