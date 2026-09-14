using System;
using Sogeti.Interaction;
using Sogeti.Planting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Sogeti.UI.ToolMenu
{
    /// <summary>
    /// The wrist menu.
    /// It owns the open state and joins the two rows to the right hand. The panel
    /// stays open after a pick, so the player sets a tool and a seed in one visit.
    /// </summary>
    public class ToolMenuController : MonoBehaviour
    {
        [Tooltip("The panel itself. It starts inactive, so planting behaves as before on frame one.")]
        [SerializeField]
        private GameObject panelRoot;

        [Header("Hand")]
        [SerializeField]
        private RightHandToolSwitch toolSwitch;
        [SerializeField]
        private SeedPlanter planter;
        [Tooltip("The planter's place in the tools array. A seed pick switches to it first.")]
        [SerializeField]
        private int planterToolIndex;

        [Header("Rows")]
        [SerializeField]
        private SeedMenuRow seedRow;
        [SerializeField]
        private ToolMenuRow toolRow;

        [Header("Input")]
        [Tooltip("Opens and closes the panel. XRI Default has no left secondary action, so this one is defined here.")]
        [SerializeField]
        private InputActionProperty toggleAction;

        public event Action<bool> OpenChanged;

        public bool IsOpen { get; private set; }

        public void Open() => SetOpen(true);

        public void Close() => SetOpen(false);

        public void Toggle() => SetOpen(!IsOpen);

        private void Awake()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            IsOpen = false;
        }

        private void OnEnable()
        {
            if (seedRow != null)
            {
                seedRow.SeedChosen += OnSeedChosen;
            }

            if (toolRow != null)
            {
                toolRow.ToolChosen += OnToolChosen;
            }

            // The highlight follows the hand, never the click. The hand stays the one source of truth.
            if (planter != null)
            {
                planter.SeedChanged += OnSeedChanged;
            }

            if (toolSwitch != null)
            {
                toolSwitch.ToolChanged += OnToolChanged;
            }

            InputAction action = toggleAction.action;
            if (action == null)
            {
                return;
            }

            action.performed += OnTogglePressed;
            action.Enable();
        }

        private void OnDisable()
        {
            if (seedRow != null)
            {
                seedRow.SeedChosen -= OnSeedChosen;
            }

            if (toolRow != null)
            {
                toolRow.ToolChosen -= OnToolChosen;
            }

            if (planter != null)
            {
                planter.SeedChanged -= OnSeedChanged;
            }

            if (toolSwitch != null)
            {
                toolSwitch.ToolChanged -= OnToolChanged;
            }

            InputAction action = toggleAction.action;
            if (action == null)
            {
                return;
            }

            action.performed -= OnTogglePressed;

            // A shared action reference belongs to the rig. Only a locally owned action is safe to disable.
            if (toggleAction.reference == null)
            {
                action.Disable();
            }
        }

        private void OnTogglePressed(InputAction.CallbackContext context) => Toggle();

        private void SetOpen(bool open)
        {
            if (IsOpen == open)
            {
                return;
            }

            IsOpen = open;

            if (panelRoot != null)
            {
                panelRoot.SetActive(open);
            }

            // The open panel shares the right trigger, so the hand empties until it closes.
            if (toolSwitch != null)
            {
                toolSwitch.SetToolsStowed(open);
            }

            if (open)
            {
                RefreshSelection();
            }

            OpenChanged?.Invoke(IsOpen);
        }

        /// <summary>The rows build on their first activation, so the marks are set after the panel shows.</summary>
        private void RefreshSelection()
        {
            if (seedRow != null && planter != null)
            {
                seedRow.SetSelected(planter.SelectedSeed);
            }

            if (toolRow != null && toolSwitch != null)
            {
                toolRow.SetSelected(toolSwitch.ActiveIndex);
            }
        }

        private void OnSeedChosen(SeedDefinition seed)
        {
            // Picking a seed states an intent to plant, so the hand takes the planter with it.
            if (toolSwitch != null)
            {
                toolSwitch.SelectTool(planterToolIndex);
            }

            if (planter != null)
            {
                planter.SelectSeed(seed);
            }
        }

        private void OnToolChosen(int toolIndex)
        {
            if (toolSwitch != null)
            {
                toolSwitch.SelectTool(toolIndex);
            }
        }

        private void OnSeedChanged(SeedDefinition seed)
        {
            if (seedRow != null)
            {
                seedRow.SetSelected(seed);
            }
        }

        private void OnToolChanged(int toolIndex, GameObject tool)
        {
            if (toolRow != null)
            {
                toolRow.SetSelected(toolIndex);
            }
        }
    }
}
