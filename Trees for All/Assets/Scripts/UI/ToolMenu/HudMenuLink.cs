using UnityEngine;

namespace Sogeti.UI.ToolMenu
{
    /// <summary>
    /// Keeps the wrist to one panel at a time.
    /// The HUD and the tool menu share the same spot, so opening the menu hides
    /// the HUD and closing it brings the HUD back. Lives on the ToolMenu object,
    /// which the gate never deactivates, so the subscription survives every open
    /// and close.
    /// </summary>
    [DisallowMultipleComponent]
    public class HudMenuLink : MonoBehaviour
    {
        [Tooltip("The HUD root. It hides while the tool menu is open.")]
        [SerializeField]
        private GameObject hudRoot;

        private ToolMenuController toolMenu;

        private void Awake() => toolMenu = GetComponent<ToolMenuController>();

        private void OnEnable()
        {
            if (toolMenu == null)
            {
                return;
            }

            toolMenu.OpenChanged += OnToolMenuOpenChanged;
        }

        private void OnDisable()
        {
            if (toolMenu != null)
            {
                toolMenu.OpenChanged -= OnToolMenuOpenChanged;
            }
        }

        private void OnToolMenuOpenChanged(bool open)
        {
            if (hudRoot != null)
            {
                hudRoot.SetActive(!open);
            }
        }
    }
}
