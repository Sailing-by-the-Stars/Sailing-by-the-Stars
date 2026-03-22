using UnityEngine;

public class WindObject : MonoBehaviour
{
    [SerializeField] private Transform windArrowPrimary;
    [SerializeField] private Transform windArrowSecondary;
    [SerializeField] private Vector3 currentWindDirection = Vector3.forward;
    [SerializeField] private float intensity = 1f;

    [Header("Wind Trails")]
    [SerializeField] private ParticleSystem windTrailsPrimary;
    [SerializeField] private ParticleSystem windTrailsSecondary;
    [SerializeField, Min(0f)] private float restartAngleThreshold = 1f;

    private Vector3 worldWindDirection = Vector3.forward;
    private Vector3 primaryAssignedDirection = Vector3.forward;
    private Vector3 secondaryAssignedDirection = Vector3.forward;

    private bool isPrimaryTrailActive = true;

    public Vector3 CurrentWindDirection => currentWindDirection;

    private void OnEnable()
    {
        CacheWindArrow();
        CacheWindTrails();

        if (currentWindDirection.sqrMagnitude > 0.0001f)
        {
            worldWindDirection = currentWindDirection.normalized;
        }

        primaryAssignedDirection = worldWindDirection;
        secondaryAssignedDirection = worldWindDirection;

        InitializeWindTrailEmission();
        RotateWindArrowsByAssignedDirections();
    }

    private void OnDisable()
    {
        StopAllWindTrailEmission();
    }

    private void LateUpdate()
    {
        // Re-apply world rotations after parent transforms update for this frame.
        RotateWindArrowsByAssignedDirections();
    }

    public void SetWindDirection(Vector3 newDirection)
    {
        if (newDirection.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Vector3 normalizedDirection = newDirection.normalized;
        float angleDelta = Vector3.Angle(GetActiveAssignedDirection(), normalizedDirection);

        worldWindDirection = normalizedDirection;
        currentWindDirection = worldWindDirection * intensity;

        if (angleDelta >= restartAngleThreshold)
        {
            SwapActiveWindTrail();
        }

        SetActiveAssignedDirection(worldWindDirection);
        RotateWindArrowsByAssignedDirections();
    }

    private void CacheWindArrow()
    {
        if (windArrowPrimary == null)
        {
            Transform primaryArrowChild = transform.Find("Wind Arrow");
            if (primaryArrowChild != null)
            {
                windArrowPrimary = primaryArrowChild;
            }
        }

        if (windArrowSecondary == null)
        {
            Transform secondaryArrowChild = transform.Find("Wind Arrow 2");
            if (secondaryArrowChild == null)
            {
                secondaryArrowChild = transform.Find("Wind Arrow Secondary");
            }

            if (secondaryArrowChild != null)
            {
                windArrowSecondary = secondaryArrowChild;
            }
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

        if (windArrowPrimary == null && windTrailsPrimary != null)
        {
            windArrowPrimary = windTrailsPrimary.transform.parent;
        }

        if (windArrowSecondary == null && windTrailsSecondary != null)
        {
            windArrowSecondary = windTrailsSecondary.transform.parent;
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

    private void RotateWindArrowsByAssignedDirections()
    {
        RotateSingleWindArrow(windArrowPrimary, primaryAssignedDirection);
        RotateSingleWindArrow(windArrowSecondary, secondaryAssignedDirection);
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
        return isPrimaryTrailActive ? primaryAssignedDirection : secondaryAssignedDirection;
    }

    private void SetActiveAssignedDirection(Vector3 direction)
    {
        if (isPrimaryTrailActive)
        {
            primaryAssignedDirection = direction;
        }
        else
        {
            secondaryAssignedDirection = direction;
        }
    }
}
