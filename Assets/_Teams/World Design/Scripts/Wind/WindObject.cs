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
    }

    private void OnDisable()
    {
        HardResetWindTrails();
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

        worldWindDirection = newDirection;
        currentWindDirection = worldWindDirection * intensity;
        RotateWindArrow(worldWindDirection);
            
        HardResetWindTrails();
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


    private void HardResetWindTrails()
    {
        foreach (ParticleSystem trail in EnumerateWindTrails())
        {
            trail.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            trail.Clear(true);
            trail.Simulate(0f, true, true, true);
            trail.Play(true);
        }
    }

    private System.Collections.Generic.IEnumerable<ParticleSystem> EnumerateWindTrails()
    {
        if (windTrailsPrimary != null)
        {
            yield return windTrailsPrimary;
        }

        if (windTrailsSecondary != null && windTrailsSecondary != windTrailsPrimary)
        {
            yield return windTrailsSecondary;
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
