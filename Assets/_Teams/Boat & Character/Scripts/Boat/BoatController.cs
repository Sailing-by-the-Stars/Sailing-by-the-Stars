/*
*   Created by Johan Beimers
*   Contributed to by: 
*/

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BoatController : MonoBehaviour
{
    [Header("Physics settings")]
    [SerializeField] private float dragCoefficient = 1.1f;
    [SerializeField] private float liftCoefficient = 1.5f;
    [SerializeField] private float sailArea = 20;
    [SerializeField] private float underwaterFrontArea = .5f;
    [SerializeField] private float airDensity = 1.225f;
    [SerializeField] private float waterDensity = 1000f;
    [SerializeField] private float keelDragStrength = 20f;
    [SerializeField] private float baseForwardForce = 900f;
    [SerializeField] private float maxRotationRate = 10f;
    [SerializeField] private float rudderTorqueStrength = 30f;
    [SerializeField] private bool useLocalWindSpeed = true;

    [Header("Physics stats (Debugging!)")]
    [SerializeField] private float forwardSpeed;
    [SerializeField] private float AoA;
    [SerializeField] private float drag;
    [SerializeField] private float lift;
    [SerializeField] private float rudderTorque;
    [SerializeField] private float rotationRate;
    [SerializeField] private Vector3 wind = Vector3.zero;
    [SerializeField] private Vector3 apparentWind = Vector3.zero;

    [Header("Boat stats")]
    [SerializeField] private float maxRudderDeflection = 20f;
    [SerializeField] private float maxMastAngle = 90f;

    [Header("Controls")]
    [SerializeField] private SmoothAxis2D rudderAxis;
    [SerializeField] private SmoothAxis2D mastAxis;

    [Header("")]
    [SerializeField] private GameObject hullObject;
    [SerializeField] private GameObject rudderObject;
    [SerializeField] private GameObject mastObject;
    [SerializeField] private GameObject sailObject;
    [SerializeField] private GameObject mastPivot;


    private Rigidbody rigidBody;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        rudderAxis.Reset();
        mastAxis.Reset();
    }

    // Update is called once per frame
    void Update()
    {
        //RotateRudder();
        RotateMastAndSail();
    }

    void FixedUpdate()
    {
        if (!useLocalWindSpeed && WeatherManager.Instance != null)
        {
            wind = WeatherManager.Instance.WindVelocity;
        }

        apparentWind = wind - rigidBody.linearVelocity;
        Vector3 mastDirection = Vector3.ProjectOnPlane(-mastObject.transform.up, Vector3.up).normalized;
        Vector3 windDirection = Vector3.ProjectOnPlane(apparentWind, Vector3.up).normalized;

        float mastDirectionIntoWind = Vector3.SignedAngle(mastDirection, -windDirection, Vector3.up);

        CalculateDrag(apparentWind.magnitude, mastDirectionIntoWind);
        CalculateLift(apparentWind.magnitude, mastDirectionIntoWind);

        AoA = mastDirectionIntoWind;

        ApplyBaseForwardForce();
        ApplyRudderTorque();
        ApplyKeelDrag();
        applyWaterDrag();

        forwardSpeed = transform.InverseTransformVector(rigidBody.linearVelocity).z;
    }

    //Calculates the force of drag experienced on the sail, used when running downwind and broadreach
    void CalculateDrag(float apparentWindSpeed, float apparentWindAngle)
    {
        drag = .5f * airDensity * apparentWindSpeed * apparentWindSpeed * sailArea * dragCoefficient;

        if (apparentWindAngle > 130f || apparentWindAngle < -130f)
        {
            drag = 0;
        }
        else if (apparentWindAngle > 40f)
        {
            drag *= Mathf.SmoothStep(0.1f, 1f, (apparentWindAngle - 40f) / 50f);
        }
        else if (apparentWindAngle < -40f)
        {
            drag *= Mathf.SmoothStep(0.1f, 1f, (-apparentWindAngle - 40f) / 50f);
        }
        else
        {
            drag *= .1f;
        }
        

        //make sure that the drag value decreased when the apparentWindAngle moves away from 180 degrees
        ApplyDrag(drag, apparentWindAngle);
    }

    //Calculates the force of lift experienced on the sail, used when broad, beam -reach, and close hauled
    void CalculateLift(float apparentWindSpeed, float apparentWindAngle)
    {
        lift = .5f * airDensity * apparentWindSpeed * apparentWindSpeed * sailArea * liftCoefficient;

        if (apparentWindAngle > 90f || apparentWindAngle < -90f)
        {
            lift = 0f;
        }
        else if (apparentWindAngle < 70f && apparentWindAngle > 0f)
        {
            lift *= Mathf.SmoothStep(1f, .01f, apparentWindAngle / 70f);
        }
        else if (apparentWindAngle > -70f && apparentWindAngle < 0f)
        {
            lift *= Mathf.SmoothStep(1f, .01f, -apparentWindAngle / 70f);
        }
        else
        {
            lift = 0f;
        }

        //make sure that the lift value decreased when the apparentWindAngle moves away from being between around 30 to 135 degrees
        ApplyLift(lift, apparentWindAngle);
    }

    void ApplyLift(float lift, float apparentWindAngle)
    {
        //calculate the drag vector perpendicular to the apparent wind direction
        Vector3 liftVector = Vector3.ProjectOnPlane(apparentWind, Vector3.up).normalized;

        if (apparentWindAngle > 0f)
            liftVector = Quaternion.AngleAxis(90f, Vector3.up) * liftVector;
        else
            liftVector = Quaternion.AngleAxis(-90f, Vector3.up) * liftVector;

        liftVector *= lift;

        liftVector = hullObject.transform.InverseTransformVector(liftVector);

        //retrieve the local y component of the lift vector
        Vector3 localLiftVector = new Vector3(0f, liftVector.y, 0f);
        liftVector = hullObject.transform.TransformVector(localLiftVector);

        rigidBody.AddForce(liftVector, ForceMode.Force);
    }

    void ApplyDrag(float drag, float apparentWindAngle)
    {
        //calculate the drag vector parallel to the apparent wind direction
        Vector3 dragVector = Vector3.ProjectOnPlane(apparentWind, Vector3.up).normalized * drag;

        dragVector = hullObject.transform.InverseTransformVector(dragVector);

        //retrieve the local y component of the drag vector
        Vector3 LocalDragVector = new Vector3(0f, dragVector.y, 0f);
        dragVector = hullObject.transform.TransformVector(LocalDragVector);

        rigidBody.AddForce(dragVector, ForceMode.Force);
    }

    void ApplyRudderTorque()
    {
        float forwardVelocity = transform.InverseTransformVector(rigidBody.linearVelocity).z;
        rigidBody.AddTorque(Vector3.up * rudderAxis.value * rudderTorqueStrength * forwardVelocity, ForceMode.Force);
        rudderTorque = rudderAxis.value * rudderTorqueStrength * forwardVelocity;
    }

    void ApplyKeelDrag()
    {
        Vector3 localVelocity = transform.InverseTransformVector(rigidBody.linearVelocity);

        Vector3 keelDragVector = Vector3.right * localVelocity.x * keelDragStrength;

        rigidBody.AddRelativeForce(-keelDragVector);
    }

    void applyWaterDrag()
    {
        Vector3 localVelocity = transform.InverseTransformVector(rigidBody.linearVelocity);

        float waterDrag = .5f * waterDensity * Mathf.Pow(localVelocity.z, 2f) * .9f * underwaterFrontArea * Mathf.Sign(localVelocity.z);

        rigidBody.AddRelativeForce(-Vector3.forward * waterDrag);
    }

    void ApplyBaseForwardForce()
    {
        rigidBody.AddRelativeForce(Vector3.forward * baseForwardForce, ForceMode.Force);
    }

    private float currentMastAngle = 0f;

    private void RotateMastAndSail()
    {
        float targetMastAngle = mastAxis.value * maxMastAngle;
        float deltaMastAngle = targetMastAngle - currentMastAngle;

        mastObject.transform.RotateAround(mastPivot.transform.position, Vector3.up, deltaMastAngle);
        sailObject.transform.RotateAround(mastPivot.transform.position, Vector3.up, deltaMastAngle);

        currentMastAngle = targetMastAngle;
    }
}
