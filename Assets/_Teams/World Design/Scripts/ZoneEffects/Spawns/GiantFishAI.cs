/*
 * Created by Christina Pence
 * Contributed to by:
 */
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using System.Collections;

public class GiantFishAI : MonoBehaviour, IWorldSpawnable
{
    [Header("Orbit")]
    [SerializeField] private float orbitRadiusStart = 15f;
    [SerializeField] private float orbitRadiusMin = 8f;
    [Tooltip("Total movement speed while circling in world units per second.")]
    [SerializeField] private float circleSpeed = 10f;
    [Tooltip("How long the fish circles at the surface before attacking")]
    [SerializeField] private float orbitDurationMin = 8f;
    [SerializeField] private float orbitDurationMax = 14f;
    [Tooltip("Number of orbits while spiraling up from underwater to the surface")]
    [SerializeField] private float underwaterOrbitCount = 2f;
    [Tooltip("How quickly the orbit center follows the target - lower values lag further behind")]
    [SerializeField] private float centerCorrectionSpeed = 0.5f;
    [Tooltip("How quickly the fish returns to target radius after being knocked off course by collision")]
    [SerializeField] private float radiusCorrectionSpeed = 3f;

    [Header("Movement")]
    [Tooltip("Speed for submerging, surfacing, diving and retreating")]
    [SerializeField] private float swimSpeed = 6f;
    [SerializeField] private float attackSpeed = 20f;
    [Tooltip("Angle of descent when diving and retreating in degrees")]
    [SerializeField] private float diveAngle = 30f;
    [Tooltip("Y position of the fish when fully submerged")]
    [SerializeField] private float fullySubmergedY = -10f;
    [Tooltip("Units above the water surface the fish is when circling")]
    [SerializeField] private float surfaceOffset = 0f;

    [Header("Attack")]
    [Tooltip("Passive fish never attacks but dives periodically")]
    [SerializeField] private bool isPassive = false;
    [SerializeField] private float attackArcHeight = 6f;
    [Tooltip("Units above boat pivot to aim the attack")]
    [SerializeField] private float boatSurfaceOffset = 1f;
    [Tooltip("Cooldown after diving before the fish approaches and surfaces again")]
    [SerializeField] private float cooldownMin = 5f;
    [SerializeField] private float cooldownMax = 12f;

    [Header("Knockback")]
    [Tooltip("Force applied to boat on attack")]
    [SerializeField] private float attackKnockbackForce = 20f;
    [Tooltip("Spin applied to boat in degrees per second")]
    [SerializeField] private float attackKnockbackSpin = 90f;

    [Header("Collision")]
    [Tooltip("Force pushing boat away when it contacts the fish")]
    [SerializeField] private float collisionBoatForce = 5f;
    [Tooltip("Force pushing fish away when contacted by boat")]
    [SerializeField] private float collisionFishForce = 8f;
    [Tooltip("Multiplies both collision forces when boat is actively ramming the fish")]
    [SerializeField] private float rammingMultiplier = 2.5f;
    [Tooltip("Minimum approach speed of boat in units per second before contact counts as ramming")]
    [SerializeField] private float rammingThreshold = 4f;
    [Tooltip("Spin applied to boat when colliding on surface without attack")]
    [SerializeField] private float collisionSpin = 0f;
    [Tooltip("Tag of object collision force should apply to")]
    [SerializeField] private string collisionTag = "boat";

    // Orbiting:    swims to entry point, spirals up from underwater, then circles at surface
    // Attacking:   parabolic charge at boat
    // Diving:      dives away from boat, swims to outer radius, waits cooldown
    // Retreating:  dives toward zone center and despawns
    private enum State { Orbiting, Attacking, Diving, Retreating }

    private GameObject boat;
    private IKnockable knockable; // collision logic implemented by movement script of target
    private Rigidbody boatRb;
    private Collider boatCollider;
    private WaterSurface waterSurface;
    private Vector3 zoneCenter;
    private Vector3 orbitCenter;
    private float currentOrbitRadius;
    private float fullySurfacedYHeight;
    private Coroutine stateRoutine;
    private Vector3 pendingKnockbackDir;
    private bool isAttacking = false;
    private const float floatingPointThreshold = 0.01f;

