/*
*   Created by Johan Beimers
*   Contributed to by: 
*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class BuoyancyController : MonoBehaviour
{
    [SerializeField] private float draft = 0f;
    [SerializeField] private WaterSurface waterSurface;
    [SerializeField] private GameObject buoyancyQuadObject;
    private Mesh buoyancyMesh;
    private List<Vector3> buoyancyVertices = new List<Vector3>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buoyancyMesh = buoyancyQuadObject.GetComponent<MeshFilter>().mesh;
        Debug.Log("Amount of vertices: " + buoyancyMesh.vertices.Length);

        foreach (Vector3 vertex in buoyancyMesh.vertices)
        {
            Debug.Log("Vertex: " + vertex);
            buoyancyVertices.Add(buoyancyQuadObject.transform.TransformPoint(vertex));
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateHeight();
    }

    WaterSearchParameters searchParameters = new WaterSearchParameters();
    WaterSearchResult searchResult = new WaterSearchResult();

    void UpdateHeight()
    {
        float waterHeight = 0f;
        foreach (Vector3 vertex in buoyancyVertices)
        {
            searchParameters.targetPositionWS = vertex;
            searchParameters.error = 0.1f;
            searchParameters.maxIterations = 4;

            if (waterSurface.ProjectPointOnWaterSurface(searchParameters, out searchResult))
            {
                waterHeight += searchResult.projectedPositionWS.y;
            }
        }

        waterHeight /= buoyancyVertices.Count;

        Debug.Log("Average water height: " + waterHeight);

        transform.position = new Vector3(0f, waterHeight - draft, 0f);
    }
}
