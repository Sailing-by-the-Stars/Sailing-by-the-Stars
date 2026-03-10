/*
*   Created by Johan Beimers
*   Contributed to by: 
*/

using UnityEngine;
using UnityEngine.UIElements;

public class BoatController : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private Vector3 wind = Vector3.zero;
    [SerializeField] private Vector3 apparentWind = Vector3.zero;
    [SerializeField] private float dragCoefficient = 1.5f;
    [SerializeField] private float liftCoefficient = 1.2f;
    [SerializeField] private float sailArea = 20;
    [SerializeField] private float airDensity = 1.225f;

    [Header("Boat stats")]
    [SerializeField] private float maxRudderDeflection = 20f;

    [Header("Controls")]
    [SerializeField] private SmoothAxis2D rudderAxis;
    [SerializeField] private SmoothAxis2D mainsheetAxis;

    [Header("")]
    [SerializeField] private GameObject rudderObject;
    [SerializeField] private GameObject telltaleObject;
    [SerializeField] private GameObject mastObject;


    private Rigidbody rigidBody;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        RotateRudder();
    }

    void FixedUpdate()
    {
        apparentWind = wind - rigidBody.linearVelocity;
        Vector3 mastDirection = Vector3.ProjectOnPlane(mastObject.transform.right, Vector3.up).normalized;
        Vector3 windDirection = Vector3.ProjectOnPlane(apparentWind, Vector3.up).normalized;

        float mastDirectionIntoWind = Vector3.SignedAngle(-mastDirection, windDirection, Vector3.up);

        Debug.Log("Angle of Attack: " + mastDirectionIntoWind);
        //rigidBody.AddRelativeForce(new Vector3(40f ,0f, 0f), ForceMode.Force);
        //rigidBody.AddRelativeTorque(new Vector3(0f, 10f, 0f), ForceMode.Force);

        float drag = CalculateDrag(apparentWind.magnitude, mastDirectionIntoWind);
        float lift = CalculateLift(apparentWind.magnitude, mastDirectionIntoWind);
    }

    //Calculates the force of drag experienced on the sail, used when running downwind and broadreach
    float CalculateDrag(float apparentWindSpeed, float apparentWindAngle)
    {
        float drag = .5f * airDensity * apparentWindSpeed * apparentWindSpeed * sailArea * dragCoefficient;

        //make sure that the drag value decreased when the apparentWindAngle moves away from 180 degrees
        return drag;
    }

    //Calculates the force of lift experienced on the sail, used when broad, beam -reach, and close hauled
    float CalculateLift(float apparentWindSpeed, float apparentWindAngle)
    {
        float lift = .5f * airDensity * apparentWindSpeed * apparentWindSpeed * sailArea * liftCoefficient;

        //make sure that the lift value decreased when the apparentWindAngle moves away from being between around 30 to 135 degrees
        return lift;
    }

    private void RotateRudder()
    {
        Vector3 rudderRotationPoint = rudderObject.transform.position + rudderObject.transform.forward;
        Vector3 rudderAxis = rudderObject.transform.up;
        rudderObject.transform.RotateAround(rudderRotationPoint, rudderAxis, 10f * Time.deltaTime);
        
    }
}
