using System;
using UnityEngine;

namespace Sogeti.Interaction
{
    /// <summary>
    /// One tool at a time in the right hand.
    /// The planter and the watering can both read the right trigger, so letting
    /// both run would plant a seed on the press that pours. Only the active tool
    /// GameObject stays enabled, which settles that clash without a mode flag
    /// inside either tool. The tool menu is the only caller.
    /// </summary>
    public class RightHandToolSwitch : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("In menu order. Element 0 is the tool the hand starts with.")]
        private GameObject[] tools = Array.Empty<GameObject>();

        /// <summary>The new index and the tool it activated. The tool is null while the hand is stowed.</summary>
        public event Action<int, GameObject> ToolChanged;

        public int ActiveIndex { get; private set; }

        /// <summary>True while the menu holds the hand empty. The index survives, so closing restores the same tool.</summary>
        public bool ToolsStowed { get; private set; }

        public GameObject ActiveTool => !ToolsStowed && IsValidIndex(ActiveIndex) ? tools[ActiveIndex] : null;

        public void SelectTool(int index)
        {
            if (!IsValidIndex(index))
            {
                return;
            }

            ActiveIndex = index;
            ApplyActiveTool();
            ToolChanged?.Invoke(ActiveIndex, ActiveTool);
        }

        /// <summary>
        /// Empties the hand without losing the choice.
        /// The open menu shares the right trigger with the planter and the can, so
        /// every tool goes off. A press then reaches the UI and nothing else.
        /// </summary>
        public void SetToolsStowed(bool stowed)
        {
            if (ToolsStowed == stowed)
            {
                return;
            }

            ToolsStowed = stowed;
            ApplyActiveTool();
            ToolChanged?.Invoke(ActiveIndex, ActiveTool);
        }

        private void Awake()
        {
            ActiveIndex = 0;
            ApplyActiveTool();
        }

        private void ApplyActiveTool()
        {
            for (int i = 0; i < tools.Length; i++)
            {
                if (tools[i] != null)
                {
                    tools[i].SetActive(!ToolsStowed && i == ActiveIndex);
                }
            }
        }

        private bool IsValidIndex(int index) => index >= 0 && index < tools.Length && tools[index] != null;
    }
}
