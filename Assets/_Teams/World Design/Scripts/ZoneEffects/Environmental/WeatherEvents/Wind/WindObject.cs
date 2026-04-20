
using UnityEngine;

public class WindObject : MonoBehaviour
{
    [SerializeField] private Transform windArrowA;
    [SerializeField] private Transform windArrowB;
    [SerializeField] private Vector3 currentWindDirection = Vector3.forward;

    [Header("Wind Trails")]
    [SerializeField] private ParticleSystem windTrailA;
    [SerializeField] private ParticleSystem windTrailB;
    [SerializeField, Tooltip("True when Wind Trail A is currently playing.")]
    private bool enableWindTrailA;
    [SerializeField, Tooltip("True when Wind Trail B is currently playing.")] 
    private bool enableWindTrailB;
    [SerializeField]private bool isTrailAActive = true;
    
    [SerializeField, Min(0f)] private float restartAngleThreshold = 1f;
    [SerializeField, Min(0f)] private float windTrailIntensity = 1f;

    private Vector3 worldWindDirection = Vector3.forward;
    private Vector3 trailAAssignedDirection = Vector3.forward;
    private Vector3 trailBAssignedDirection = Vector3.forward;
    private float windTrailABaseSpeedMultiplier = 1f;
    private float windTrailBBaseSpeedMultiplier = 1f;
    private bool hasCachedWindTrailBaseSpeed;

    public Vector3 CurrentWindDirection => currentWindDirection;
    public bool EnableWindTrailA => enableWindTrailA;
    public bool EnableWindTrailB => enableWindTrailB;

    private void OnEnable()
    {
        CacheWindArrow();
        CacheWindTrails();
        CacheWindTrailBaseSpeed();

        worldWindDirection = currentWindDirection;

        trailAAssignedDirection = worldWindDirection;
        trailBAssignedDirection = worldWindDirection;

        ApplyWindTrailIntensityToSpeed();
        InitializeWindTrailEmission();
        RotateWindArrowsByAssignedDirections();
    }

    private void Update()
    {
        if (enableWindTrailA && enableWindTrailB)
        {
            Debug.LogWarning("Wind Trail A and Wind Trail B is currently playing.");
            if (isTrailAActive)
            {
                windTrailB.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                enableWindTrailB = false;
            }
            else
            {
                windTrailA.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                enableWindTrailA = false;
            }
        }
    }

    private void OnDisable()
    {
        StopAllWindTrailEmission();
    }

    private void LateUpdate()
    {
        // Re-apply world rotations after parent transforms update for this frame.
        RotateWindArrowsByAssignedDirections();
        SyncWindTrailPlaybackFlags();
    }

    public void SetWindDirection(Vector3 newDirection)
    {
        if (newDirection.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Vector3 normalizedDirection = newDirection.normalized;
        float angleDelta = Vector3.Angle(GetActiveAssignedDirection(), normalizedDirection);

        currentWindDirection = newDirection;

        if (angleDelta >= restartAngleThreshold)
        {
            SwapActiveWindTrail();
        }

        SetActiveAssignedDirection(currentWindDirection);
        RotateWindArrowsByAssignedDirections();
    }

    private void CacheWindArrow()
    {
        if (windArrowA == null)
        {
            Transform primaryArrowChild = transform.Find("Wind Arrow");
            if (primaryArrowChild != null)
            {
                windArrowA = primaryArrowChild;
            }
        }

        if (windArrowB == null)
        {
            Transform secondaryArrowChild = transform.Find("Wind Arrow 2");
            if (secondaryArrowChild == null)
            {
                secondaryArrowChild = transform.Find("Wind Arrow Secondary");
            }

            if (secondaryArrowChild != null)
            {
                windArrowB = secondaryArrowChild;
            }
        }
    }

    private void CacheWindTrails()
    {
        if (windTrailA != null && windTrailB != null)
        {
            return;
        }

        Transform trailAChild = transform.Find("windTrailA");
        if (trailAChild == null)
        {
            trailAChild = transform.Find("wind trail a");
        }

        if (trailAChild == null)
        {
            trailAChild = transform.Find("wind trails");
        }

        if (windTrailA == null && trailAChild != null)
        {
            windTrailA = trailAChild.GetComponent<ParticleSystem>();
        }

        Transform trailBChild = transform.Find("windTrailB");
        if (trailBChild == null)
        {
            trailBChild = transform.Find("wind trail b");
        }

        if (trailBChild == null)
        {
            trailBChild = transform.Find("wind trails 2");
        }

        if (windTrailB == null && trailBChild != null)
        {
            windTrailB = trailBChild.GetComponent<ParticleSystem>();
        }

        if (windArrowA == null && windTrailA != null)
        {
            windArrowA = windTrailA.transform.parent;
        }

        if (windArrowB == null && windTrailB != null)
        {
            windArrowB = windTrailB.transform.parent;
        }

        if (windTrailA != null && windTrailB != null)
        {
            return;
        }

        ParticleSystem[] childTrails = GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem childTrail in childTrails)
        {
            if (windTrailA == null)
            {
                windTrailA = childTrail;
                continue;
            }

            if (windTrailB == null && childTrail != windTrailA)
            {
                windTrailB = childTrail;
                break;
            }
        }
    }

