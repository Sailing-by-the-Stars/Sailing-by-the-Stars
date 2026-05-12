using System.Collections.Generic;
using UnityEngine;

namespace _Teams.World_Design.Scripts.Challenges.Lost_Souls
{
    [RequireComponent(typeof(GhostBoatAudio))]
    public class LostSoulFollower : MonoBehaviour
    {
        [Header("Follow Target")]
        [SerializeField] private Transform target;
        [Tooltip("Distance to the left side of the target.")]
        [SerializeField] private float sideDistance = 20f;
        [SerializeField, Min(0f)] private float followSpeed = 0.1f;

        [Header("View detection")]
        [SerializeField] private Camera playerCamera;
        [SerializeField, Range(0.01f, 0.5f)] private float centerTolerance = 0.1f;

        [Header("Optional Rotation")]
        [SerializeField] private bool faceTarget;

        [Header("Opacity Settings")]
        [SerializeField] private float maxOpacity = 1f;
        [SerializeField] private float minOpacity = 0.3f;
        [SerializeField] private float opacityFadeSpeed = 5f;

        [Header("Shader Adjustments")]
        [Tooltip("Leave empty to automatically find all renderers on this boat and its children")]
        [SerializeField] private Renderer[] targetRenderers;
        
        [Header("Lights")]
        [Tooltip("List of lights to fade in/out with the boat")]
        [SerializeField] private List<Light> targetLights = new List<Light>();

        private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
        private Dictionary<Light, float> originalLightIntensities = new Dictionary<Light, float>();

        private GhostBoatAudio ghostBoatAudio;
        private bool wasInView = false;
        private float currentOpacity;

        private static readonly int OpacityID = Shader.PropertyToID("Opacity");
        private static readonly int UnderOpacityID = Shader.PropertyToID("_Opacity");

        private void Start()
        {
            ghostBoatAudio = GetComponent<GhostBoatAudio>();
            if (ghostBoatAudio == null)
            {
                Debug.LogWarning(gameObject.name + ": GhostBoatAudio not found in scene (check prefab)");
            }

            SetOriginalRenderer();
            
            foreach (Light tLight in targetLights)
            {
                if (tLight != null)
                {
                    originalLightIntensities[tLight] = tLight.intensity;
                }
            }

            currentOpacity = maxOpacity;
            UpdateOpacity(currentOpacity);

            // Check initial view state on spawn
            bool initiallyInView = IsInCenterView();
            if (ghostBoatAudio != null)
            {
                ghostBoatAudio.SetInView(initiallyInView);
            }
            wasInView = initiallyInView;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        private void LateUpdate()
        {
            MoveToTarget();
            bool isCentered = IsInCenterView();

            float targetOpacity = isCentered ? minOpacity : maxOpacity;
            currentOpacity = Mathf.Lerp(currentOpacity, targetOpacity, Time.deltaTime * opacityFadeSpeed);
            UpdateOpacity(currentOpacity);

            // only update audio if in view has changed since last frame
            if (isCentered != wasInView)
            {
                if (ghostBoatAudio)
                {
                    ghostBoatAudio.SetInView(isCentered);
                }
            }
            wasInView = isCentered;

            if (!target)
            {
                return;
            }

            if (!isCentered)
            {
            }
            
            if (faceTarget)
            {
                Vector3 lookDirection = target.position - transform.position;
                if (lookDirection.sqrMagnitude > 0.0001f)
                {
                    transform.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
                }
            }
        }

        private void UpdateOpacity(float opacity)
        {
            foreach (var kvp in originalMaterials)
            {
                Renderer key = kvp.Key;
                Material[] cachedMaterials = kvp.Value; 

                if (!key) continue;
        
                foreach (Material mat in cachedMaterials)
                {
                    if (mat.HasProperty(OpacityID))
                    {
                        mat.SetFloat(OpacityID, opacity);
                    }
                    else if (mat.HasProperty(UnderOpacityID))
                    {
                        mat.SetFloat(UnderOpacityID, opacity);
                    }
                }
            }

            float normalizedOpacity = maxOpacity > 0f ? Mathf.Clamp01(opacity / maxOpacity) : 0f;
            foreach (var kvp in originalLightIntensities)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.intensity = kvp.Value * normalizedOpacity;
                }
            }
        }

        private void MoveToTarget()
        {
            if (target == null) return;
            
            // Calculate desired position to be strictly on the left side (-right) of the target based on its rotation
            Vector3 desiredPosition = target.position - target.right * sideDistance;
            desiredPosition.y = 0f; // Keep on ground level, adjust if needed for your game
            float speed = followSpeed <= 0f ? 1f : followSpeed * Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, speed);
        }

        private bool IsInCenterView()
        {
            Camera cam = playerCamera ? playerCamera : Camera.main;
            if (!cam)
            {
                return false;
            }

            Vector3 viewportPoint = cam.WorldToViewportPoint(transform.position);

            if (viewportPoint.z <= 0f)
            {
                return false;
            }

            float dx = Mathf.Abs(viewportPoint.x - 0.5f);
            float dy = Mathf.Abs(viewportPoint.y - 0.5f);

            return dx <= centerTolerance && dy <= centerTolerance;
        }

        private void OnDestroy()
        {
            if (ghostBoatAudio != null)
            {
                ghostBoatAudio.ResetAudio();
            }
        }
        
        private void SetOriginalRenderer()
        {
            Renderer[] renderersToProcess = targetRenderers != null && targetRenderers.Length > 0 
                ? targetRenderers 
                : GetComponentsInChildren<Renderer>(true);

            foreach (Renderer rend in renderersToProcess)
            {
                if (rend != null)
                {
                    originalMaterials[rend] = rend.materials; // Use .materials to get instances, same as LostSoulBoatScript
                }
            }
        }
    }
}