using Sogeti.Interaction;
using UnityEngine;

namespace Sogeti.UI.WorldUI
{
    /// <summary>
    /// The water level printed on the watering can.
    /// The can is fixed to the hand, so this needs no billboard and no distance
    /// gate. It reads WateringCan.Fill01 and never owns the number.
    /// </summary>
    [DisallowMultipleComponent]
    public class WateringCanLevelMeter : MonoBehaviour
    {
        [SerializeField]
        private WateringCan can;

        [SerializeField]
        [Tooltip("Sits at the left edge of the bar. Scaling it on X drains the fill to the left.")]
        private Transform fillPivot;

        [SerializeField]
        private Renderer fillRenderer;

        [Header("Urgency")]
        [SerializeField]
        private Material fullMaterial;

        [SerializeField]
        [Tooltip("Shown once the can holds less than the low fraction.")]
        private Material lowMaterial;

        [SerializeField]
        [Range(0f, 1f)]
        private float lowFraction = 0.25f;

        private Vector3 fillBaseScale = Vector3.one;
        private bool isLow;
        private bool hasLevelState;

        private void Awake()
        {
            if (can == null)
            {
                can = GetComponentInParent<WateringCan>();
            }

            if (fillPivot != null)
            {
                fillBaseScale = fillPivot.localScale;
            }
        }

        private void Update()
        {
            if (can == null)
            {
                return;
            }

            float fill = Mathf.Clamp01(can.Fill01);

            if (fillPivot != null)
            {
                Vector3 scale = fillBaseScale;
                scale.x = fillBaseScale.x * fill;
                fillPivot.localScale = scale;
            }

            ApplyLevelColour(fill < lowFraction);
        }

        private void ApplyLevelColour(bool low)
        {
            if (hasLevelState && low == isLow)
            {
                return;
            }

            isLow = low;
            hasLevelState = true;

            Material material = low ? lowMaterial : fullMaterial;
            if (fillRenderer != null && material != null)
            {
                // sharedMaterial, not material. An instance leaks one material per can.
                fillRenderer.sharedMaterial = material;
            }
        }
    }
}
