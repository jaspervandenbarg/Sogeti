using UnityEngine;

namespace Sogeti.UI.WorldUI
{
    public class WorldUIAtEyeHeight : MonoBehaviour
    {
        [SerializeField]
        private float eyeHeightOffset = 0f;

        public void SetEyeHeight(float eyeHeight)
        {
            transform.position = new Vector3(transform.position.x, eyeHeight + eyeHeightOffset, transform.position.z);
        }
    }
}

