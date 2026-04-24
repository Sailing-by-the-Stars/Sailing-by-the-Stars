/*
 * Created by Christina Pence
 * Contributed to by:
 */
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using System.Collections;

/// <summary>
/// Circling entity for the Bargaining grief event.
/// Spawns on the orbit radius, enters at an angle from above or below,
/// then spirals inward until reaching minimum radius and orbits indefinitely.
/// Despawned externally by BargainingController on checkpoint or timer fail.
///
/// HEIGHT MODES:
///   followWaterSurface = true: entity rides water surface height
///   followWaterSurface = false: entity stays at fixed world Y (eyeball, aerial)
///
/// SETUP:
///   Disable all renderers on the prefab in the inspector.
///   Entity enables them at start of entry so player sees it arrive.
/// </summary>
public class BargainingEntity : MonoBehaviour
{
    [Header("Orbit")]
    [Tooltip("Starting orbit radius when entity first appears.")]
    [SerializeField] private float orbitRadiusStart = 20f;
    [Tooltip("Minimum orbit radius (entity remains at this distance until timer ends).")]
    [SerializeField] private float orbitRadiusMin = 8f;
    [Tooltip("Total movement speed when circling in world units per second.")]
    [SerializeField] private float circleSpeed = 3f;
    [Tooltip("How quickly the orbit center follows the boat. " +
             "Lower values mean the entity lags further behind when the boat moves.")]
    [SerializeField] private float centerCorrectionSpeed = 0.5f;
    [Tooltip("How quickly the entity corrects back to its target radius " +
             "when the boat moves and pulls the orbit center.")]
    [SerializeField] private float radiusCorrectionSpeed = 3f;
    [Tooltip("Entity reaches minimum radius this many times faster than the timer duration. " +
        "(accounts for delay following boat and entry time or increases pressure)")]
    [SerializeField] private float closeInSpeedFactor = 1.5f; 

    [Header("Height")]
    [Tooltip("If true, entity rides water surface height + heightOffset. " +
             "If false, entity stays at a fixed world Y position = heightOffset.")]
    [SerializeField] private bool followWaterSurface = true;
    [Tooltip("Height above water surface (followWaterSurface = true) " +
             "or fixed world Y orbit height (followWaterSurface = false).")]
    [SerializeField] private float heightOffset = 0f;

    [Header("Entry")]
    [Tooltip("Distance below (water mode) or above (aerial mode) orbit height " +
             "where the entity starts its entry approach.")]
    [SerializeField] private float entryDistance = 20f;
    [Tooltip("Speed of entry movement in world units per second.")]
    [SerializeField] private float entrySpeed = 6f;

    private GameObject boat;
    private WaterSurface waterSurface;
    private Vector3 orbitCenter;
    private float currentOrbitRadius;
    private float activeCloseInSpeed;
    private float fullySurfacedYHeight;
    private Coroutine stateRoutine;
    private Renderer[] entityRenderers;

    private const float floatingPointThreshold = 0.01f;

