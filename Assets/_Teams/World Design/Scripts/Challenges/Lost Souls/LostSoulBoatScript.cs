using System.Collections.Generic;
using UnityEngine;

namespace _Teams.World_Design.Scripts.Challenges.Lost_Souls
{
    [RequireComponent(typeof(GhostBoatAudio))]
    public class LostSoulBoatScript : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform playerTransform;

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 2f;
        
        [Header("Lifetime Settings")]
        [SerializeField] private float timeUntilFade = 15f;
        [SerializeField] private float fadeDuration = 3f;
        
        [Header("Opacity Distance Fading")]
        [SerializeField] private float fadeStartRange = 40f;
        [SerializeField] private float fadeEndRange = 10f;
        [SerializeField, Range(0f, 1f)] private float maxOpacity = 0.3f;

        [Header("Debug")]
        [SerializeField] private float distanceToPlayer;

        [Header("Shader Adjustments")]
        [Tooltip("Leave empty to automatically find all renderers on this boat and its children")]
        [SerializeField] private Renderer[] targetRenderers;
        
        [SerializeField] private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

        public System.Action<LostSoulBoatScript> OnFadeComplete;

        private GhostBoatAudio ghostBoatAudio;
        private bool isFadingOut;
        private float currentOpacity;

        private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorID = Shader.PropertyToID("_Color");

        private void Start()
        {
            originalMaterials = new Dictionary<Renderer, Material[]>();
            
            targetRenderers = targetRenderers != null && targetRenderers.Length > 0 
                ? targetRenderers 
                : GetComponentsInChildren<Renderer>(true);
            

            foreach (Renderer rend in targetRenderers)
            {
                if (rend != null)
                {
                    originalMaterials[rend] = rend.materials; 
                }
            }

            ghostBoatAudio = GetComponent<GhostBoatAudio>();
            if (ghostBoatAudio == null)
            {
                Debug.LogWarning(gameObject.name + ": GhostBoatAudio not found in scene (check prefab)");
            }

            if (playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                }
            }

            SetOriginalRenderer();
            currentOpacity = maxOpacity;
            UpdateOpacity(currentOpacity);
            
            Invoke(nameof(StartFadeAndDelete), timeUntilFade);
        }

        private void Update()
        {
            transform.position += transform.forward * (moveSpeed * Time.deltaTime);

            if (playerTransform != null && !isFadingOut)
            {
                float distance = Vector3.Distance(transform.position, playerTransform.position);
                distanceToPlayer = distance;
                
                if (distance < fadeStartRange)
                {
                    float range = fadeStartRange - fadeEndRange;
                    float currentVal = distance - fadeEndRange;
                    float normalized = Mathf.Clamp01(currentVal / range);
                    currentOpacity = normalized * maxOpacity;
                    
                    UpdateOpacity(currentOpacity);
                }
                else
                {
                    currentOpacity = maxOpacity;
                    UpdateOpacity(currentOpacity);
                }
            }
        }

        public void StartFadeAndDelete()
        {
            StartCoroutine(FadeAndDelete());
        }

        private System.Collections.IEnumerator FadeAndDelete()
        {
            isFadingOut = true;
            float elapsedTime = 0f;
            float startOpacity = currentOpacity;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                currentOpacity = Mathf.Lerp(startOpacity, 0f, elapsedTime / fadeDuration);
                UpdateOpacity(currentOpacity);
                yield return null;
            }

            OnFadeComplete?.Invoke(this);
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
                    if (mat.HasProperty(BaseColorID))
                    {
                        Color c = mat.GetColor(BaseColorID);
                        c.a = opacity;
                        mat.SetColor(BaseColorID, c);
                    }
                    else if (mat.HasProperty(ColorID))
                    {
                        Color c = mat.GetColor(ColorID);
                        c.a = opacity;
                        mat.SetColor(ColorID, c);
                    }
                }
            }
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
                    originalMaterials[rend] = rend.materials;
                }
            }
        }
    }
}