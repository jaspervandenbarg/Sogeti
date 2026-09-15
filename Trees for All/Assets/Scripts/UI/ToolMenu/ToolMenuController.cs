using Sogeti.Atoms;
using Sogeti.Interaction;
using Sogeti.Planting;
using UnityAtoms.BaseAtoms;
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

        [Tooltip("True while this panel is open. The HUD hides on it.")]
        [SerializeField]
        private BoolVariable toolMenuOpen;

        [Header("Hand")]
        [SerializeField]
        private RightHandToolSwitch toolSwitch;
        [Tooltip("The seed the hand plants next. The menu writes it here, never into the planter.")]
        [SerializeField]
        private SeedDefinitionVariable selectedSeed;
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
            PublishOpen();
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

            // The highlight follows the Variable, never the click. The Variable stays the one source of truth.
            if (selectedSeed != null)
            {
                selectedSeed.Changed.Register(OnSeedChanged);
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

            if (selectedSeed != null)
            {
                selectedSeed.Changed.Unregister(OnSeedChanged);
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

            PublishOpen();
        }

        private void PublishOpen()
        {
            if (toolMenuOpen != null)
            {
                toolMenuOpen.Value = IsOpen;
            }
        }

        /// <summary>Repaints both rows from the hand. The rows build on their first activation, so this also runs on open.</summary>
        private void RefreshSelection()
        {
            if (toolRow != null && toolSwitch != null)
            {
                toolRow.SetSelected(toolSwitch.ActiveIndex);
            }

            if (seedRow != null)
            {
                seedRow.SetSelected(SeedOnShow());
            }
        }

        // The seed row speaks for the planter alone, so picking the can leaves it dark.
        // The Variable keeps the seed, so coming back restores the same one.
        private SeedDefinition SeedOnShow()
        {
            if (selectedSeed == null)
            {
                return null;
            }

            if (toolSwitch != null && toolSwitch.ActiveIndex != planterToolIndex)
            {
                return null;
            }

            return selectedSeed.Value;
        }

        private void OnSeedChosen(SeedDefinition seed)
        {
            // Picking a seed states an intent to plant, so the hand takes the planter with it.
            // The tool comes first, because the write below repaints both rows.
            if (toolSwitch != null)
            {
                toolSwitch.SelectTool(planterToolIndex);
            }

            if (selectedSeed != null)
            {
                selectedSeed.Value = seed;
            }
        }

        private void OnToolChosen(int toolIndex)
        {
            if (toolSwitch != null)
            {
                toolSwitch.SelectTool(toolIndex);
            }
        }

        private void OnSeedChanged(SeedDefinition seed) => RefreshSelection();

        private void OnToolChanged(int toolIndex, GameObject tool) => RefreshSelection();
    }
}
