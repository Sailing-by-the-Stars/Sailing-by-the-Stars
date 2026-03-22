using UnityEngine;

public class WindObject : MonoBehaviour
{
    [SerializeField] private Transform windArrow;
    [SerializeField] private Vector3 currentWindDirection = Vector3.forward;
    [SerializeField] private float intensity = 1f;

    [Header("Wind Trails")]
    [SerializeField] private ParticleSystem windTrailsPrimary;
    [SerializeField] private ParticleSystem windTrailsSecondary;
    [SerializeField, Min(0f)] private float restartAngleThreshold = 1f;

    // Stores the normalized world-space heading used for arrow orientation.
    private Vector3 worldWindDirection = Vector3.forward;

    private bool isPrimaryTrailActive = true;

    public Vector3 CurrentWindDirection => currentWindDirection;

    private void OnEnable()
    {
        CacheWindArrow();
        CacheWindTrails();

        if (currentWindDirection.sqrMagnitude > 0.0001f)
        {
            worldWindDirection = currentWindDirection;
        }

        RotateWindArrow(worldWindDirection);
        InitializeWindTrailEmission();
    }

    private void OnDisable()
    {
        StopAllWindTrailEmission();
    }

    private void LateUpdate()
    {
        // Re-apply world rotation after parent transforms have updated for this frame.
        RotateWindArrow(worldWindDirection);
    }

    public void SetWindDirection(Vector3 newDirection)
    {
        if (newDirection.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Vector3 normalizedDirection = newDirection.normalized;
        float angleDelta = worldWindDirection.sqrMagnitude > 0.0001f
            ? Vector3.Angle(worldWindDirection, normalizedDirection)
            : 180f;

        worldWindDirection = normalizedDirection;
        currentWindDirection = worldWindDirection * intensity;
        RotateWindArrow(worldWindDirection);

        if (angleDelta >= restartAngleThreshold)
        {
            SwapActiveWindTrail();
        }
    }

    private void CacheWindArrow()
    {
        if (windArrow != null)
        {
            return;
        }

        Transform arrowChild = transform.Find("Wind Arrow");
        if (arrowChild != null)
        {
            windArrow = arrowChild;
        }
    }

    private void CacheWindTrails()
    {
        if (windTrailsPrimary != null && windTrailsSecondary != null)
        {
            return;
        }

        Transform primaryChild = transform.Find("wind trails");
        if (windTrailsPrimary == null && primaryChild != null)
        {
            windTrailsPrimary = primaryChild.GetComponent<ParticleSystem>();
        }

        Transform secondaryChild = transform.Find("wind trails 2");
        if (windTrailsSecondary == null && secondaryChild != null)
        {
            windTrailsSecondary = secondaryChild.GetComponent<ParticleSystem>();
        }

        if (windTrailsPrimary != null && windTrailsSecondary != null)
        {
            return;
        }

        ParticleSystem[] childTrails = GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem childTrail in childTrails)
        {
            if (windTrailsPrimary == null)
            {
                windTrailsPrimary = childTrail;
                continue;
            }

            if (windTrailsSecondary == null && childTrail != windTrailsPrimary)
            {
                windTrailsSecondary = childTrail;
                break;
            }
        }
    }


    private void InitializeWindTrailEmission()
    {
        isPrimaryTrailActive = true;

        if (windTrailsPrimary != null)
        {
            windTrailsPrimary.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            windTrailsPrimary.Play(true);
        }

        if (windTrailsSecondary != null)
        {
            windTrailsSecondary.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        // If primary is missing, fall back to secondary as the active emitter.
        if (windTrailsPrimary == null && windTrailsSecondary != null)
        {
            isPrimaryTrailActive = false;
            windTrailsSecondary.Play(true);
        }
    }

    private void SwapActiveWindTrail()
    {
        ParticleSystem activeTrail = isPrimaryTrailActive ? windTrailsPrimary : windTrailsSecondary;
        ParticleSystem nextTrail = isPrimaryTrailActive ? windTrailsSecondary : windTrailsPrimary;

        if (nextTrail == null)
        {
            if (activeTrail != null && !activeTrail.isPlaying)
            {
                activeTrail.Play(true);
            }

            return;
        }

        if (activeTrail != null)
        {
            // Stop emitting only, so already-spawned particles finish naturally.
            activeTrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        nextTrail.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        nextTrail.Play(true);
        isPrimaryTrailActive = !isPrimaryTrailActive;
    }

    private void StopAllWindTrailEmission()
    {
        if (windTrailsPrimary != null)
        {
            windTrailsPrimary.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (windTrailsSecondary != null)
        {
            windTrailsSecondary.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void RotateWindArrow(Vector3 direction)
    {
        if (!windArrow || direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        windArrow.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }
}
