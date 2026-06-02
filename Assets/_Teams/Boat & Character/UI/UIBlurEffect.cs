using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIBlurEffect : MonoBehaviour
{
    [Header("Blur Settings")]
    [SerializeField] private float blurSizeMax = 5f;
    [SerializeField] private float blurAmountMax = 0.8f;
    [SerializeField] private float blurFadeDuration = 0.3f;

    private Image targetImage;
    private Material blurMaterial;
    private Material originalMaterial;
    private Coroutine blurCoroutine;

    private void Awake()
    {
        targetImage = GetComponent<Image>();
        if (targetImage == null)
        {
            Debug.LogError("UIBlurEffect must be attached to an Image component!");
            enabled = false;
            return;
        }

        originalMaterial = targetImage.material;
    }

    /// <summary>
    /// Enables the blur effect with a fade-in animation.
    /// </summary>
    public void EnableBlur()
    {
        if (blurCoroutine != null)
            StopCoroutine(blurCoroutine);

        blurCoroutine = StartCoroutine(BlurFadeRoutine(0f, blurAmountMax));
    }

    public void DisableBlur()
    {
        if (blurCoroutine != null)
            StopCoroutine(blurCoroutine);

        blurCoroutine = StartCoroutine(BlurFadeRoutine(blurAmountMax, 0f));
    }
    public void SetBlurAmount(float amount)
    {
        if (blurMaterial == null)
            InitializeBlurMaterial();

        amount = Mathf.Clamp01(amount);
        blurMaterial.SetFloat("_BlurAmount", amount);
        
        // Disable material when blur is 0
        if (amount <= 0f && targetImage.material != originalMaterial)
        {
            targetImage.material = originalMaterial;
        }
    }

    private void InitializeBlurMaterial()
    {
        Shader blurShader = Shader.Find("UI/Blur");
        if (blurShader == null)
        {
            Debug.LogError("UIBlur shader not found! Make sure the shader is in your project.");
            return;
        }

        blurMaterial = new Material(blurShader);
        if (originalMaterial.mainTexture != null)
        {
            blurMaterial.mainTexture = originalMaterial.mainTexture;
        }

        blurMaterial.SetFloat("_BlurSize", blurSizeMax);
        blurMaterial.SetFloat("_BlurAmount", 0f);
    }

    private IEnumerator BlurFadeRoutine(float startAmount, float endAmount)
    {
        if (blurMaterial == null)
            InitializeBlurMaterial();

        if (blurMaterial == null)
            yield break;

        // Switch to blur material if enabling blur
        if (endAmount > 0f && targetImage.material != blurMaterial)
        {
            targetImage.material = blurMaterial;
        }

        float elapsed = 0f;
        
        while (elapsed < blurFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / blurFadeDuration);
            float currentAmount = Mathf.Lerp(startAmount, endAmount, t);
            
            blurMaterial.SetFloat("_BlurAmount", currentAmount);
            yield return null;
        }

        blurMaterial.SetFloat("_BlurAmount", endAmount);

        // Switch back to original material when blur is disabled
        if (endAmount <= 0f)
        {
            targetImage.material = originalMaterial;
        }
    }

    private void OnDestroy()
    {
        if (blurMaterial != null)
        {
            Destroy(blurMaterial);
        }
    }
}
