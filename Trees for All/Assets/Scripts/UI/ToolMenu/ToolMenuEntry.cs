using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Sogeti.UI.ToolMenu
{
    /// <summary>
    /// One icon button in the menu.
    /// It knows no seed and no tool. The row that owns it decides what a click
    /// means, so the same prefab serves both rows.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ToolMenuEntry : MonoBehaviour
    {
        [SerializeField]
        private Button button;
        [SerializeField]
        private Image icon;
        [SerializeField]
        private TMP_Text label;
        [Tooltip("Drawn while this entry is the selected one.")]
        [SerializeField]
        private GameObject selectedMark;

        public event Action<ToolMenuEntry> Clicked;

        public void SetContent(Sprite iconSprite, string text)
        {
            if (icon != null)
            {
                icon.sprite = iconSprite;

                // An unset sprite draws a white box, which reads as a broken button.
                icon.enabled = iconSprite != null;
            }

            if (label != null)
            {
                label.text = text;
            }

            name = $"Entry {text}";
        }

        public void SetSelected(bool selected)
        {
            if (selectedMark != null)
            {
                selectedMark.SetActive(selected);
            }
        }

        private void Awake()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            SetSelected(false);
        }

        private void OnEnable() => button.onClick.AddListener(RaiseClicked);

        private void OnDisable() => button.onClick.RemoveListener(RaiseClicked);

        private void RaiseClicked() => Clicked?.Invoke(this);
    }
}
