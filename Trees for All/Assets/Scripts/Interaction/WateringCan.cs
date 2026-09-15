using Sogeti.Planting;
using Sogeti.Watering;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Sogeti.Interaction
{
    /// <summary>
    /// The watering can in the right hand.
    /// The player tilts it to pour and dips the spout in the pond to refill. It
    /// only ever calls Plant.ApplyWaterFlow, so the seed keeps owning its fill
    /// rate and one can cannot change how fast every seed type grows.
    /// </summary>
    [DisallowMultipleComponent]
    public class WateringCan : MonoBehaviour
    {
        // A 3 m sphere at the tightest spacing holds a handful of plants. The cap
        // only guards against a player standing in a dense grove.
        private const int MaxPlantsInRange = 16;

        [Header("Spout")]
        [SerializeField]
        [Tooltip("The mouth of the can. The pour test and the refill test both run from here.")]
        private Transform spout;

        [SerializeField]
        [Tooltip("How far from the spout a plant still catches the water.")]
        private float pourRadius = 0.6f;

        [Header("Masks")]
        [SerializeField]
        [Tooltip("Layers that carry planted plants. A dead plant moves off this layer, so it is skipped for free.")]
        private LayerMask plantMask;

        [SerializeField]
        [Tooltip("The pond. Its collider is a trigger, so this query asks for triggers.")]
        private LayerMask waterMask;

        [SerializeField]
        private float refillRadius = 0.25f;

        [Header("Pouring")]
        [SerializeField]
        [Tooltip("Its up axis must point out of the top of the can. The imported mesh carries a baked rotation, so the root axes may not match the model.")]
        private Transform tiltReference;

        [SerializeField]
        private float pourStartAngle = PourFlow.DefaultStartDegrees;

        [SerializeField]
        private float pourFullAngle = PourFlow.DefaultFullDegrees;

        [SerializeField]
        [Tooltip("Tick to also require the trigger while tilting. Off means the tilt alone pours.")]
        private bool requireTriggerToPour;

        [SerializeField]
        [Tooltip("Only read while the trigger is required. This is the same action that plants a seed.")]
        private InputActionProperty pourAction;

        [Header("Tank")]
        [SerializeField]
        [Tooltip("Seconds of pouring at full tilt before the can runs dry.")]
        private float capacitySeconds = 10f;

        [SerializeField]
        [Tooltip("Meter units per second while the spout sits in the pond.")]
        private float refillPerSecond = 0.5f;

        [Header("Feedback")]
        [SerializeField]
        private ParticleSystem stream;

        private readonly Collider[] plantBuffer = new Collider[MaxPlantsInRange];
        private WaterTank tank;
        private bool isStreaming;
        private bool hasStreamState;

        /// <summary>How much water is left, 0 to 1. The level bar reads this.</summary>
        public float Fill01 => tank == null ? 0f : tank.Fill01;

        public bool IsEmpty => tank == null || tank.IsEmpty;

        /// <summary>The flow the can served this frame. Zero while upright or dry.</summary>
        public float CurrentFlow01 { get; private set; }

        public Plant CurrentTarget { get; private set; }

        private void Awake()
        {
            if (spout == null)
            {
                spout = transform;
            }

            if (tiltReference == null)
            {
                tiltReference = transform;
            }

            tank = new WaterTank(capacitySeconds);
        }

        private void OnEnable()
        {
            InputAction action = pourAction.action;
            if (action != null)
            {
                action.Enable();
            }

            SetStreaming(false);
        }

        private void OnDisable()
        {
            // A shared action reference belongs to the rig. Only a locally owned action is safe to disable.
            InputAction action = pourAction.action;
            if (action != null && pourAction.reference == null)
            {
                action.Disable();
            }

            CurrentFlow01 = 0f;
            CurrentTarget = null;
            SetStreaming(false);
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            CurrentFlow01 = tank.Drain(RequestedFlow(), deltaTime);
            SetStreaming(CurrentFlow01 > 0f);

            CurrentTarget = CurrentFlow01 > 0f ? FindThirstiestPlantInRange() : null;
            if (CurrentTarget != null)
            {
                CurrentTarget.ApplyWaterFlow(CurrentFlow01);
            }

            if (IsSpoutInWater())
            {
                tank.Refill(refillPerSecond, deltaTime);
            }
        }

        /// <summary>The tilt the player asks for, before the tank decides what it can serve.</summary>
        private float RequestedFlow()
        {
            if (requireTriggerToPour && !IsTriggerHeld())
            {
                return 0f;
            }

            // The can's own up axis, so the reading follows the wrist and not the head.
            float tilt = Vector3.Angle(tiltReference.up, Vector3.up);
            return PourFlow.For(tilt, pourStartAngle, pourFullAngle);
        }

        // A pour is a held state, not an event, so this polls instead of subscribing.
        private bool IsTriggerHeld()
        {
            InputAction action = pourAction.action;
            return action != null && action.IsPressed();
        }

        /// <summary>
        /// One plant per frame, not every plant in the sphere.
        /// The target player has little game experience, and one clear effect teaches
        /// the rule faster than a sprinkler. A fully grown tree is skipped, so it
        /// cannot shadow the seedling beside it.
        /// </summary>
        private Plant FindThirstiestPlantInRange()
        {
            int found = Physics.OverlapSphereNonAlloc(
                spout.position,
                pourRadius,
                plantBuffer,
                plantMask,
                QueryTriggerInteraction.Ignore);

            Plant nearest = null;
            float nearestSqrDistance = float.MaxValue;

            for (int i = 0; i < found; i++)
            {
                Collider collider = plantBuffer[i];
                if (collider == null)
                {
                    continue;
                }

                Plant plant = collider.GetComponentInParent<Plant>();
                if (plant == null || !plant.HasWaterMeter)
                {
                    continue;
                }

                float sqrDistance = (plant.transform.position - spout.position).sqrMagnitude;
                if (sqrDistance < nearestSqrDistance)
                {
                    nearest = plant;
                    nearestSqrDistance = sqrDistance;
                }
            }

            return nearest;
        }

        /// <summary>
        /// A hand held can has no Rigidbody, so OnTriggerStay would never fire.
        /// CheckSphere asks the pond instead, and it allocates nothing.
        /// </summary>
        private bool IsSpoutInWater()
        {
            return Physics.CheckSphere(
                spout.position,
                refillRadius,
                waterMask,
                QueryTriggerInteraction.Collide);
        }

        // Driven by the served flow, not by the tilt. An empty can that still sprays reads as a bug.
        private void SetStreaming(bool streaming)
        {
            if (stream == null || (hasStreamState && isStreaming == streaming))
            {
                return;
            }

            isStreaming = streaming;
            hasStreamState = true;

            if (streaming)
            {
                stream.Play();
            }
            else
            {
                stream.Stop();
            }
        }

        private void OnValidate()
        {
            pourRadius = Mathf.Max(pourRadius, 0.05f);
            refillRadius = Mathf.Max(refillRadius, 0.05f);
            capacitySeconds = Mathf.Max(capacitySeconds, 0.1f);
            refillPerSecond = Mathf.Max(refillPerSecond, 0f);
            pourStartAngle = Mathf.Clamp(pourStartAngle, 0f, 180f);
            pourFullAngle = Mathf.Clamp(pourFullAngle, 0f, 180f);
        }
    }
}
