using System.Collections.Generic;
using Sogeti.Planting;
using TMPro;
using UnityEngine;

namespace Sogeti.Interaction
{
    /// <summary>
    /// The ghost plant the player sees before planting.
    /// Green means the spot is legal, red means it is refused, and the label names
    /// the reason. A colour alone does not teach the spacing rule to a new player.
    /// </summary>
    public class PlantingPreview : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [SerializeField]
        private Transform ghostAnchor;
        [SerializeField]
        private TMP_Text reasonLabel;
        [SerializeField]
        private Transform labelAnchor;
        [SerializeField]
        private Color validTint = new Color(0.35f, 1f, 0.45f, 1f);
        [SerializeField]
        private Color blockedTint = new Color(1f, 0.35f, 0.3f, 1f);

        private readonly List<Renderer> ghostRenderers = new List<Renderer>();

        private MaterialPropertyBlock propertyBlock;
        private SeedDefinition ghostSeed;
        private GameObject ghostVisual;
        private Transform playerHead;
        private bool lastTintWasValid;
        private bool hasTint;

        private void Awake()
        {
            EnsureInitialized();
            Hide();
        }

        // Show can arrive before Awake, because Hide leaves this object inactive.
        private void EnsureInitialized()
        {
            if (propertyBlock != null)
            {
                return;
            }

            propertyBlock = new MaterialPropertyBlock();
            if (ghostAnchor == null)
            {
                ghostAnchor = transform;
            }
        }

        /// <summary>Places the ghost on the candidate spot and colours it by the verdict.</summary>
        public void Show(SeedDefinition seed, in PlacementResult result)
        {
            EnsureInitialized();

            if (seed == null || !result.HasSurface)
            {
                Hide();
                return;
            }

            BuildGhost(seed);

            ghostAnchor.position = result.Position;

            // The preview hangs off the hand, so it has to hold its own upright pose.
            ghostAnchor.rotation = Quaternion.identity;
            gameObject.SetActive(true);

            ApplyTint(result.IsValid);
            ShowLabel(PlacementReason.Describe(result));
        }

        public void Hide()
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        // The ghost is rebuilt only when the player picks another seed, not every frame.
        private void BuildGhost(SeedDefinition seed)
        {
            if (ghostSeed == seed && ghostVisual != null)
            {
                return;
            }

            if (ghostVisual != null)
            {
                Destroy(ghostVisual);
            }

            ghostSeed = seed;
            ghostRenderers.Clear();
            hasTint = false;

            GrowthStage firstStage = seed.GetStage(0);
            if (firstStage == null || firstStage.VisualPrefab == null)
            {
                ghostVisual = null;
                return;
            }

            ghostVisual = Instantiate(firstStage.VisualPrefab, ghostAnchor);
            ghostVisual.transform.localPosition = Vector3.zero;
            ghostVisual.transform.localRotation = Quaternion.identity;
            ghostVisual.transform.localScale = Vector3.one * firstStage.VisualScale;

            StripColliders(ghostVisual);
            ghostVisual.GetComponentsInChildren(true, ghostRenderers);
        }

        // A preview must never block the ray that is measuring the spot under it.
        private static void StripColliders(GameObject root)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Destroy(colliders[i]);
            }
        }

        private void ApplyTint(bool isValid)
        {
            if (hasTint && lastTintWasValid == isValid)
            {
                return;
            }

            Color tint = isValid ? validTint : blockedTint;
            propertyBlock.SetColor(BaseColorId, tint);
            propertyBlock.SetColor(ColorId, tint);

            for (int i = 0; i < ghostRenderers.Count; i++)
            {
                if (ghostRenderers[i] != null)
                {
                    ghostRenderers[i].SetPropertyBlock(propertyBlock);
                }
            }

            lastTintWasValid = isValid;
            hasTint = true;
        }

        private void ShowLabel(string text)
        {
            if (reasonLabel == null)
            {
                return;
            }

            bool hasText = !string.IsNullOrEmpty(text);
            if (reasonLabel.gameObject.activeSelf != hasText)
            {
                reasonLabel.gameObject.SetActive(hasText);
            }

            if (!hasText)
            {
                return;
            }

            reasonLabel.text = text;
            FaceThePlayer();
        }

        // Yaw only. Tilting world space text towards the headset reads as unstable in VR.
        private void FaceThePlayer()
        {
            Transform anchor = labelAnchor != null ? labelAnchor : reasonLabel.transform;
            Transform head = ResolvePlayerHead();
            if (head == null)
            {
                return;
            }

            Vector3 direction = anchor.position - head.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            anchor.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        private Transform ResolvePlayerHead()
        {
            if (playerHead != null)
            {
                return playerHead;
            }

            UnityEngine.Camera main = UnityEngine.Camera.main;
            playerHead = main == null ? null : main.transform;
            return playerHead;
        }
    }
}
