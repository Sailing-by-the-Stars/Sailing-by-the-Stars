/*
*   Created by Johan Beimers
*   Contributed to by: 
*/

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BoatController : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private Vector3 wind = Vector3.zero;
    [SerializeField] private Vector3 apparentWind = Vector3.zero;
    [SerializeField] private float dragCoefficient = 1.1f;
    [SerializeField] private float liftCoefficient = 1.5f;
    [SerializeField] private float sailArea = 20;
    [SerializeField] private float airDensity = 1.225f;
    [SerializeField] private float torqueStrength = 10f;

    [Header("Boat stats")]
    [SerializeField] private float maxRudderDeflection = 20f;

    [Header("Controls")]
    [SerializeField] private SmoothAxis2D rudderAxis;
    [SerializeField] private SmoothAxis2D mainsheetAxis;

    [Header("")]
    [SerializeField] private GameObject hullObject;
    [SerializeField] private GameObject rudderObject;
    [SerializeField] private GameObject mastObject;
    [SerializeField] private GameObject sailObject;


    private Rigidbody rigidBody;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        //RotateRudder();
    }

    void FixedUpdate()
    {
        apparentWind = wind - rigidBody.linearVelocity;
        Vector3 mastDirection = Vector3.ProjectOnPlane(mastObject.transform.up, Vector3.up).normalized;
        Vector3 windDirection = Vector3.ProjectOnPlane(apparentWind, Vector3.up).normalized;

        float mastDirectionIntoWind = Vector3.SignedAngle(-mastDirection, windDirection, Vector3.up);
        //rigidBody.AddRelativeForce(new Vector3(40f ,0f, 0f), ForceMode.Force);
        //rigidBody.AddRelativeTorque(new Vector3(0f, 10f, 0f), ForceMode.Force);

        CalculateDrag(apparentWind.magnitude, mastDirectionIntoWind);
        CalculateLift(apparentWind.magnitude, mastDirectionIntoWind);
    }

    //Calculates the force of drag experienced on the sail, used when running downwind and broadreach
    void CalculateDrag(float apparentWindSpeed, float apparentWindAngle)
    {
        float drag = .5f * airDensity * apparentWindSpeed * apparentWindSpeed * sailArea * dragCoefficient;

        if (apparentWindAngle > 135f)
        {
            drag *= Mathf.SmoothStep(0.1f, 1f, (apparentWindAngle - 135f) / 45f);
        }
        else if (apparentWindAngle < -135f)
        {
            drag *= Mathf.SmoothStep(0.1f, 1f, (-apparentWindAngle - 135f) / 45f);
        }
        else
            drag *= .1f;

        //Debug.Log("Drag: " + drag);

        //make sure that the drag value decreased when the apparentWindAngle moves away from 180 degrees
        ApplyDrag(drag, apparentWindAngle);
    }

    //Calculates the force of lift experienced on the sail, used when broad, beam -reach, and close hauled
    void CalculateLift(float apparentWindSpeed, float apparentWindAngle)
    {
        float lift = .5f * airDensity * apparentWindSpeed * apparentWindSpeed * sailArea * liftCoefficient;

        if (apparentWindAngle < 135f && apparentWindAngle > 0f)
        {
            lift *= Mathf.SmoothStep(1f, .1f, apparentWindAngle / 135f);
        }
        else if (apparentWindAngle > -135f && apparentWindAngle < 0f)
        {
            lift *= Mathf.SmoothStep(1f, .1f, -apparentWindAngle / 135f);
        }
        else
            lift *= .1f;

        //Debug.Log("Lift: " + lift);

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
        
        Debug.Log("Lift Vector: " + liftVector);
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

        Debug.Log("Drag Vector: " + dragVector);
        rigidBody.AddForce(dragVector, ForceMode.Force);
    }

    private void RotateRudder()
    {
        Vector3 rudderRotationPoint = rudderObject.transform.position + rudderObject.transform.forward;
        Vector3 rudderAxis = rudderObject.transform.up;
        rudderObject.transform.RotateAround(rudderRotationPoint, rudderAxis, 10f * Time.deltaTime);
        
    }
}
