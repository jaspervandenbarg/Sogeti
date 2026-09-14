using UnityEngine;

namespace Sogeti.UI.WorldUI
{
    public class WorldUILookAtPlayer : MonoBehaviour
    {
        [SerializeField]
        private Vector3 rotationOffset;

        public void LookAtPlayer(Vector3 position)
        {
            // just rotate the y axis towards the player
            Vector3 direction = position - transform.position;
            direction.y = 0; // keep only the horizontal direction

            // A player standing on top of this object gives a zero vector, and LookAt then warns every frame.
            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            position = transform.position + direction;
            transform.LookAt(position);
            transform.rotation *= Quaternion.Euler(rotationOffset);
        }
    }

}
