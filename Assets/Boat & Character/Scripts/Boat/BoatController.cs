/*
*   Created by Johan Beimers
*   Contributed to by: 
*/

using UnityEngine;

public class BoatController : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private Vector3 wind = Vector3.zero;
    [SerializeField] private Vector3 apparentWind = Vector3.zero;

    [Header("Boat stats")]
    [SerializeField] private float maxRudderDeflection = 20f;

    [Header("Controls")]
    [SerializeField] private SmoothAxis2D rudderAxis;
    [SerializeField] private SmoothAxis2D mainsheetAxis;

    [Header("")]
    [SerializeField] private GameObject rudderObject;
    [SerializeField] private GameObject telltaleObject;


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
        //rigidBody.AddRelativeForce(new Vector3(40f ,0f, 0f), ForceMode.Force);
        //rigidBody.AddRelativeTorque(new Vector3(0f, 10f, 0f), ForceMode.Force);
    }

    private void RotateRudder()
    {
        Vector3 rudderRotationPoint = rudderObject.transform.position + rudderObject.transform.forward;
        Vector3 rudderAxis = rudderObject.transform.up;
        rudderObject.transform.RotateAround(rudderRotationPoint, rudderAxis, 10f * Time.deltaTime);
        
    }
}
