using Sogeti.Interaction;
using Sogeti.Planting;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Sogeti.UI.WorldUI
{
    /// <summary>
    /// Names the seed in the right hand.
    /// It sits under the planter, so it leaves with it when the can comes out and
    /// while the menu stows the hand. The panel names the seed in both cases.
    /// </summary>
    [DisallowMultipleComponent]
    public class SelectedSeedReadout : MonoBehaviour
    {
        [SerializeField]
        private SeedPlanter planter;
        [SerializeField]
        private Image icon;
        [SerializeField]
        private TMP_Text label;

        private void OnEnable()
        {
            if (planter == null)
            {
                Debug.LogError($"{name}: no planter assigned, the readout stays empty.", this);
                return;
            }

            planter.SeedChanged += Show;

            // The readout sleeps while the hand is stowed, so it misses every change it was away for.
            Show(planter.SelectedSeed);
        }

        private void OnDisable()
        {
            if (planter != null)
            {
                planter.SeedChanged -= Show;
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
