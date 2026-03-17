using UnityEngine;

public class WindObject : MonoBehaviour
{
    [SerializeField] private WindController windController;
    [SerializeField] private Transform windArrow;
    [SerializeField] private Vector3 currentWindDirection = Vector3.forward;

    public Vector3 CurrentWindDirection => currentWindDirection;

    private void OnEnable()
    {
        if (windArrow == null)
        {
            Transform arrowChild = transform.Find("Wind Arrow");
            if (arrowChild != null)
            {
                windArrow = arrowChild;
            }
        }

        if (windController == null)
        {
            windController = FindFirstObjectByType<WindController>();
        }

        if (windController == null)
        {
            Debug.LogWarning($"[{nameof(WindObject)}] No {nameof(WindController)} found in scene.", this);
            return;
        }

        windController.WindDirectionChanged += HandleWindDirectionChanged;
        HandleWindDirectionChanged(windController.CurrentWindDirection);
    }

    private void OnDisable()
    {
        if (windController == null)
        {
            return;
        }

        windController.WindDirectionChanged -= HandleWindDirectionChanged;
    }

    private void HandleWindDirectionChanged(Vector3 newDirection)
    {
        if (newDirection.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        currentWindDirection = newDirection.normalized;
        RotateWindArrow(currentWindDirection);
    }

    private void RotateWindArrow(Vector3 direction)
    {
        if (windArrow == null)
        {
            return;
        }

        windArrow.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }
}
