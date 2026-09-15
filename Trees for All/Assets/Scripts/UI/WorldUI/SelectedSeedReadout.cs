using Sogeti.Atoms;
using Sogeti.Planting;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Sogeti.UI.WorldUI
{
    /// <summary>
    /// Names the seed in the right hand.
    /// It sits under the planter, so it leaves with it when the can comes out and
    /// while the menu stows the hand.
    /// </summary>
    [DisallowMultipleComponent]
    public class SelectedSeedReadout : MonoBehaviour
    {
        [SerializeField]
        private SeedDefinitionVariable selectedSeed;
        [SerializeField]
        private Image icon;
        [SerializeField]
        private TMP_Text label;

        private void OnEnable()
        {
            if (selectedSeed == null)
            {
                Debug.LogError($"{name}: no selected seed Variable assigned, the readout stays empty.", this);
                return;
            }

            selectedSeed.Changed.Register(Show);

            // Nobody raises Changed for the starting seed, only the Initial Value sets
            // it, so a direct read is the only way to see it before the first pick.
            Show(selectedSeed.Value);
        }

        private void OnDisable()
        {
            if (selectedSeed != null)
            {
                selectedSeed.Changed.Unregister(Show);
            }
        }

        private void Show(SeedDefinition seed)
        {
            if (label != null)
            {
                label.text = seed != null ? seed.DisplayName : string.Empty;
            }

            if (icon != null)
            {
                icon.sprite = seed != null ? seed.MenuIcon : null;
                icon.enabled = icon.sprite != null;
            }
        }
    }
}
