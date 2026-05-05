using System.Collections.Generic;
using UnityEngine;

namespace _Teams.World_Design.Scripts.Challenges.Ghost_Boat
{
    [RequireComponent(typeof(GhostBoatAudio))]
    public class GhostBoatScript : MonoBehaviour
    {
        [Header("Follow Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -5f);
        [SerializeField, Min(0f)] private float followSpeed = 0.1f;

        [Header("View detection")]
        [SerializeField] private Camera playerCamera;
        [SerializeField, Range(0.01f, 0.5f)] private float centerTolerance = 0.1f;

        [Header("Optional Rotation")]
        [SerializeField] private bool faceTarget;

        [Header("Shader Adjustments")]
        [SerializeField] private Material inViewMaterial;
        [Tooltip("Leave empty to automatically find all renderers on this boat and its children")]
        [SerializeField] private Renderer[] targetRenderers;
        
        private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

        private GhostBoatAudio ghostBoatAudio;
        private bool wasInView = false;

        private void Start()
        {
            ghostBoatAudio = GetComponent<GhostBoatAudio>();
            if (ghostBoatAudio == null)
            {
                Debug.LogWarning(gameObject.name + ": GhostBoatAudio not found in scene (check prefab)");
            }

            // Save original materials to switch materials without loosing original material
            if (inViewMaterial != null)
            {
                SetOriginalRenderer();
            }

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
            bool isCentered = IsInCenterView();

            // only update audio if in view has changed since last frame
            if (isCentered != wasInView)
            {
                if (ghostBoatAudio)
                {
                    ghostBoatAudio.SetInView(isCentered);
                }

                if (inViewMaterial)
                {
                    ApplyMaterial(isCentered);
                }
            }
            wasInView = isCentered;

            if (!target)
            {
                return;
            }

            if (!isCentered)
            {
                MoveToTarget();
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

        private void ApplyMaterial(bool isCentered)
        {
            foreach (var (key, value) in originalMaterials)
            {
                if (!key) continue;
                        
                if (isCentered)
                {
                    Material[] tempMats = new Material[value.Length];
                    for (int i = 0; i < tempMats.Length; i++)
                    {
                        tempMats[i] = inViewMaterial;
                    }
                    key.sharedMaterials = tempMats;
                }
                else
                {
                    key.sharedMaterials = value;
                }
            }
        }

        private void MoveToTarget()
        {
            Vector3 desiredPosition = target.position + offset;
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
                    originalMaterials[rend] = rend.sharedMaterials;
                }
            }
        }
    }
}