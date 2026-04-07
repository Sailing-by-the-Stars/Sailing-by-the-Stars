using UnityEngine;
using UnityEngine.UIElements;

public class FireflyFollower : MonoBehaviour
{
    [SerializeField] private FireflyPath path;
    [SerializeField] private Transform player;
    [SerializeField] private float offset = 3f;
    [SerializeField] private float smoothSpeed = 2f;
    [SerializeField] private float maxAdvanceSpeed = 5f;
    [SerializeField] private float hoverAnimationIntensity = 0.01f;

    private float currentDistance = 0f;

    void Update()
    {
        // Get the player's closest distance on the path
        float playerDistance = path.GetClosestDistance(player.position);

        // Never go backwards
        float desiredDistance = Mathf.Max(currentDistance, playerDistance);

        // Smooth forward movement along the path
        currentDistance = Mathf.MoveTowards(
            currentDistance,
            desiredDistance,
            maxAdvanceSpeed * Time.deltaTime
        );

        // Always stay slightly ahead of the player
        float targetDistance = currentDistance + offset;

        // Compute the firefly's target position along the path
        Vector3 pathPos = path.GetPositionAtDistance(targetDistance);

        // Apply hover effect
        Vector3 hoverOffset = new Vector3(
            Mathf.Sin(Time.time * 3f),
            Mathf.Sin(Time.time * 5f),
            0
        ) * hoverAnimationIntensity;

        // Directly set position: path + hover
        transform.position = pathPos + hoverOffset;

        // Rotate smoothly toward movement direction
        Vector3 forwardDir = (path.GetPositionAtDistance(targetDistance + 0.1f) - pathPos).normalized;
        transform.forward = Vector3.Slerp(transform.forward, forwardDir, Time.deltaTime * smoothSpeed);
    }
}
