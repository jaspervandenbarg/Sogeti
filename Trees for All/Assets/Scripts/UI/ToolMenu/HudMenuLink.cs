using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Sogeti.UI.ToolMenu
{
    /// <summary>
    /// Keeps the wrist to one panel at a time.
    /// The HUD and the tool menu share the same spot, so opening the menu hides
    /// the HUD and closing it brings the HUD back.
    /// It must not live on the HUD root it toggles. SetActive(false) would
    /// unregister this, and the close write would then find no listener to bring
    /// the HUD back. Any object the gate leaves alone will do.
    /// </summary>
    [DisallowMultipleComponent]
    public class HudMenuLink : MonoBehaviour
    {
        [Tooltip("True while the wrist menu is open. The tool menu writes it.")]
        [SerializeField]
        private BoolVariable toolMenuOpen;

        [Tooltip("The HUD root. It hides while the tool menu is open.")]
        [SerializeField]
        private GameObject hudRoot;

        private void OnEnable()
        {
            if (toolMenuOpen == null)
            {
                return;
            }

            toolMenuOpen.Changed.Register(OnToolMenuOpenChanged);

            // Nobody raises Changed for the starting state, only the Initial Value
            // sets it, so a direct read is the only way to sync before the first open.
            OnToolMenuOpenChanged(toolMenuOpen.Value);
        }

        private void OnDisable()
        {
            if (toolMenuOpen != null)
            {
                toolMenuOpen.Changed.Unregister(OnToolMenuOpenChanged);
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
