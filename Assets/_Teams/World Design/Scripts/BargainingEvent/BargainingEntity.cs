/*
 * Created by Christina Pence
 * Contributed to by:
 */
using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

/// <summary>
/// Entity for the Bargaining grief event.
///
/// HEIGHT MODES:
///   followWaterSurface = true: entity rides water surface height
///   followWaterSurface = false: entity stays at fixed world Y (eyeball, aerial)
/// </summary>
public class BargainingEntity : MonoBehaviour
{
    [Header("Gaze Detection")]
    [Tooltip("Camera used to detect whether the player is looking at this entity. Falls back to Camera.main.")]
    [SerializeField] private Camera playerCamera;
    [Tooltip("How close to screen center counts as looking at the entity.")]
    [SerializeField, Range(0.01f, 0.5f)] private float gazeCenterTolerance = 0.15f;
    [Tooltip("How many seconds of continuous looking to fully dissolve the entity.")]
    [SerializeField, Min(0.01f)] private float gazeDissolveTime = 2.5f;
    [Tooltip("If true, looking away recovers dissolve progress over gazeRecoverTime. " +
         "If false, progress holds until the player looks again.")]
    [SerializeField] private bool gazeRecoveryEnabled = true;
    [Tooltip("How many seconds of looking away to fully recover from current dissolve progress if recovery is enabled.")]
    [SerializeField, Min(0.01f)] private float gazeRecoverTime = 2.5f;


    [Header("Height")]
    [Tooltip("If true, entity rides water surface height + heightOffset. " +
             "If false, entity stays at a fixed world Y = heightOffset.")]
    [SerializeField] private bool followWaterSurface = true;
    [Tooltip("Height above water surface (followWaterSurface = true) " +
             "or fixed world Y (followWaterSurface = false).")]
    [SerializeField] private float heightOffset = 0f;

    [Header("Audio")]
    [Tooltip("Shared audio data containing whisper voice lines.")]
    [SerializeField] private BargainingWhispers whispersAudio;
    [Tooltip("Minimum pause between whisper lines in seconds.")]
    [SerializeField] private float whisperPauseMin = 3f;
    [Tooltip("Maximum pause between whisper lines in seconds.")]
    [SerializeField] private float whisperPauseMax = 5f;
    [SerializeField] private float whisperVolume = 1f;
    [Tooltip("Delay in seconds before the first whisper plays after the entity appears.")]
    [SerializeField] private float firstWhisperDelay = 2f;
    /// <summary>
    /// Fired after audo fade and full gaze progress reaches 1 for entity.
    /// </summary>
    public event System.Action OnDissolveComplete;

    private GameObject boat;
    private WaterSurface waterSurface;
    private Vector3 boatLocalOffset;
    private float gazeProgress = 0f;
    private bool visible = false;

    // Audio
    private Coroutine whisperRoutine;
    private EventInstance currentWhisperInstance;

    // Rendering
    private Renderer[] entityRenderers;
    private Dictionary<Renderer, Color> baseColors = new();
    private MaterialPropertyBlock propertyBlock;
    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private Light[] entityLights;

