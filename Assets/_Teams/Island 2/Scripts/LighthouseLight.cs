using UnityEngine;

public class LighthouseLight : MonoBehaviour
{
    private const float DefaultRotationSpeed = 45f;
    private const float MinRotationSpeed = 0.1f;

    [SerializeField] private float rotationSpeed = DefaultRotationSpeed;
    [SerializeField] private Vector3 rotationAxis = Vector3.up;

    private void Update()
    {
        float clampedSpeed = Mathf.Max(rotationSpeed, MinRotationSpeed);
        transform.Rotate(rotationAxis * clampedSpeed * Time.deltaTime);
    }

    public void SetRotationSpeed(float newSpeed)
    {
        rotationSpeed = newSpeed;
    }

    public float GetRotationSpeed()
    {
        return rotationSpeed;
    }
}
