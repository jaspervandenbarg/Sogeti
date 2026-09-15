using Sogeti.Interaction;
using Sogeti.UI.ToolMenu;
using UnityEngine;

namespace Sogeti.Session
{
    /// <summary>
    /// Turns every player ability off while a panel owns the view, then back on.
    /// The arrays are generic on purpose. Naming ControllerInputActionManager here
    /// would tie gameplay code to a read-only XRI sample, and toggling enabled or
    /// SetActive is all this needs.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerActionGate : MonoBehaviour
    {
        [Tooltip("Stows the planter and the watering can. The right trigger then drives UI only.")]
        [SerializeField]
        private RightHandToolSwitch toolSwitch;

        [Tooltip("The wrist menu. It closes first, because closing un-stows the hand.")]
        [SerializeField]
        private ToolMenuController toolMenu;

        [Tooltip("Left Controller Input Action Manager, plus the Move and Turn providers.")]
        [SerializeField]
        private Behaviour[] behavioursToSuspend = new Behaviour[0];

        [Tooltip("The left Teleport Interactor and the HUD root.")]
        [SerializeField]
        private GameObject[] objectsToDeactivate = new GameObject[0];

        public bool PlayerActionsEnabled { get; private set; } = true;

        /// <summary>
        /// Order matters in both directions.
        /// ToolMenuController.SetOpen calls SetToolsStowed(open), so Close un-stows
        /// the hand. Closing after the stow would hand the player a live planter
        /// behind the panel.
        /// </summary>
        public void SetPlayerActionsEnabled(bool enable)
        {
            PlayerActionsEnabled = enable;

            if (enable)
            {
                // Objects first. ControllerInputActionManager.OnEnable deactivates the
                // Teleport Interactor, so enabling it last leaves the arc correctly off.
                // The reverse order would revive a live interactor with nothing driving it.
                SetObjectsActive(true);
                SetBehavioursEnabled(true);

                if (toolMenu != null)
                {
                    toolMenu.enabled = true;
                }

                if (toolSwitch != null)
                {
                    toolSwitch.SetToolsStowed(false);
                }

                return;
            }

            if (toolMenu != null)
            {
                toolMenu.Close();
                toolMenu.enabled = false;
            }

            if (toolSwitch != null)
            {
                toolSwitch.SetToolsStowed(true);
            }

            SetBehavioursEnabled(false);
            SetObjectsActive(false);
        }

        private void SetBehavioursEnabled(bool enable)
        {
            for (int i = 0; i < behavioursToSuspend.Length; i++)
            {
                if (behavioursToSuspend[i] != null)
                {
                    behavioursToSuspend[i].enabled = enable;
                }
            }
        }

        // Deactivating the Teleport Interactor clears an arc left mid aim. Re-enabling
        // ControllerInputActionManager deactivates it again in OnEnable, so the
        // resume path never has to guess which state it was in.
        private void SetObjectsActive(bool active)
        {
            for (int i = 0; i < objectsToDeactivate.Length; i++)
            {
                if (objectsToDeactivate[i] != null)
                {
                    objectsToDeactivate[i].SetActive(active);
                }
            }
        }
    }
}
