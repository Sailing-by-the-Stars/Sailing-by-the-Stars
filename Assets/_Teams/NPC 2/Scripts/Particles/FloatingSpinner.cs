// Programmer: Arch

using UnityEngine;

/// <summary>
/// Makes an object gently bob up and down and slowly spin around its Y axis.
/// </summary>
public class FloatingSpinner : MonoBehaviour
{
    [Header("Bob (up / down)")]
    [SerializeField] private float bobHeight = 0.15f;
    [SerializeField] private float bobSpeed = 2f;

    [Header("Spin (around Y)")]
    [SerializeField] private float spinSpeed = 60f;

    private Vector3 startLocalPosition;

    private void Start()
    {
        startLocalPosition = transform.localPosition;
    }

    private void Update()
    {
        float yOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.localPosition = startLocalPosition + Vector3.up * yOffset;

        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
    }
}
