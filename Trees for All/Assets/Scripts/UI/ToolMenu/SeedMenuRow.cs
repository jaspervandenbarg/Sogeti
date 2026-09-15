using System;
using System.Collections.Generic;
using Sogeti.Planting;
using UnityEngine;

namespace Sogeti.UI.ToolMenu
{
    /// <summary>
    /// The seed row.
    /// It spawns one entry per catalog seed, so a fifth seed asset needs no code
    /// and no prefab edit.
    /// </summary>
    public class SeedMenuRow : MonoBehaviour
    {
        [SerializeField]
        private SeedCatalog catalog;
        [SerializeField]
        private ToolMenuEntry entryPrefab;
        [Tooltip("The layout group that holds the entries. Falls back to this transform.")]
        [SerializeField]
        private Transform entryParent;

        private readonly List<SeedDefinition> seeds = new List<SeedDefinition>();
        private readonly List<ToolMenuEntry> entries = new List<ToolMenuEntry>();

        public event Action<SeedDefinition> SeedChosen;

        public void SetSelected(SeedDefinition seed)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].SetSelected(seeds[i] == seed);
            }
        }

        private void Awake() => Build();

        private void Build()
        {
            if (catalog == null || entryPrefab == null)
            {
                Debug.LogError($"{name}: the seed row needs a catalog and an entry prefab.", this);
                return;
            }

            Transform parent = entryParent != null ? entryParent : transform;

            foreach (SeedDefinition seed in catalog.Seeds)
            {
                if (seed == null)
                {
                    continue;
                }

                ToolMenuEntry entry = Instantiate(entryPrefab, parent);
                entry.SetContent(seed.MenuIcon, seed.DisplayName);
                entry.Clicked += OnEntryClicked;

                seeds.Add(seed);
                entries.Add(entry);
            }
        }

        private void OnEntryClicked(ToolMenuEntry entry)
        {
            int index = entries.IndexOf(entry);
            if (index >= 0)
            {
                SeedChosen?.Invoke(seeds[index]);
            }
        }

        private void OnDestroy()
        {
            foreach (ToolMenuEntry entry in entries)
            {
                if (entry != null)
                {
                    entry.Clicked -= OnEntryClicked;
                }
            }
        }
    }
}
