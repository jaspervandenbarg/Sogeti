using UnityEngine;

namespace Sogeti.UI.WorldUI
{
    public class WorldUIScaleAtDistance : MonoBehaviour
    {
        [SerializeField]
        private float distanceScaleOffset = 0f;
        [SerializeField]
        private float scaleFactor = 1f;

        private float baseScale = 0f;

        private void Awake()
        {
            baseScale = transform.localScale.x;
        }

        private float CalculateDistance(Vector3 distance) => Vector3.Distance(transform.position, distance);

        /// <summary>
        /// Scale the UI based on the distance to the player using a scale factor
        /// The further the player the larger the scale
        /// In hopes of keeping important UI visible through the world
        /// </summary>
        /// <param name="distance"></param>
        public void SetScaleAtDistance(Vector3 distance)
        {
            // log scale factor
            float distanceValue = Mathf.Max(CalculateDistance(distance), 1);
            float calculatedScale = Mathf.Log(distanceValue + 1) * scaleFactor + distanceScaleOffset;
            float scale = Mathf.Max(calculatedScale, baseScale);
            transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}
