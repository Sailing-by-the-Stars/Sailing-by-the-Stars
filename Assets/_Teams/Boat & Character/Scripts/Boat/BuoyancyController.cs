/*
*   Created by Johan Beimers
*   Contributed to by: 
*/

using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(Rigidbody))]
public class BuoyancyController : MonoBehaviour
{
    [Header("Physics settings")]
    [SerializeField] private float draft = 0f;
    [SerializeField] [Range(1, 4)] private int accuracy = 1;
    [SerializeField] float waveTorqueStrength = 1f;
    [SerializeField] Vector3 centerOfMass = Vector3.zero;

    [Header("Object references")]

    [SerializeField] private WaterSurface waterSurface;
    [SerializeField] private GameObject buoyancyQuadObject;
    private List<Vector3> buoyancyVertices = new();

    private WaterSearchResult[] searchResult;
    private Rigidbody rigidBody;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateExtraVertices();

        Vector3 buoyancyObjectPosition = new Vector3(0f, -draft, 0f);
        buoyancyQuadObject.transform.position = buoyancyObjectPosition;

        searchResult = new WaterSearchResult[buoyancyVertices.Count];

        rigidBody = GetComponent<Rigidbody>();
        rigidBody.centerOfMass = centerOfMass;

        Debug.Log("CoM: " + rigidBody.centerOfMass);
    }

    void CreateExtraVertices()
    {
        for (int x = 0; x < accuracy + 1; x++) {
            for (int y = 0; y < accuracy + 1; y++)
            {
                buoyancyVertices.Add(new Vector3(-0.5f + ((float)x / accuracy), -0.5f + ((float)y /accuracy)));
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateHeight();
        UpdateWaveTorque();
    }

    private WaterSearchParameters searchParameters = new();
    private float waterHeight = 0f;

    void UpdateHeight()
    {
        waterHeight = 0f;

        for (int i = 0; i < buoyancyVertices.Count; i++)
        {
            Vector3 globalVertex = buoyancyQuadObject.transform.TransformPoint(buoyancyVertices[i]);

            searchParameters.startPositionWS = searchResult[i].projectedPositionWS;
            searchParameters.targetPositionWS = globalVertex;
            searchParameters.error = 0.01f;
            searchParameters.maxIterations = 6;

            if (waterSurface.ProjectPointOnWaterSurface(searchParameters, out searchResult[i]))
            {
                waterHeight += searchResult[i].projectedPositionWS.y;
            }
        }

        waterHeight /= buoyancyVertices.Count;

        Vector3 boatPosition = transform.position;
        boatPosition.y = waterHeight + draft;
        transform.position = boatPosition;
    }

    void UpdateWaveTorque()
    {
        for (int i = 0; i < buoyancyVertices.Count; i++)
        {
            Vector3 globalVertex = buoyancyQuadObject.transform.TransformPoint(buoyancyVertices[i]);

            float deltaHeight = globalVertex.y - searchResult[i].projectedPositionWS.y;
            float accuracyDivider = 1f / Mathf.Pow(accuracy + 1, 2f); 

            Vector3 torqueVector = new Vector3(buoyancyVertices[i].y * 3 * deltaHeight * waveTorqueStrength * accuracyDivider, 0f, -buoyancyVertices[i].x * deltaHeight * waveTorqueStrength * accuracyDivider);
            rigidBody.AddRelativeTorque(torqueVector, ForceMode.Force);
        }
    }
}