    private void CacheWindTrailBaseSpeed()
    {
        if (hasCachedWindTrailBaseSpeed)
        {
            return;
        }

        if (windTrailA != null)
        {
            windTrailABaseSpeedMultiplier = windTrailA.main.startSpeedMultiplier;
        }

        if (windTrailB != null)
        {
            windTrailBBaseSpeedMultiplier = windTrailB.main.startSpeedMultiplier;
        }

        hasCachedWindTrailBaseSpeed = true;
    }

    private void ApplyWindTrailIntensityToSpeed()
    {
        CacheWindTrailBaseSpeed();
        ApplyWindTrailSpeed(windTrailA, windTrailABaseSpeedMultiplier);
        ApplyWindTrailSpeed(windTrailB, windTrailBBaseSpeedMultiplier);
    }

    private void ApplyWindTrailSpeed(ParticleSystem trail, float baseSpeedMultiplier)
    {
        if (trail == null)
        {
            return;
        }

        ParticleSystem.MainModule main = trail.main;
        main.startSpeedMultiplier = baseSpeedMultiplier * windTrailIntensity;
    }

    private void InitializeWindTrailEmission()
    {
        isTrailAActive = true;

        if (windTrailA != null)
        {
            windTrailA.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            windTrailA.Play(true);
        }

        if (windTrailB != null)
        {
            windTrailB.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            // If Trail A is not assigned, use Trail B as the active fallback.
            if (windTrailA == null)
            {
                isTrailAActive = false;
                windTrailB.Play(true);
            }
        }

        SyncWindTrailPlaybackFlags();
    }

    private void SwapActiveWindTrail()
    {
        Debug.Log("SwapActiveWindTrail");
        ParticleSystem activeTrail = isTrailAActive ? windTrailA : windTrailB;
        ParticleSystem nextTrail = isTrailAActive ? windTrailB : windTrailA;

        if (nextTrail == null)
        {
            if (activeTrail != null && !activeTrail.isPlaying)
            {
                activeTrail.Play(true);
            }

            SyncWindTrailPlaybackFlags();
            return;
        }

        if (activeTrail != null)
        {
            activeTrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        nextTrail.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        nextTrail.Play(true);
        isTrailAActive = !isTrailAActive;
        SyncWindTrailPlaybackFlags();
    }

    private void StopAllWindTrailEmission()
    {
        if (windTrailA != null)
        {
            windTrailA.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (windTrailB != null)
        {
            windTrailB.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        SyncWindTrailPlaybackFlags();
    }

    private void RotateWindArrowsByAssignedDirections()
    {
        RotateSingleWindArrow(windArrowA, trailAAssignedDirection);
        RotateSingleWindArrow(windArrowB, trailBAssignedDirection);
    }

    private static void RotateSingleWindArrow(Transform arrow, Vector3 direction)
    {
        if (arrow == null || direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        arrow.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    private Vector3 GetActiveAssignedDirection()
    {
        return isTrailAActive ? trailAAssignedDirection : trailBAssignedDirection;
    }

    private void SetActiveAssignedDirection(Vector3 direction)
    {
        if (isTrailAActive)
        {
            trailAAssignedDirection = direction;
        }
        else
        {
            trailBAssignedDirection = direction;
        }
    }

    private void SyncWindTrailPlaybackFlags()
    {
        enableWindTrailA = windTrailA != null && windTrailA.isEmitting;
        enableWindTrailB = windTrailB != null && windTrailB.isEmitting;
    }

    public void SetWindIntensity(float newIntensity)
    {
        windTrailIntensity = Mathf.Max(0f, newIntensity);
        ApplyWindTrailIntensityToSpeed();
    }
}
