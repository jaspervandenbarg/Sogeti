using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Sogeti.Interaction
{
    /// <summary>
    /// One tool at a time in the right hand.
    /// The planter and the watering can both read the right trigger, so letting
    /// both run would plant a seed on the press that pours. Only the active tool
    /// GameObject stays enabled, which settles that clash without a mode flag
    /// inside either tool. The tool menu step replaces the button, not this API.
    /// </summary>
    public class RightHandToolSwitch : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("In player order. Element 0 is the tool the hand starts with.")]
        private GameObject[] tools = Array.Empty<GameObject>();

        [SerializeField]
        [Tooltip("Steps to the next tool. XRI Right Interaction has no primary button action, so this one is defined here.")]
        private InputActionProperty switchAction;

        /// <summary>The new index and the tool it activated.</summary>
        public event Action<int, GameObject> ToolChanged;

        public int ActiveIndex { get; private set; }

        public GameObject ActiveTool => IsValidIndex(ActiveIndex) ? tools[ActiveIndex] : null;

        /// <summary>The tool menu step calls this instead of stepping through the list.</summary>
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

        public void SelectNextTool()
        {
            if (tools.Length == 0)
            {
                return;
            }

            SelectTool((ActiveIndex + 1) % tools.Length);
        }

        private void Awake()
        {
            ActiveIndex = 0;
            ApplyActiveTool();
        }

        private void OnEnable()
        {
            InputAction action = switchAction.action;
            if (action == null)
            {
                return;
            }

            action.performed += OnSwitchPressed;
            action.Enable();
        }

        private void OnDisable()
        {
            InputAction action = switchAction.action;
            if (action == null)
            {
                return;
            }

            action.performed -= OnSwitchPressed;

            // A shared action reference belongs to the rig. Only a locally owned action is safe to disable.
            if (switchAction.reference == null)
            {
                action.Disable();
            }
        }

        private void OnSwitchPressed(InputAction.CallbackContext context) => SelectNextTool();

        private void ApplyActiveTool()
        {
            for (int i = 0; i < tools.Length; i++)
            {
                if (tools[i] != null)
                {
                    tools[i].SetActive(i == ActiveIndex);
                }
            }
        }

        private bool IsValidIndex(int index) => index >= 0 && index < tools.Length && tools[index] != null;
    }
}
