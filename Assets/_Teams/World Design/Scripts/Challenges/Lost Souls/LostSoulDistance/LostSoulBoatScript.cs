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
        [SerializeField] private float timeUntilFadeMin = 10f;
        [SerializeField] private float timeUntilFadeMax = 15f;
        [SerializeField] private float fadeDuration = 3f;
        
        [Header("Opacity Distance Fading")]
        [SerializeField] private float fadeStartRange = 40f;
        [SerializeField] private float fadeEndRange = 10f;
        [SerializeField] private float maxOpacity = 1.1f;
        [SerializeField] private float minFadeOpacity = 0.0001f;

        [Header("Debug")]
#pragma warning disable 0414
        [SerializeField] private float distanceToPlayer;
        [SerializeField] private float currentFadeInTime;
        [SerializeField] private float currentFadeOutTime;
        [SerializeField] private float currentTimeUntilFade;
#pragma warning restore 0414

        [Header("Lights")]
        [Tooltip("List of lights to fade in/out with the boat")]
        [SerializeField] private List<Light> targetLights = new List<Light>();

        [Header("Shader Adjustments")]
        [Tooltip("Leave empty to automatically find all renderers on this boat and its children")]
        [SerializeField] private Renderer[] targetRenderers;
        
        [SerializeField] private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

        private Dictionary<Light, float> originalLightIntensities = new Dictionary<Light, float>();

        public System.Action<LostSoulBoatScript> OnFadeComplete;

        private GhostBoatAudio ghostBoatAudio;
        private bool isFadingOut;
        private float currentOpacity;

        private static readonly int OpacityID = Shader.PropertyToID("Opacity");
        private static readonly int UnderOpacityID = Shader.PropertyToID("_Opacity");

        private void Start()
        {
            originalMaterials = new Dictionary<Renderer, Material[]>();
            originalLightIntensities = new Dictionary<Light, float>();
            
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

            foreach (Light tLight in targetLights)
            {
                if (tLight != null)
                {
                    originalLightIntensities[tLight] = tLight.intensity;
                }
            }

            ghostBoatAudio = GetComponent<GhostBoatAudio>();
            if (ghostBoatAudio == null)
            {
                Debug.LogWarning(gameObject.name + ": GhostBoatAudio not found in scene (check prefab)");
            }

            if (playerTransform == null)
            {
                GameObject player = Camera.main.gameObject;
                if (player != null)
                {
                    playerTransform = player.transform;
                }
            }

            SetOriginalRenderer();
            currentOpacity = GetOpacityForCurrentPlayerDistance();
            UpdateOpacity(currentOpacity);
            
            float randomFadeTime = Random.Range(timeUntilFadeMin, timeUntilFadeMax);
            currentTimeUntilFade = randomFadeTime;
            Invoke(nameof(StartFadeAndDelete), randomFadeTime);
            

            StartFadeIn();
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
            currentFadeOutTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                currentFadeOutTime = elapsedTime;
                float targetOpacity = GetOpacityForCurrentPlayerDistance();

                currentOpacity = Mathf.Lerp(targetOpacity, 0f, elapsedTime / fadeDuration);
                if(targetOpacity < currentOpacity)
                {
                    currentOpacity = targetOpacity;
                }
                
                UpdateOpacity(currentOpacity);
                yield return null;
            }

            OnFadeComplete?.Invoke(this);
        }
        public void StartFadeIn()
        {
            StartCoroutine(FadeIn());
        }


        
        private System.Collections.IEnumerator FadeIn()
        {
            isFadingOut = true; 
            float elapsedTime = 0f;
            currentFadeInTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                currentFadeInTime = elapsedTime;
                float targetOpacity = GetOpacityForCurrentPlayerDistance();
                currentOpacity = Mathf.Lerp(0f, targetOpacity, elapsedTime / fadeDuration);
                
                if(targetOpacity < currentOpacity)
                {
                    currentOpacity = targetOpacity;
                }
                
                UpdateOpacity(currentOpacity);
                yield return null;
            }

            currentOpacity = GetOpacityForCurrentPlayerDistance();
            UpdateOpacity(currentOpacity);
            
            isFadingOut = false; 
        }

        private float GetOpacityForCurrentPlayerDistance()
        {
            if (playerTransform == null)
            {
                return maxOpacity;
            }

            float distance = Vector3.Distance(transform.position, playerTransform.position);
            distanceToPlayer = distance;

            if (distance < fadeStartRange)
            {
                float range = fadeStartRange - fadeEndRange;
                float currentVal = distance - fadeEndRange;
                float normalized = Mathf.Clamp01(currentVal / range);
                return normalized * maxOpacity;
            }

            return maxOpacity;
        }

        private void UpdateOpacity(float opacity)
        {
            bool isMinOpacity = opacity <= minFadeOpacity;
            opacity = Mathf.Max(opacity, minFadeOpacity);

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
                    kvp.Key.enabled = !isMinOpacity;
                    if (!isMinOpacity)
                    {
                        kvp.Key.intensity = kvp.Value * normalizedOpacity;
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (fadeStartRange > 0f)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, fadeStartRange);
            }

            if (fadeEndRange > 0f)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, fadeEndRange);
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