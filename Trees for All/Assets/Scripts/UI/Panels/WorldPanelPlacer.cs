using System;
using Sogeti.UI.WorldUI;
using UnityEngine;

namespace Sogeti.UI.Panels
{
    /// <summary>
    /// Drops a panel in front of the player once, then leaves it in the world.
    /// A panel that follows the head is a helmet. One that stays put is a sign the
    /// player can step away from and look back at.
    /// </summary>
    [DisallowMultipleComponent]
    public class WorldPanelPlacer : MonoBehaviour
    {
        [SerializeField]
        private float distance = 2f;

        [Tooltip("Negative sits the panel centre below eye level, the resting gaze angle.")]
        [SerializeField]
        private float heightOffset = -0.1f;

        [Tooltip("Turns the placed panel to face the player. Yaw only, so it never tips.")]
        [SerializeField]
        private WorldUILookAtPlayer facing;

        private Transform head;
        private int requestedOnFrame = -1;

        /// <summary>Raised on the frame the panel lands, so the caller can reveal it.</summary>
        public event Action Placed;

        public bool HasPlaced { get; private set; }

        /// <summary>
        /// Asks for a placement in front of the head.
        /// The pose is not settled on the frame a scene loads, so the placement waits
        /// for the next one. PlantWaterMeter skips a frame for the same reason.
        /// </summary>
        public void RequestPlacement(Transform playerHead)
        {
            head = playerHead;
            HasPlaced = false;
            requestedOnFrame = Time.frameCount;
            enabled = true;
        }

        private void LateUpdate()
        {
            if (HasPlaced || head == null || Time.frameCount <= requestedOnFrame)
            {
                return;
            }

            Place();
        }

        private void Place()
        {
            Vector3 forward = head.forward;
            forward.y = 0f;

            // A player looking straight down gives no heading. Keep the last one.
            if (forward.sqrMagnitude < 0.0001f)
            {
                forward = transform.forward;
                forward.y = 0f;
            }

            Vector3 target = head.position + forward.normalized * distance;
            target.y = head.position.y + heightOffset;
            transform.position = target;

            if (facing != null)
            {
                facing.LookAtPlayer(head.position);
            }

            HasPlaced = true;

            // Nothing left to watch. The Quest 3 is mobile hardware.
            enabled = false;
            Placed?.Invoke();
        }

        private void OnValidate()
        {
            distance = Mathf.Max(distance, 0.5f);
        }
    }
}
