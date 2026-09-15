using System.Collections.Generic;
using UnityEngine;

namespace Sogeti.Planting
{
    /// <summary>
    /// The seeds the player can pick, as one asset.
    /// The planter and the menu must read the same list. Spacing is mutual, so a
    /// menu with its own list would let the player plant a seed the rules never
    /// search for. One asset removes that drift.
    /// </summary>
    [CreateAssetMenu(fileName = "SeedCatalog", menuName = "Trees for All/Seed Catalog")]
    public class SeedCatalog : ScriptableObject
    {
        [SerializeField]
        [Tooltip("In menu order. Element 0 is the seed the planter starts with.")]
        private SeedDefinition[] seeds = System.Array.Empty<SeedDefinition>();

        public IReadOnlyList<SeedDefinition> Seeds => seeds;

        public int Count => seeds == null ? 0 : seeds.Length;

        public SeedDefinition FirstSeed => Count > 0 ? seeds[0] : null;

        private void OnValidate()
        {
            if (seeds == null)
            {
                return;
            }

            for (int i = 0; i < seeds.Length; i++)
            {
                if (seeds[i] == null)
                {
                    Debug.LogWarning($"{name}: seed {i} is empty. The menu skips it.", this);
                }
            }
        }
    }
}
