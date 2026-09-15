using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Sogeti.UI.WorldUI
{
    /// <summary>
    /// The water level printed on the watering can.
    /// The can is fixed to the hand, so this needs no billboard and no distance
    /// gate. It reads the fill Variable and never owns the number. A still can
    /// writes the same value, which Atoms drops, so the bar costs nothing then.
    /// That same drop means the can's first write may raise nothing if it
    /// already matches the Initial Value, so this also reads directly on enable.
    /// </summary>
    [DisallowMultipleComponent]
    public class WateringCanLevelMeter : MonoBehaviour
    {
        [SerializeField]
        private FloatVariable fill01;

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
            if (fillPivot != null)
            {
                fillBaseScale = fillPivot.localScale;
            }
        }

        private void OnEnable()
        {
            if (fill01 == null)
            {
                return;
            }

            fill01.Changed.Register(Show);
            Show(fill01.Value);
        }

        private void OnDisable()
        {
            if (fill01 != null)
            {
                fill01.Changed.Unregister(Show);
            }
        }

        private void Show(float level)
        {
            float fill = Mathf.Clamp01(level);

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
