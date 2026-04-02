using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GlobeShape : MonoBehaviour
{
    [SerializeField] float globeRadius = 5;
    [SerializeField] int resolution = 10;
    [SerializeField] float offsetY = 5000f;

    List<Vector3> verticePositions = new();
    List<Vector3> vertices = new();
    List<int> triangles = new();
    Mesh myMesh;
    MeshFilter meshFilter;


    List<TwinklingStar> relatedStars = new();
    List<StarInfo> relatedMiniStars = new();

    private void OnEnable()
    {
        StarFieldRotator.globeTypeChanged += ToggleStars;
    }

    private void OnDisable()
    {
        StarFieldRotator.globeTypeChanged -= ToggleStars;
    }



    void ToggleStars(globeType globeType)
    {
        switch (globeType)
        {
            case globeType.relative:
                foreach (TwinklingStar star in relatedStars)
                {
                    star.gameObject.SetActive(false);
                }
                foreach (StarInfo star in relatedMiniStars)
                {
                    star.gameObject.SetActive(false);
                }
                break;
            case globeType.positional:
                foreach (TwinklingStar star in relatedStars)
                {
                    star.gameObject.SetActive(false);
                }
                foreach (StarInfo star in relatedMiniStars)
                {
                    star.gameObject.SetActive(false);
                }
                break;
            case globeType.manual:
                foreach (TwinklingStar star in relatedStars)
                {
                    star.gameObject.SetActive(true);
                }
                foreach (StarInfo star in relatedMiniStars)
                {
                    star.gameObject.SetActive(true);
                }
                break;
            default:
                Debug.LogError("it should never switch to this state!");
                break;
        }
    }


    private void Awake()
    {
        relatedStars = GetComponentsInChildren<TwinklingStar>().ToList();
        relatedMiniStars = GetComponentsInChildren<StarInfo>().ToList();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GetComponent<MeshRenderer>() == null && GetComponent<MeshRenderer>().enabled == false)
        {
            return;
        }
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

        targetpos.y -= offsetY;

        transform.position = targetpos;


        foreach (TwinklingStar star in relatedStars)
        {
            Vector3 starTargetpos = star.initpos;

            star.transform.position = starTargetpos;

            starTargetpos.y = GetY(star.transform.localPosition.x, star.transform.localPosition.z);

            star.transform.position = starTargetpos;

            star.transform.LookAt(Camera.main.transform.position);
            star.transform.Rotate(0, 180, 0);
        }

        foreach (StarInfo star in relatedMiniStars)
        {
            Vector3 starTargetpos = star.initpos;

            star.transform.position = starTargetpos;

            starTargetpos.y = GetY(star.transform.localPosition.x, star.transform.localPosition.z);

            star.transform.position = starTargetpos;

            star.transform.LookAt(Camera.main.transform.position);
            star.transform.Rotate(0, 180, 0);
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
        x -= globeRadius;
        z -= globeRadius;

        float y = 0;
        y = Mathf.Sqrt(Mathf.Pow(globeRadius, 2) - (Mathf.Pow(x, 2) + Mathf.Pow(z, 2)));

        y += transform.position.y;

        if (y.ToString() == "NaN")
        {
            y = 0;
        }

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
        if(GetComponent<MeshRenderer>() == null || GetComponent<MeshRenderer>().enabled == false)
        {
            return;
        }
        
        myMesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = myMesh;
        
        DrawSphere();
    }
}
