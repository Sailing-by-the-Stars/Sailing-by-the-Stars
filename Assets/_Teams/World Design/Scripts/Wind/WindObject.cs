using UnityEngine;

public class WindObject : MonoBehaviour
{
    [SerializeField] private Transform windArrow;
    [SerializeField] private Vector3 currentWindDirection = Vector3.forward;
    [SerializeField] private float intensity = 1f;

    public Vector3 CurrentWindDirection => currentWindDirection;

    private void OnEnable()
    {
        CacheWindArrow();
        RotateWindArrow(currentWindDirection.normalized);
    }

    public void SetWindDirection(Vector3 newDirection)
    {
        RotateWindArrow(newDirection);
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
