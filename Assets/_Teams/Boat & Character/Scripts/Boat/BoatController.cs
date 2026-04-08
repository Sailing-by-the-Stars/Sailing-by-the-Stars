/*
*   Created by Johan Beimers
*   Contributed to by: 
*/

using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private float keelDragStrength = 100f;
    [SerializeField] private float baseForwardForce = 900f;
    [SerializeField] private float maxRotationRate = 10f;
    [SerializeField] private float rudderTorqueStrength = 30f;
    [SerializeField] private float sidedriftCorrectionStrength = 150f;
    [SerializeField] private bool useLocalWindSpeed = true;
    [SerializeField] private bool enableWindForces = true;

    [Header("Physics stats (Debugging!)")]
    [SerializeField] private float forwardSpeed;
    [SerializeField] private float apparentWindSpeed;
    [SerializeField] private float sideSlipSpeed;
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
    [SerializeField] public SmoothAxis2D rudderAxis;
    [SerializeField] public SmoothAxis2D mastAxis;

    [Header("")]
    [SerializeField] private GameObject hullObject;
    [SerializeField] private GameObject mastPivot;
    [SerializeField] private GameObject rudderPivot;


    private Rigidbody rigidBody;
    private GameObject[] rudderObjects;
    private GameObject[] mastObjects;

    private bool anchorDropped = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();

        rudderObjects = GameObject.FindGameObjectsWithTag("rudder");
        if (rudderObjects.Count() == 0)
        {
            Debug.LogError("No objects with tag \"rudder\" found");
            enabled = false;
            return;
        }

        mastObjects = GameObject.FindGameObjectsWithTag("mast");
        if (rudderObjects.Count() == 0)
        {
            Debug.LogError("No objects with tag \"mast\" found");
            enabled = false;
            return;
        }
    }

    void OnEnable()
    {
        rudderAxis.Reset();
        mastAxis.Reset();
    }

    // Update is called once per frame
    void Update()
    {
        RotateRudder();
        RotateMastAndSail();
    }

    void FixedUpdate()
    {
        if (!useLocalWindSpeed && WeatherManager.Instance != null)
        {
            wind = WeatherManager.Instance.WindVelocity;
        }

        apparentWind = wind - rigidBody.linearVelocity;
        apparentWindSpeed = apparentWind.magnitude;

        Vector3 mastDirection = Vector3.ProjectOnPlane(mastObjects[0].transform.right, Vector3.up).normalized;
        Vector3 windDirection = Vector3.ProjectOnPlane(apparentWind, Vector3.up).normalized;

        float mastDirectionIntoWind = Vector3.SignedAngle(mastDirection, -windDirection, Vector3.up);
        if (!anchorDropped)
        {
            if (enableWindForces)
            {
                CalculateDrag(apparentWind.magnitude, mastDirectionIntoWind);
                CalculateLift(apparentWind.magnitude, mastDirectionIntoWind);
            }

            AoA = mastDirectionIntoWind;

            ApplyBaseForwardForce();
            ApplyRudderTorque();
            ApplyKeelDrag();
        }

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

        //retrieve the local x component of the lift vector
        Vector3 localLiftVector = new Vector3(liftVector.x, 0f, 0f);
        liftVector = hullObject.transform.TransformVector(localLiftVector);

        rigidBody.AddForce(liftVector, ForceMode.Force);
    }

    void ApplyDrag(float drag, float apparentWindAngle)
    {
        //calculate the drag vector parallel to the apparent wind direction
        Vector3 dragVector = Vector3.ProjectOnPlane(apparentWind, Vector3.up).normalized * drag;

        dragVector = hullObject.transform.InverseTransformVector(dragVector);

        //retrieve the local x component of the drag vector
        Vector3 LocalDragVector = new Vector3(dragVector.x, 0f, 0f);
        dragVector = hullObject.transform.TransformVector(LocalDragVector);

        rigidBody.AddForce(dragVector, ForceMode.Force);
    }

    void ApplyRudderTorque()
    {
        Vector3 localVelocity = transform.InverseTransformVector(rigidBody.linearVelocity);
        float forwardVelocity = localVelocity.z;
        rigidBody.AddTorque(forwardVelocity * rudderAxis.value * rudderTorqueStrength * Vector3.up, ForceMode.Force);
        rudderTorque = rudderAxis.value * rudderTorqueStrength * forwardVelocity;

        if (Mathf.Abs(localVelocity.x) > .2f)
        {
            Vector3 sideSlipForce = -localVelocity.x * sidedriftCorrectionStrength * Vector3.right;
            rigidBody.AddRelativeForce(sideSlipForce, ForceMode.Force);
        }

        rotationRate = rigidBody.angularVelocity.y * Mathf.Rad2Deg;
    }

    void ApplyKeelDrag()
    {
        Vector3 localVelocity = transform.InverseTransformVector(rigidBody.linearVelocity);

        Vector3 localKeelDragVector = -keelDragStrength * localVelocity.x * Vector3.right;
        Vector3 keelDragVector = transform.TransformVector(localKeelDragVector);


        rigidBody.AddRelativeForce(keelDragVector);
        rigidBody.AddForceAtPosition(keelDragVector, transform.position + .65f * -transform.up, ForceMode.Force);

        sideSlipSpeed = localVelocity.x;
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

        foreach(GameObject mastObject in mastObjects)
        {
            mastObject.transform.RotateAround(mastPivot.transform.position, Vector3.up, deltaMastAngle);
        }

        currentMastAngle = targetMastAngle;
    }

    private float currentRudderAngle = 0f;
    
    private void RotateRudder()
    {
        float targetRudderAngle = rudderAxis.value * -maxRudderDeflection;
        float deltaRudderAngle = targetRudderAngle - currentRudderAngle;

        foreach(GameObject rudderObject in rudderObjects)
        {
            rudderObject.transform.RotateAround(rudderPivot.transform.position, rudderPivot.transform.up, deltaRudderAngle);
        }

        currentRudderAngle = targetRudderAngle;
    }

    public void DropAnchor()
    {
        anchorDropped = true;
        rigidBody.linearDamping = .5f;
    }

    public void HaulAnchor()
    {
        anchorDropped = false;
        rigidBody.linearDamping = 0f;
    }
}
