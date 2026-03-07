using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using Unity.Mathematics;
using UnityEditor;

public class FloatingObject : MonoBehaviour
{
    [Header("Water Settings")]
    public WaterSurface targetSurface; // Reference to the water surface that the object will float on
    public bool includeDeformers = true; // Whether to include surface deformers (waves, ripples) in calculations
    public float verticalOffset = 0.0f; // Vertical offset above the water surface for object positioning

    [Header("Follow Settings")]
    public bool followWaterCurrent = false; // Determines if the object should move along the water current
    public float currentSpeedMultiplier = 1f; // Multiplier to adjust the influence of water current on object movement

    [Header("Collision Settings")]
    public bool useRigidbodyForCollision = false; // Determines if Rigidbody physics should be used for collision detection
    public LayerMask obstacleLayers; // Layers considered obstacles for collision detection (e.g., shore, rocks)


    private bool disableCurrentFlow = false; // Flag to temporarily stop current-based movement after collision
    private WaterSearchParameters searchParameters = new WaterSearchParameters(); // Parameters for projecting object onto water surface
    private WaterSearchResult searchResult = new WaterSearchResult(); // Stores result of the water surface projection
    private Rigidbody rb; // Reference to the Rigidbody component for physics-based collisions

    void Awake()
    {
        rb = GetComponent<Rigidbody>(); // Try to get an existing Rigidbody component
        if (rb == null && useRigidbodyForCollision) // If Rigidbody is required but missing
        {
            rb = gameObject.AddComponent<Rigidbody>(); // Add a new Rigidbody component to enable collisions
        }

        if (rb != null) // If Rigidbody exists
        {
            rb.useGravity = false; // Disable gravity so the object is not affected by physics falling
            rb.isKinematic = !useRigidbodyForCollision; // Make kinematic if collisions are not enabled to prevent unwanted physics
        }
    }

    void Update()
    {
        if (targetSurface == null) return; // If there is not water system it exit the program

        // Prepare parameters for projecting the object onto the water surface
        searchParameters.startPositionWS = (float3)searchResult.candidateLocationWS; // Starting point for water projection
        searchParameters.targetPositionWS = (float3)transform.position; // Target point (current object position)
        searchParameters.error = 0.01f; // Tolerance for projection accuracy
        searchParameters.maxIterations = 8; // Maximum number of iterations for projection solver
        searchParameters.includeDeformation = includeDeformers; // Include water deformation if enabled
        searchParameters.excludeSimulation = false; // Include simulation data in projection

        if (targetSurface.ProjectPointOnWaterSurface(searchParameters, out searchResult)) // Perform the projection
        {
            Vector3 projectedPosition = (Vector3)searchResult.projectedPositionWS; // Get the projected position on the water surface
            Vector3 newPosition = projectedPosition + Vector3.up * verticalOffset; // Apply vertical offset above water

            if (followWaterCurrent && !disableCurrentFlow) // Only move with current if enabled and not disabled
            {
                Vector3 currentDirection = (Vector3)searchResult.currentDirectionWS; // Get the local water current direction
                newPosition += currentDirection * currentSpeedMultiplier * Time.deltaTime; // Move object along the current scaled by speed multiplier and deltaTime
            }

            transform.position = newPosition; // Update object's position in the world
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!useRigidbodyForCollision) return; // Ignore collision if Rigidbody usage is disabled

        // Check if the collided object's layer matches any obstacle layers
        if (((1 << collision.gameObject.layer) & obstacleLayers) != 0)
        {
            disableCurrentFlow = true; // Disable water current movement when hitting an obstacle
            followWaterCurrent = false; // Stop current-following movement temporarily
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!useRigidbodyForCollision) return; // Ignore collision if Rigidbody usage is disabled

        // Check if the object exits collision with an obstacle
        if (((1 << collision.gameObject.layer) & obstacleLayers) != 0)
        {
            disableCurrentFlow = false; // Re-enable current-following movement
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(FloatingObject))] // Define a custom editor for the FloatingObject class
public class SimpleFloatingObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Draw default inspector fields automatically

        FloatingObject obj = (FloatingObject)target; // Cast the target object to FloatingObject

        if (obj.useRigidbodyForCollision) // Only check for a Collider if Rigidbody collision is enabled
        {
            Collider col = obj.GetComponent<Collider>(); // Try to get a Collider component
            if (col == null) // If no Collider exists
            {
                EditorGUILayout.HelpBox(
                    "This object requires a Collider to use Rigidbody for collisions. Please add a Collider manually.",
                    MessageType.Warning // Display as a warning box in the Inspector
                );
            }
        }
    }
}
#endif
