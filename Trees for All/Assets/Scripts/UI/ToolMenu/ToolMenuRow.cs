using System;
using UnityEngine;

namespace Sogeti.UI.ToolMenu
{
    /// <summary>
    /// The tool row.
    /// An array position is a tool index on RightHandToolSwitch, so both lists
    /// must keep the same order.
    /// </summary>
    public class ToolMenuRow : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("In the same order as the tools array on RightHandToolSwitch.")]
        private ToolMenuEntry[] entries = Array.Empty<ToolMenuEntry>();

        public event Action<int> ToolChosen;

        public void SetSelected(int toolIndex)
        {
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i] != null)
                {
                    entries[i].SetSelected(i == toolIndex);
                }
            }
        }

        private void OnEnable()
        {
            foreach (ToolMenuEntry entry in entries)
            {
                if (entry != null)
                {
                    entry.Clicked += OnEntryClicked;
                }
            }
        }

        private void OnDisable()
        {
            foreach (ToolMenuEntry entry in entries)
            {
                if (entry != null)
                {
                    entry.Clicked -= OnEntryClicked;
                }
            }
        }

        private void OnEntryClicked(ToolMenuEntry entry)
        {
            int index = Array.IndexOf(entries, entry);
            if (index >= 0)
            {
                ToolChosen?.Invoke(index);
            }
        }
    }
}
