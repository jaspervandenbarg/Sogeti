using System;
using Sogeti.Planting;
using UnityAtoms;
using UnityEngine;

namespace Sogeti.Atoms
{
    /// <summary>IPair of type `SeedDefinition`. Feeds the changed with history Event.</summary>
    [Serializable]
    public struct SeedDefinitionPair : IPair<SeedDefinition>
    {
        public SeedDefinition Item1 { get => item1; set => item1 = value; }

        public SeedDefinition Item2 { get => item2; set => item2 = value; }

        [SerializeField]
        private SeedDefinition item1;

        [SerializeField]
        private SeedDefinition item2;

        public void Deconstruct(out SeedDefinition first, out SeedDefinition second)
        {
            first = Item1;
            second = Item2;
        }
    }
}
