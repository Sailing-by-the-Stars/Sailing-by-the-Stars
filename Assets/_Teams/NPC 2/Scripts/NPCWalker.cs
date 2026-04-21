using System.Collections;
using UnityEngine;

// Created by: Arch

public class NPCWalker : MonoBehaviour
{
    [Header("Walking Settings")]
    [Tooltip("List of points the NPC will walk to in order.")]
    [SerializeField] private Transform[] waypoints;
    
    [Tooltip("How fast the NPC moves.")]
    [SerializeField] private float walkSpeed = 2f;
    
    [Tooltip("How long the NPC waits after reaching a point.")]
    [SerializeField] private float waitTimeAtWaypoint = 1.5f;
    
    [Tooltip("How fast the NPC turns to face the next point.")]
    [SerializeField] private float rotationSpeed = 5f;

    [Tooltip("Distance required to consider a waypoint reached.")]
    [SerializeField] private float waypointTolerance = 0.1f;

    private int currentWaypointIndex = 0;
    private bool isWaiting = false;

    private void Start()
    {
        if (waypoints.Length == 0)
        {
            Debug.LogWarning("NPCWalker: No waypoints assigned to the array.");
        }
    }

    private void Update()
    {
        // stop moving if the array is empty or the npc is currently waiting
        if (waypoints.Length == 0 || isWaiting) 
        {
            return;
        }

        Transform targetWaypoint = waypoints[currentWaypointIndex];

        // move the npc towards the current target position
        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, walkSpeed * Time.deltaTime);

        // rotate the npc to look at the target while walking
        Vector3 direction = (targetWaypoint.position - transform.position).normalized;
        
        if (direction != Vector3.zero)
        {
            // ignore the y axis so the npc does not tilt up or down
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }

        // check if the npc has reached the waypoint
        if (Vector3.Distance(transform.position, targetWaypoint.position) < waypointTolerance)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }

    /// <summary>
    /// Pauses the NPC movement for a set duration before targeting the next waypoint.
    /// </summary>
    private IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        
        // pause movement for a brief moment
        yield return new WaitForSeconds(waitTimeAtWaypoint);
        
        // move to the next waypoint in the list
        currentWaypointIndex++;
        
        // loop back to the start if the end of the list is reached
        if (currentWaypointIndex >= waypoints.Length)
        {
            currentWaypointIndex = 0; 
        }
        
        isWaiting = false;
    }
}