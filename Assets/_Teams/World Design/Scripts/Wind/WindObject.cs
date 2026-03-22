using UnityEngine;

public class WindObject : MonoBehaviour
{
    [SerializeField] private Transform windArrow;
    [SerializeField] private Vector3 currentWindDirection = Vector3.forward;
    [SerializeField] private float intensity = 1f;

    [Header("Wind Trails")]
    [SerializeField] private ParticleSystem windTrails;
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

        Vector3 previousDirection = worldWindDirection;
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
        if (windTrails != null)
        {
            return;
        }

        Transform trailsChild = transform.Find("wind trails");
        if (trailsChild != null)
        {
            windTrails = trailsChild.GetComponent<ParticleSystem>();
        }
    }


    private void HardResetWindTrails()
    {
        Debug.Log("Hard Reset Wind Trails");
        
        // if (windTrails == null)
        // {
            // return;
        // }
        Debug.Log("Hard Reset Wind Trails 2");

        windTrails.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        windTrails.Clear(true);
        windTrails.Simulate(0f, true, true, true);
        
        windTrails.Play(true);
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
