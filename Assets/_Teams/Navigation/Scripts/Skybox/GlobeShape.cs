using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GlobeShape : MonoBehaviour
{
    [SerializeField] float globeRadius = 5;
    [SerializeField] int resolution = 10;
    [SerializeField] float offsetY = 5000f;
    [Tooltip("this determines over how manny frames the moving of the stars will be divided")]
    [SerializeField] int operationDivisions = 3; 


    List<Vector3> verticePositions = new();
    List<Vector3> vertices = new();
    List<int> triangles = new();
    Mesh myMesh;
    MeshFilter meshFilter;


    List<TwinklingStar> relatedStars = new();
    List<StarInfo> relatedMiniStars = new();
    List<List<StarInfo>> dividedMiniStars = new();
    int Iterator;
    public int iterator
    {
        get => Iterator;
        set => Iterator = (value >= dividedMiniStars.Count) ? 0 : value;
    }


    private void Awake()
    {
        relatedStars = GetComponentsInChildren<TwinklingStar>().ToList();

        InitializeMiniStars(operationDivisions);
    }


    void Start()
    {
        if (GetComponent<MeshRenderer>() == null && GetComponent<MeshRenderer>().enabled == false)
        {
            return;
        }
        DrawSphere();
    }


    void InitializeMiniStars(int divisions)
    {
        if (divisions < 1)
        {
            divisions = 1;
        }

        operationDivisions = divisions;

        relatedMiniStars = GetComponentsInChildren<StarInfo>().ToList();
        dividedMiniStars.Clear();

        if (relatedMiniStars.Count <= operationDivisions)
        {
            dividedMiniStars.Add(relatedMiniStars);
            operationDivisions = 1;
            return;
        }

        int starsPerDiv = Mathf.CeilToInt((float)relatedMiniStars.Count / operationDivisions);
        List<StarInfo> starsToDiv = new(relatedMiniStars);   

        for (int i = 0; i < operationDivisions && starsToDiv.Count > 0; i++)
        {
            List<StarInfo> remainderStars = starsToDiv.Take(starsPerDiv).ToList();
            
            dividedMiniStars.Add(remainderStars);

            if(remainderStars.Count < starsPerDiv)
            {
                break;
            }

            starsToDiv = starsToDiv.Skip(starsPerDiv).ToList();
        }

        iterator = 0;
    }


    void Update()
    {
        MoveSphere();

        MoveStars(relatedStars, dividedMiniStars[iterator]);
        iterator++;
    }


    void MoveSphere()
    {
        Vector3 targetpos = Camera.main.transform.position;

        targetpos.x -= globeRadius;
        targetpos.z -= globeRadius;

        targetpos.y -= offsetY;

        transform.position = targetpos;
    }

    void MoveStars(List<TwinklingStar> tStars, List<StarInfo> iStars)
    {
        float starTargetY = 0;
        foreach (TwinklingStar star in tStars)
        {
            starTargetY = GetY(star.initpos.x, star.initpos.z);

            star.transform.position = new Vector3(star.initpos.x, starTargetY, star.initpos.z);


            if (starTargetY > 0)
            {
                star.transform.LookAt(Camera.main.transform.position);
            }
        }

        foreach (StarInfo star in iStars)
        {
            starTargetY = GetY(star.initpos.x, star.initpos.z);

            star.transform.position = new Vector3(star.initpos.x, starTargetY, star.initpos.z);


            if (starTargetY > 0)
            {
                star.transform.LookAt(Camera.main.transform.position);
            }
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

        //Debug.Log($"radius '{radius}', x '{x}', and y '{z}' give z '{y}'");
        return y;
    }

    float GetY(float x, float z)
    {
        x -= globeRadius + transform.position.x;
        z -= globeRadius + transform.position.z;

        float y = 0;
        y = Mathf.Sqrt(Mathf.Pow(globeRadius, 2) - (Mathf.Pow(x, 2) + Mathf.Pow(z, 2)));

        if (float.IsNaN(y))
        {
            y = 0;
        }

        y += transform.position.y;


        //Debug.Log($"global position x '{x}', and y '{z}' give z '{y}'");
        
        return y;
    }


    void GeneratePlane(float size, int resolution)
    {
        vertices = new List<Vector3>();
        float xStepSize = (size) / resolution;
        float zStepSize = (size) / resolution;

        for (int z = 0; z < resolution + 1; z++)
        {
            for (int x = 0; x < resolution + 1; x++)
            {
                vertices.Add(new Vector3(x * xStepSize, 0, z * zStepSize));
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
        if (GetComponent<MeshRenderer>() == null || GetComponent<MeshRenderer>().enabled == false)
        {
            return;
        }
        myMesh.Clear();
        myMesh.vertices = vertices.ToArray();
        myMesh.triangles = triangles.ToArray();
    }


    private void OnValidate()
    {
        if (Camera.main == null)
        {
            return;
        }


        Vector3 targetpos = Camera.main.transform.position;

        targetpos.x -= globeRadius;
        targetpos.z -= globeRadius;

        targetpos.y -= offsetY;

        transform.position = targetpos;

        InitializeMiniStars(operationDivisions);

        if (GetComponent<MeshRenderer>() == null || GetComponent<MeshRenderer>().enabled == false)
        {
            return;
        }
        
        myMesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = myMesh;
        
        DrawSphere();
    }
}
