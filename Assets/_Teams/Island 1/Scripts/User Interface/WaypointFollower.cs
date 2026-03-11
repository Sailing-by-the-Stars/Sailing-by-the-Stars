using UnityEngine;

public class WaypointFollower : MonoBehaviour
{
    [SerializeField] private GameObject[] waypoints;
    [SerializeField] private float movementSpeed = 1f;
    private readonly float switchDistance = .01f;

    private int currentWaypointIndex = 0;

    void Update()
    {
        UpdateWaypointIndex();
        MoveTowardsCurrentWaypoint();
    }

    void UpdateWaypointIndex()
    {
        if (Vector3.Distance(transform.position, waypoints[currentWaypointIndex].transform.position) < switchDistance)
        {
            currentWaypointIndex = currentWaypointIndex + 1;
            if (currentWaypointIndex >= waypoints.Length)
            {
                currentWaypointIndex = 0;
            }
        }
    }

    void MoveTowardsCurrentWaypoint()
    {
        transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypointIndex].transform.position, movementSpeed * Time.deltaTime);
    }
}
