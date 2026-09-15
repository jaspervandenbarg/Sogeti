using Sogeti.Planting;
using UnityAtoms;
using UnityEngine;

namespace Sogeti.Atoms
{
    /// <summary>Event of type `SeedDefinition`.</summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(fileName = "SeedDefinitionEvent", menuName = "Sogeti/Trees for All/Atoms/Events/Seed Definition")]
    public sealed class SeedDefinitionEvent : AtomEvent<SeedDefinition>
    {
    }
}
