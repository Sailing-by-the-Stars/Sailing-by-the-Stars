using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


enum globeType
{
    relative,
    positional,
    manual
}


public class StarFieldRotator : MonoBehaviour
{
    [Header("Planet Settings")]
    public float planetRadius = 1000f;

    private Vector3 lastPlayerPos;



    [SerializeField] globeType globeType = globeType.relative;


    void Start()
    {
        lastPlayerPos = Camera.main.transform.position;
    }

    void Update()
    {

        transform.position = Camera.main.transform.position;


        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            switch (globeType)
            {
                case globeType.relative:
                    globeType = globeType.manual;
                    break;
                case globeType.positional:
                    globeType = globeType.relative;
                    break;
                case globeType.manual:
                    globeType = globeType.positional;
                    break;
                default:
                    break;
            }
        }



        if (globeType == globeType.positional)
        {
            float degreesPerUnit = 360f / (2f * Mathf.PI * planetRadius);

            float yaw = Camera.main.transform.position.x * degreesPerUnit;   // around Y
            float pitch = -Camera.main.transform.position.z * degreesPerUnit; // around X

            // First rotate around global Y (longitude)
            Quaternion yawRot = Quaternion.AngleAxis(yaw, Vector3.right);

            // Then rotate around the *rotated* right axis (latitude)
            Vector3 rightAxis = yawRot * Vector3.forward;
            Quaternion pitchRot = Quaternion.AngleAxis(-pitch, rightAxis);

            // Combine them in the correct order
            transform.rotation = pitchRot * yawRot;

            // Keep centered
            transform.position = Camera.main.transform.position;
        }
        else if( globeType == globeType.relative) 
        {
            Vector3 delta = Camera.main.transform.position - lastPlayerPos;

            // Only care about horizontal movement
            float deltaX = delta.x;
            float deltaZ = delta.z;

            // Convert movement to rotation (in degrees)
            float rotationFactor = Mathf.Rad2Deg / planetRadius;

            float rotZ = deltaX * rotationFactor;   // Move in X ? rotate around Z
            float rotX = -deltaZ * rotationFactor;  // Move in Z ? rotate around X

            // Apply rotation in local space
            transform.Rotate(rotX, 0f, rotZ, Space.World);

            lastPlayerPos = Camera.main.transform.position;

        }else
        {

        }
    }

}