    public void Initialize(GameObject instigator, Vector3 center)
    {
        boat = instigator;
        knockable = boat.GetComponent<IKnockable>();
        if (knockable == null)
        {
            Debug.LogWarning($"{gameObject.name}: no IKnockable found on {boat.name} — knockback will have no effect");
        }

        boatRb = boat.GetComponent<Rigidbody>();
        if (boatRb == null)
        {
            Debug.Log($"{gameObject.name}: no Rigidbody found on {boat.name} — ramming detection will not work");
        }
        boatCollider = boat.GetComponent<Collider>();
        if (boat.GetComponent<Collider>() == null)
        {
            Debug.LogWarning($"{gameObject.name}: no Collider found on {boat.name} — attack will aim at pivot point");
        }


        waterSurface = FindFirstObjectByType<WaterSurface>();

        zoneCenter = center;
        orbitCenter = new Vector3(boat.transform.position.x, 0f, boat.transform.position.z);
        currentOrbitRadius = orbitRadiusStart;

        // start fish swimming up from the depths
        transform.position = new Vector3(transform.position.x, fullySubmergedY, transform.position.z);

        EnterState(State.Orbiting);
    }

    public void Despawn()
    {
        EnterState(State.Retreating);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(collisionTag))
        {
            return;
        }

        if (isAttacking)
        {
            ApplyKnockback(pendingKnockbackDir);

            if (stateRoutine != null)
            {
                StopCoroutine(stateRoutine);
            }

            stateRoutine = StartCoroutine(DivingRoutine());
            return;
        }
        // apply force when running into object to prevent visual overlap or run away
        if (knockable != null)
        {   
            PushApart(other);
        }
        else
        {
            EnterState(State.Diving);
        }
    }

    private void PushApart(Collider other)
    {
        // get directional unit vector (force independent of distance)
        Vector3 pushDir = other.transform.position - transform.position;
        pushDir.y = 0f;
        pushDir.Normalize();

        // if no Rigidbody, velocity is zero — isRamming will always be false
        Vector3 boatVelocity = boatRb != null ? boatRb.linearVelocity : Vector3.zero;
        // dot product to get speed component of boat directed towards fish
        bool isRamming = Vector3.Dot(-pushDir, boatVelocity) > rammingThreshold;
        float multiplier = isRamming ? rammingMultiplier : 1f;
        Vector3 velocity = pushDir * collisionBoatForce; // do not apply ramming to boat (feels like punishment)
        knockable?.ApplyKnockback(velocity, collisionSpin);
        // push away fish with ramming multiplier
        transform.position += -pushDir * collisionFishForce * multiplier * Time.deltaTime;
    }

    private void EnterState(State state)
    {
        Debug.Log($"{gameObject.name}: entering {state}");
        isAttacking = false;

        if (stateRoutine != null)
        {
            StopCoroutine(stateRoutine);
        }

        stateRoutine = state switch
        {
            State.Orbiting => StartCoroutine(OrbitingRoutine()),
            State.Attacking => StartCoroutine(AttackingRoutine()),
            State.Diving => StartCoroutine(DivingRoutine()),
            State.Retreating => StartCoroutine(RetreatingRoutine()),
            _ => null // unrecognized case defaults to no behavior
        };
    }

    // phase 1: swim to orbit entry point underwater
    // phase 2: spiral closer to boat and surface at the same time
    // phase 3: at surface keep closing in until orbitRadiusMin before attacking
    private IEnumerator OrbitingRoutine()
    {
        fullySurfacedYHeight = GetWaterHeight(transform.position) + surfaceOffset;

        // get horizontal direction for fish to move to entry point
        Vector3 fromCenterToFish = transform.position - orbitCenter;
        fromCenterToFish.y = 0f;
        Vector3 entryPoint = orbitCenter + fromCenterToFish.normalized * currentOrbitRadius;
        // start at full submerged depth
        entryPoint.y = fullySubmergedY;

        yield return MoveToPosition(transform.position, entryPoint, swimSpeed);

        // face along orbit (perpendicular to line from orbit center to fish)
        Vector3 tangent = Vector3.Cross(Vector3.up, (transform.position - orbitCenter).normalized);
        if (tangent != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(tangent);
        }

        // estimate speed fish should move inwards based on parameters given
        float circumference = 2f * Mathf.PI * currentOrbitRadius;
        float spiralDuration = (circumference * underwaterOrbitCount) / circleSpeed;
        float totalRadialDistance = orbitRadiusStart - orbitRadiusMin;
        // underwater estimated duration + average surface duration
        float totalDuration = spiralDuration + ((orbitDurationMin + orbitDurationMax) * 0.5f);
        float closeInSpeed = totalRadialDistance / totalDuration;

        // circle in and upward
        float verticalDistance = fullySurfacedYHeight - fullySubmergedY;
        float riseSpeed = verticalDistance / spiralDuration; // use approximate time fish should be underwater

        float spiralElapsed = 0f;

        while (spiralElapsed < spiralDuration)
        {
            spiralElapsed += Time.deltaTime;

            float tangentialSpeed = UpdateOrbit(closeInSpeed, riseSpeed);

            // angle between vertical and forward components to match travel direction
            float pitchAngle = Mathf.Atan2(riseSpeed, tangentialSpeed) * Mathf.Rad2Deg;
            // negative x tilts upwards (fish is rising)
            transform.rotation = Quaternion.Euler(-pitchAngle, transform.rotation.eulerAngles.y, 0f);
            
            float newY = Mathf.Min(fullySubmergedY + riseSpeed * spiralElapsed, fullySurfacedYHeight);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            yield return null;
        }

        // circle at surface
        float orbitDuration = Random.Range(orbitDurationMin, orbitDurationMax);
        float orbitElapsed = 0f;

        while (orbitElapsed < orbitDuration)
        {
            orbitElapsed += Time.deltaTime;
            // use default 0 rise speed when fish is at surface
            UpdateOrbit(closeInSpeed);

            // use Y of water when fish is surfaced
            transform.position = new Vector3(transform.position.x, fullySurfacedYHeight, transform.position.z);

            yield return null;
        }
        // transition to diving if fish is passive, otherwise attack
        EnterState(isPassive ? State.Diving : State.Attacking);
    }
    // update orbit center, radius, and angular speed
    // returns tangential speed
    private float UpdateOrbit(float closeInSpeed, float riseSpeed = 0f)
    {
        // gradual adjustment of center back to boat to allow variation in distance between boat and fish
        orbitCenter = Vector3.Lerp(orbitCenter,
            new Vector3(boat.transform.position.x, orbitCenter.y, boat.transform.position.z),
            centerCorrectionSpeed * Time.deltaTime);

        currentOrbitRadius = Mathf.Max(orbitRadiusMin, currentOrbitRadius - closeInSpeed * Time.deltaTime);
        // 3D Pythagoras: tangentialSpeed = sqrt(circleSpeed^2 - closeInSpeed^2 - riseSpeed^2)
        float tangentialSpeed = Mathf.Sqrt(Mathf.Max(0f, circleSpeed * circleSpeed -
                                                     closeInSpeed * closeInSpeed - riseSpeed * riseSpeed));
        float angularSpeed = (tangentialSpeed / currentOrbitRadius) * Mathf.Rad2Deg;
        RotateAroundOrbit(angularSpeed);
        CorrectRadius();

        return tangentialSpeed;
    }

    // attacks boat by jumping in a parabolic arc
    private IEnumerator AttackingRoutine()
    {
        isAttacking = true;

        CalculateKnockbackDirection();

        Vector3 startPos = transform.position;
        // default to boat pivot if no collider found
        Vector3 targetPos = boatCollider != null ?
                            boatCollider.ClosestPoint(transform.position) : boat.transform.position;
        targetPos.y = fullySurfacedYHeight + boatSurfaceOffset;

        float distance = Vector3.Distance(startPos, targetPos);
        float duration = distance / attackSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Vector3 pos = Vector3.Lerp(startPos, targetPos, t);
            // parabolic path for attack
            pos.y = Mathf.Lerp(startPos.y, targetPos.y, t) + attackArcHeight * Mathf.Sin(t * Mathf.PI);
            transform.position = pos;

            Vector3 moveDir = targetPos - transform.position;
            moveDir.y = 0f;
            // pitch from arc slope: derivative of (lerp + arcHeight * Sin(t * PI)) with respect to t
            float arcSlope = (targetPos.y - startPos.y) + attackArcHeight * Mathf.PI * Mathf.Cos(t * Mathf.PI);
            if (moveDir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(new Vector3(moveDir.normalized.x,
                                                                         arcSlope / distance, moveDir.normalized.z));
            }

            yield return null;
        }
        // attack knockback force does not account for boat velocity
        ApplyKnockback(pendingKnockbackDir);
        EnterState(State.Diving);
    }

    // sinks away from boat, swims back to outer radius, waits, then approaches again
    private IEnumerator DivingRoutine()
    {
        isAttacking = false;
        // face away from boat for retreat
        Vector3 awayFromBoat = transform.position - boat.transform.position;
        awayFromBoat.y = 0f;

        if (awayFromBoat != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(awayFromBoat.normalized);
        }

        yield return AngleMoveToDepth(fullySubmergedY);

        // swim back to outer radius underwater
        Vector3 fromCenterToFish = transform.position - orbitCenter;
        fromCenterToFish.y = 0f;
        // use unit direction vector to get position on outer radius
        Vector3 outerPos = orbitCenter + fromCenterToFish.normalized * orbitRadiusStart;
        outerPos.y = fullySubmergedY;

        yield return MoveToPosition(transform.position, outerPos, swimSpeed);

        yield return new WaitForSeconds(Random.Range(cooldownMin, cooldownMax));

        currentOrbitRadius = orbitRadiusStart;
        EnterState(State.Orbiting);
    }
    private void CalculateKnockbackDirection()
    {
        Vector3 knockbackDir = boat.transform.position - transform.position;
        knockbackDir.y = 0f;
        knockbackDir.Normalize();
        pendingKnockbackDir = knockbackDir;
    }

    // faces zone center, dives at diveAngle, then despawns
    private IEnumerator RetreatingRoutine()
    {
        Vector3 toCenter = zoneCenter - transform.position;
        toCenter.y = 0f;

        if (toCenter != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(toCenter.normalized);
        }

        yield return AngleMoveToDepth(fullySubmergedY);

        Destroy(gameObject);
    }

    // rotates around orbitCenter and faces direction of travel
    private void RotateAroundOrbit(float angularSpeed)
    {
        transform.RotateAround(orbitCenter, Vector3.up, angularSpeed * Time.deltaTime);
        // face fish horizontally tangent to the radius (use outward direction for counterclockwise rotation/ positive angularSpeed)
        transform.rotation = Quaternion.LookRotation(Vector3.Cross(Vector3.up,
                                                                   transform.position - orbitCenter).normalized);
    }

    // gradually corrects fish back to currentOrbitRadius if it was knocked off
    private void CorrectRadius()
    {
        Vector3 toFish = transform.position - orbitCenter;
        toFish.y = 0f;
        float actualRadius = toFish.magnitude;
        // move fish towards correct orbit path relative to boat faster when further away and more gradually when closer
        if (Mathf.Abs(actualRadius - currentOrbitRadius) > floatingPointThreshold) // skip if close to avoid infinite calculation due to floating point
        {
            float correctedRadius = Mathf.Lerp(actualRadius, currentOrbitRadius, radiusCorrectionSpeed * Time.deltaTime);
            Vector3 corrected = orbitCenter + toFish.normalized * correctedRadius;
            transform.position = new Vector3(corrected.x, transform.position.y, corrected.z);
        }
    }

    // moves between two points at given speed with smooth easing
    private IEnumerator MoveToPosition(Vector3 from, Vector3 to, float speed)
    {
        float duration = Vector3.Distance(from, to) / speed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            transform.position = Vector3.Lerp(from, to, t);
            yield return null;
        }
    }

    // swims to targetY at diveAngle degrees in current facing direction, tilting nose during travel
    private IEnumerator AngleMoveToDepth(float targetY)
    {
        Vector3 startPosition = transform.position;
        float verticalDistance = Mathf.Abs(targetY - startPosition.y);

        if (verticalDistance < floatingPointThreshold)
        {
            yield break;
        }

        float horizontalDistance = verticalDistance / Mathf.Tan(diveAngle * Mathf.Deg2Rad);

        Vector3 forward = transform.forward;
        forward.y = 0f;

        if (forward == Vector3.zero)
        {
            forward = Vector3.forward;
        }

        forward.Normalize();

        Vector3 endPosition = new Vector3(startPosition.x + forward.x * horizontalDistance,
                                     targetY,
                                     startPosition.z + forward.z * horizontalDistance);

        Vector3 diveDirection = (endPosition - startPosition).normalized;
        Quaternion diveRotation = Quaternion.LookRotation(diveDirection);
        Quaternion levelRotation = Quaternion.LookRotation(new Vector3(diveDirection.x, 0f, diveDirection.z));

        float duration = Vector3.Distance(startPosition, endPosition) / swimSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            // gradually transition rotation from dive angle to level horizontally
            transform.rotation = Quaternion.Lerp(diveRotation, levelRotation, t);
            yield return null;
        }

        transform.rotation = levelRotation;
    }

    private float GetWaterHeight(Vector3 position)
    {
        if (waterSurface == null)
        {
            return 0f;
        }

        WaterSearchParameters searchParams = new WaterSearchParameters
        {
            targetPositionWS = position,
            error = 0.01f,
            maxIterations = 8
        };

        if (waterSurface.ProjectPointOnWaterSurface(searchParams, out WaterSearchResult result))
        {
            return result.projectedPositionWS.y;
        }

        return 0f;
    }

    private void ApplyKnockback(Vector3 dir)
    {
        if (knockable == null)
        {
            return;
        }
        Vector3 velocity = dir * attackKnockbackForce;

        knockable.ApplyKnockback(velocity, attackKnockbackSpin);
    }

    private void OnDrawGizmos()
    {
        if (boat == null)
        {
            return;
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(orbitCenter, currentOrbitRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(boat.transform.position, 2f);
    }
}