    private void Awake()
    {
        waterSurface = FindFirstObjectByType<WaterSurface>();
        entityRenderers = GetComponentsInChildren<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
        entityLights = GetComponentsInChildren<Light>();

        foreach (Renderer r in entityRenderers)
        {
            if (r != null && r.sharedMaterial != null && r.sharedMaterial.HasProperty(BaseColorID))
            {
                baseColors[r] = r.sharedMaterial.GetColor(BaseColorID);
            }
        }
        SetRenderersEnabled(false);
    }
    /// <summary>
    /// Positions entity and triggers first appearance.
    /// </summary>
    public void Initialize(GameObject boatObject, Vector3 spawnPosition)
    {
        boat = boatObject;
        playerCamera = playerCamera != null ? playerCamera : Camera.main;
        Appear(spawnPosition);
    }
    /// <summary>
    /// Moves entity to a new position and makes it visible again.
    /// </summary>
    public void Reappear(Vector3 newPosition)
    {
        Appear(newPosition);
    }
    /// <summary>
    /// Fades out audio and destroys the entity.
    /// Called only when player leaves the event zone.
    /// </summary>
    public void Despawn()
    {
        visible = false;
        StopWhispers();
        Destroy(gameObject);
    }
    private void Update()
    {
        if (!visible || boat == null)
        {
            return;
        }
        UpdatePosition();
        FaceBoat();
        UpdateGaze();
    }
    private void Appear(Vector3 position)
    {
        SetRenderersEnabled(false);
        transform.position = new Vector3(position.x, GetSurfaceY(position), position.z);
        // Store XZ offset in boat local space so entity follows boat rotation
        Vector3 offset = boat.transform.InverseTransformPoint(transform.position);
        boatLocalOffset = new Vector3(offset.x, 0f, offset.z);

        gazeProgress = 0f;
        visible = true;

        ApplyDissolveVisual(0f);
        FaceBoat();
        SetRenderersEnabled(true);

        StopWhispers();
        whisperRoutine = StartCoroutine(WhisperRoutine());
    }
    private void Hide()
    {
        visible = false;
        SetRenderersEnabled(false);
        StopWhispers();
        OnDissolveComplete?.Invoke();
    }
    private void UpdatePosition()
    {
        Vector3 targetPos = boat.transform.TransformPoint(new Vector3(boatLocalOffset.x, 0f, boatLocalOffset.z));
        transform.position = new Vector3(targetPos.x, GetSurfaceY(targetPos), targetPos.z);
    }
    private void FaceBoat()
    {
        Vector3 direction = boat.transform.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }
        transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }
    private void UpdateGaze()
    {
        if (IsInCenterView())
        {
            gazeProgress = Mathf.MoveTowards(gazeProgress, 1f, (1f / gazeDissolveTime) * Time.deltaTime);
        }
        else
        {
            if(gazeRecoveryEnabled)
            {
                gazeProgress = Mathf.MoveTowards(gazeProgress, 0f, (1f / gazeRecoverTime) * Time.deltaTime);
            }
        }

        ApplyDissolveVisual(gazeProgress);

        if (visible && gazeProgress >= 1f)
        {
            Hide();
        }
    }
    private bool IsInCenterView()
    {
        if (playerCamera == null)
        {
            return false;
        }
        Vector3 viewportPoint = playerCamera.WorldToViewportPoint(transform.position);
        if (viewportPoint.z <= 0f)
        {
            return false;
        }

        float dx = Mathf.Abs(viewportPoint.x - 0.5f);
        float dy = Mathf.Abs(viewportPoint.y - 0.5f);

        return dx <= gazeCenterTolerance && dy <= gazeCenterTolerance;
    }
    // TODO: Confirm with new asset
    // REQUIRES: surface type = transparent
    private void ApplyDissolveVisual(float progress)
    {
        float alpha = 1f - progress;
        foreach (Renderer r in entityRenderers)
        {
            if (r == null || !baseColors.TryGetValue(r, out Color c))
            {
                continue;
            }
            r.GetPropertyBlock(propertyBlock);
            c.a = alpha;
            propertyBlock.SetColor(BaseColorID, c);
            r.SetPropertyBlock(propertyBlock);
        }
    }
    private void SetRenderersEnabled(bool state)
    {
        foreach (Renderer r in entityRenderers)
        {
            if (r != null)
            {
                r.enabled = state;
            }
        }
        foreach (Light l in entityLights)
        {
            if (l != null)
            {
                l.enabled = state;
            }
        }
    }
    private IEnumerator WhisperRoutine()
    {
        yield return new WaitForSeconds(firstWhisperDelay);
        if (!visible)
        {
            yield break;
        }
        if (whispersAudio == null || whispersAudio.whisperLines == null || whispersAudio.whisperLines.Length == 0)
        {
            Debug.Log($"{gameObject.name}: No whisper lines assigned");
            yield break;
        }

        while (visible)
        {
            ReleaseWhisperInstance();

            EventReference line = whispersAudio.whisperLines[Random.Range(0, whispersAudio.whisperLines.Length)];
            if (!line.IsNull)
            {
                currentWhisperInstance = RuntimeManager.CreateInstance(line);
                RuntimeManager.AttachInstanceToGameObject(currentWhisperInstance, gameObject);
                currentWhisperInstance.setVolume(whisperVolume);
                currentWhisperInstance.start();

                // wait for line to finish before pausing
                currentWhisperInstance.getDescription(out EventDescription description);
                description.getLength(out int lengthMs);
                yield return new WaitForSeconds(lengthMs / 1000f);
            }
            if (!visible)
            {
                yield break;
            }
            float pause = Random.Range(whisperPauseMin, whisperPauseMax);
            yield return new WaitForSeconds(pause);
        }
    }
    private void StopWhispers()
    {
        if (whisperRoutine != null)
        {
            StopCoroutine(whisperRoutine);
            whisperRoutine = null;
        }
        ReleaseWhisperInstance();
    }

    private void ReleaseWhisperInstance()
    {
        if (!currentWhisperInstance.isValid())
        {
            return;
        }
        currentWhisperInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        currentWhisperInstance.release();
        currentWhisperInstance.clearHandle();
    }
    private float GetSurfaceY(Vector3 position)
    {
        if (!followWaterSurface || waterSurface == null)
        {
            return heightOffset;
        }

        WaterSearchParameters searchParams = new WaterSearchParameters
        {
            targetPositionWS = position,
            error = 0.01f,
            maxIterations = 8
        };

        return waterSurface.ProjectPointOnWaterSurface(searchParams, out WaterSearchResult result)
            ? result.projectedPositionWS.y + heightOffset
            : heightOffset;
    }
    private void OnDestroy()
    {
        ReleaseWhisperInstance();
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
#endif
}