    private void Awake()
    {
        entityRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in entityRenderers)
        {
            r.enabled = false;
        }
    }
    /// <summary>
    /// Spawns entity on the orbit radius and starts the entry and spiral sequence.
    /// </summary>
    public void Initialize(GameObject boatObject, float timerDuration)
    {
        boat = boatObject;
        waterSurface = FindFirstObjectByType<WaterSurface>();
        orbitCenter = new Vector3(boat.transform.position.x, 0f, boat.transform.position.z);
        currentOrbitRadius = orbitRadiusStart;

        // calculate close in speed from timer duration so visual pressure
        // tracks with the time the player has
        float totalRadialDistance = orbitRadiusStart - orbitRadiusMin;
        activeCloseInSpeed = (totalRadialDistance / timerDuration) * closeInSpeedFactor;

        // spawn at random angle on orbit radius so entity never appears directly in front
        float angle = Random.Range(0f, 360f);
        Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad),
                                     0f,
                                     Mathf.Sin(angle * Mathf.Deg2Rad)) * currentOrbitRadius;

        fullySurfacedYHeight = GetSurfaceY(orbitCenter + offset);

        // orbit center Y matches entity movement plane to keep rotation planar
        orbitCenter.y = fullySurfacedYHeight;

        // position below or above orbit height depending on entry mode
        float startY = followWaterSurface ? fullySurfacedYHeight - entryDistance : fullySurfacedYHeight + entryDistance;

        transform.position = new Vector3(orbitCenter.x + offset.x, startY, orbitCenter.z + offset.z);

        stateRoutine = StartCoroutine(OrbitingRoutine());
    }

    /// <summary>
    /// Stops orbit and destroys entity.
    /// </summary>
    public void Despawn()
    {
        if (stateRoutine != null)
        {
            StopCoroutine(stateRoutine);
            stateRoutine = null;
        }
        Destroy(gameObject);
    }
    // TODO: use to trigger warning effects or remove
    public bool AtMinimumRadius => currentOrbitRadius <= orbitRadiusMin + floatingPointThreshold;

    private IEnumerator OrbitingRoutine()
    {
        // sample at orbit center for consistent height across all entity positions
        fullySurfacedYHeight = GetSurfaceY(orbitCenter);

        foreach (Renderer r in entityRenderers)
        {
            r.enabled = true;
        }

        // face along the orbit tangent before beginning entry
        Vector3 tangent = Vector3.Cross(Vector3.up, (transform.position - orbitCenter).normalized);
        if (tangent != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(tangent);
        }

        // Entry phase: approach orbit height while already orbiting horizontally
        float entryY = fullySurfacedYHeight;
        float startY = transform.position.y;
        float elapsed = 0f;
        float duration = Mathf.Max(Mathf.Abs(entryY - startY) / entrySpeed, 0.01f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));

            UpdateOrbit(0f);

            float currentY = Mathf.Lerp(startY, entryY, t);
            transform.position = new Vector3(transform.position.x,
                                             currentY,
                                             transform.position.z);

            yield return null;
        }

        // Phase 2: spiral inward until minimum radius reached
        while (currentOrbitRadius > orbitRadiusMin + floatingPointThreshold)
        {
            fullySurfacedYHeight = GetSurfaceY(orbitCenter);
            UpdateOrbit(activeCloseInSpeed);
            transform.position = new Vector3(transform.position.x,
                                             fullySurfacedYHeight,
                                             transform.position.z);
            yield return null;
        }

        // Orbit at minimum radius until Despawn is called
        while (true)
        {
            fullySurfacedYHeight = GetSurfaceY(orbitCenter);
            UpdateOrbit(0f);
            transform.position = new Vector3(transform.position.x,
                                             fullySurfacedYHeight,
                                             transform.position.z);
            yield return null;
        }
    }
    private float UpdateOrbit(float radialSpeed)
    {
        // orbit center Y tracks surface height to keep rotation planar
        orbitCenter = Vector3.Lerp(orbitCenter,
                                   new Vector3(boat.transform.position.x,
                                               fullySurfacedYHeight,
                                               boat.transform.position.z),
                                   centerCorrectionSpeed * Time.deltaTime);

        currentOrbitRadius = Mathf.Max(orbitRadiusMin, currentOrbitRadius - radialSpeed * Time.deltaTime);

        // tangential speed is derived from total speed minus radial component
        // keeps total movement speed constant as entity spirals in
        float tangentialSpeed = Mathf.Sqrt(Mathf.Max(0f, circleSpeed * circleSpeed - radialSpeed * radialSpeed));

        float angularSpeed = (tangentialSpeed / currentOrbitRadius) * Mathf.Rad2Deg;
        RotateAroundOrbit(angularSpeed);
        CorrectRadius();

        return tangentialSpeed;
    }
    private void RotateAroundOrbit(float angularSpeed)
    {
        transform.RotateAround(orbitCenter, Vector3.up, angularSpeed * Time.deltaTime);
        transform.rotation = Quaternion.LookRotation(Vector3.Cross(Vector3.up, 
                                                                   transform.position - orbitCenter).normalized);
    }

    /// <summary>
    /// Gradually corrects entity back to target radius when the boat moves
    /// and pulls the orbit center away from the entity's current position.
    /// </summary>
    private void CorrectRadius()
    {
        Vector3 toEntity = transform.position - orbitCenter;
        toEntity.y = 0f;
        float actualRadius = toEntity.magnitude;

        if (Mathf.Abs(actualRadius - currentOrbitRadius) > floatingPointThreshold)
        {
            float correctedRadius = Mathf.Lerp(actualRadius,
                                               currentOrbitRadius,
                                               radiusCorrectionSpeed * Time.deltaTime);
            Vector3 corrected = orbitCenter + toEntity.normalized * correctedRadius;
            transform.position = new Vector3(corrected.x, transform.position.y, corrected.z);
        }
    }

    private float GetSurfaceY(Vector3 position)
    {
        if (!followWaterSurface)
        {
            return heightOffset;
        }

        if (waterSurface == null)
        {
            return heightOffset;
        }

        WaterSearchParameters searchParams = new WaterSearchParameters
        {
            targetPositionWS = position,
            error = 0.01f,
            maxIterations = 8
        };

        return waterSurface.ProjectPointOnWaterSurface(searchParams, out WaterSearchResult result)
               ? result.projectedPositionWS.y + heightOffset
               : heightOffset;
    }

    private void OnDrawGizmos()
    {
        if (boat == null)
        {
            return;
        }
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(orbitCenter, currentOrbitRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}