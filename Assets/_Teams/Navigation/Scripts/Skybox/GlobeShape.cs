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


    List<TwinklingStar> relatedStars = new();


    private void Awake()
    {
        myMesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = myMesh;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DrawSphere();
    }

    // Update is called once per frame
    void Update()
    {
        MoveSphere();
    }


    void MoveSphere()
    {
        Vector3 targetpos = Camera.main.transform.position;

        targetpos.x -= globeRadius;
        targetpos.z -= globeRadius;

        transform.position = targetpos;


        foreach (TwinklingStar star in relatedStars)
        {
            Vector3 starTargetpos = star.initpos;

            starTargetpos.y = GetY(globeRadius, star.initpos.x, star.initpos.y);

            star.transform.LookAt(transform.position);
            star.transform.Rotate(0, 180, 0);

            star.transform.position = starTargetpos;
        }
    }


    void DrawSphere()
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
            vertex.y = GetY(globeRadius, vertex.x - globeRadius, vertex.z - globeRadius);
            vertices[i] = vertex;
        }
    }

    float GetY(float radius, float x, float z)
    {
        float y = 0;
        y = Mathf.Sqrt(Mathf.Pow(radius, 2) - (Mathf.Pow(x, 2) + Mathf.Pow(z, 2)));

        if(y.ToString() == "NaN")
        {
            y = 0;
        }

        Debug.Log($"radius '{radius}', x '{x}', and y '{z}' give z '{y}'");
        return y;
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


    private void OnValidate()
    {
        myMesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = myMesh;

        DrawSphere();

        Vector3 targetpos = Camera.main.transform.position;

        targetpos.x -= globeRadius;
        targetpos.z -= globeRadius;

        transform.position = targetpos;
    }
}
