using UnityEngine;
using UnityAtoms.BaseAtoms;

namespace Sogeti.Camera
{
    public class ShareCameraPosition : MonoBehaviour
    {
        [SerializeField]
        private Vector3Variable cameraPosition;

        private void Update()
        {
            if (cameraPosition != null)
            {
                cameraPosition.Value = transform.position;
            }
        }
    }
}

