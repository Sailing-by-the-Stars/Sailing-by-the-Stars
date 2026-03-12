using System.Collections.Generic;
using UnityEngine;

public class GlobeShape : MonoBehaviour
{
    [SerializeField] float globeRadius = 5;
    [SerializeField] int resolution = 10;

    List<Vector3> verticePositions = new();
    List<Vector3> vertices = new();
    List<int> triangles = new();
    Mesh myMesh;
    MeshFilter meshFilter;


    private void Awake()
    {
        myMesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = myMesh;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //DrawShape(resolution, resolution);

        //myMesh = GetComponent<MeshFilter>().mesh;
        //myMesh = new();
    }

    // Update is called once per frame
    void Update()
    {

        GeneratePlane(globeRadius * 2, resolution);

        OffsetMesh();

        AssignMesh();
    }



    void OffsetMesh()
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 vertex = vertices[i];
            vertex.y = GetZ(globeRadius, vertex.x - globeRadius, vertex.z - globeRadius);
            vertices[i] = vertex;
        }
    }




    void DrawShape(int xSteps, int ySteps)
    {
        float currentX = -globeRadius;
        float currentY = -globeRadius;
        float currentZ = 0;

        float xStepSize = (globeRadius * 2) / xSteps; 
        float yStepSize = (globeRadius * 2) / ySteps;

        verticePositions.Clear();

        for (int i = 0; i < xSteps; i++)
        {
            for (int j = 0; j < ySteps; j++)
            {
                currentZ = 0;
                currentZ = GetZ(globeRadius, xStepSize * i, yStepSize * j);

                verticePositions.Add(new Vector3(currentX, currentY, currentZ));
            }
        }


    }



    float GetZ(float radius, float x, float y)
    {
        float z = 0;
        z = Mathf.Sqrt(Mathf.Pow(radius, 2) - (Mathf.Pow(x, 2) + Mathf.Pow(y, 2)));

        if(z.ToString() == "NaN")
        {
            z = 0;
        }

        Debug.Log($"radius '{radius}', x '{x}', and y '{y}' give z '{z}'");
        return z;
    }


    void GeneratePlane(float size, int resolution)
    {
        vertices = new List<Vector3>();
        float xStepSize = (size) / resolution;
        float yStepSize = (size) / resolution;

        for (int y = 0; y < resolution + 1; y++)
        {
            for (int x = 0; x < resolution + 1; x++)
            {
                vertices.Add(new Vector3(x * xStepSize, 0, y * yStepSize));
            }
        }

        triangles = new List<int>();

        for (int row = 0; row < resolution; row++)
        {
            for (int col = 0; col < resolution; col++)
            {
                int i = (row * resolution) + row + col;

                triangles.Add(i);
                triangles.Add(i + resolution + 1);
                triangles.Add(i + resolution + 2);

                triangles.Add(i);
                triangles.Add(i + resolution + 2);
                triangles.Add(i + 1);

            }
        }
    }


    void AssignMesh()
    {
        myMesh.Clear();
        myMesh.vertices = vertices.ToArray();
        myMesh.triangles = triangles.ToArray();
    }
}
