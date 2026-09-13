using UnityEngine;

namespace Sogeti.UI.WorldUI
{
    public class WorldUIHeightAtDistance : MonoBehaviour
    {
        [SerializeField]
        private float distanceHeightOffset = 0f;
        [SerializeField]
        private float scaleFactor = 1f;

        private float baseHeight = 0f;

        private void Awake()
        {
            baseHeight = transform.position.y;
        }

        private float CalculateDistance(Vector3 distance) => Vector3.Distance(transform.position, distance);

        /// <summary>
        /// Scale the height based on the distance to the player using a scale factor
        /// The further the player the heigher the elevation
        /// In hopes of keeping important UI visible through the world
        /// </summary>
        /// <param name="distance"></param>
        public void SetHeightAtDistance(Vector3 distance)
        {
            // log scale factor
            float distanceValue = Mathf.Max(CalculateDistance(distance), 1);
            float heightOffset = Mathf.Log(distanceValue + 1) * scaleFactor + distanceHeightOffset;
            transform.position = new Vector3(transform.position.x, baseHeight + heightOffset, transform.position.z);
        }
    }
}
