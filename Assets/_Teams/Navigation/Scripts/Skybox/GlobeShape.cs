using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using static StarDataLoader;

public class GlobeShape : MonoBehaviour
{
    [SerializeField] float globeRadius = 5;
    [SerializeField] int resolution = 10;
    [SerializeField] float offsetY = 5000f;
    [Tooltip("this determines over how manny frames the moving of the stars will be divided")]
    [SerializeField] int operationDivisions = 5;
    [SerializeField] GlobePositionType positionType;

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
        if (positionType == null)
        {
            positionType = new XZPosition();
        }

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

            if (remainderStars.Count < starsPerDiv)
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

        MoveStars(relatedStars, dividedMiniStars[iterator], Camera.main.transform);
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

    // Move stars with multi-core Jobs
    public void MoveStars(List<TwinklingStar> tStars, List<StarInfo> iStars, Transform playerCamera)
    {
        NativeArray<Vector3> starPositions = new NativeArray<Vector3>(iStars.Count, Allocator.TempJob);
        NativeArray<byte> unchanged = new NativeArray<byte>(iStars.Count, Allocator.TempJob);

        for (int i = 0; i < iStars.Count; i++)
        {
            starPositions[i] = iStars[i].initpos;
        }

        var job = new StarSphereJob()
        {
            globeRadius = globeRadius,
            center = transform.position + new Vector3(globeRadius, -500, globeRadius),
            radiusSqr = globeRadius * globeRadius,
            starPositions = starPositions,
            unchanged = unchanged
        };

        // Schedule across all stars
        JobHandle handle = job.Schedule(starPositions.Length, 64);
        // 64 = batch size per job thread, tweak for performance
        handle.Complete();



        for (int i = 0; i < iStars.Count; i++)
        {
            if (unchanged[i] == 0)
            {
                Vector3 newPos = starPositions[i];
                Transform t = iStars[i].transform;

                if ((t.position - newPos).sqrMagnitude > 0.01f)
                {
                    t.position = newPos;
                    if (starPositions[i].y > 0) 
                        iStars[i].transform.LookAt(playerCamera);
                }
            }
        }

        starPositions.Dispose();
    }

    // Burst-compiled job
    [BurstCompile]
    struct StarSphereJob : IJobParallelFor
    {
        public float globeRadius;
        public Vector3 center;
        public float radiusSqr;

        public NativeArray<Vector3> starPositions;
        public NativeArray<byte> unchanged;

        public void Execute(int index)
        {
            Vector3 starPos = starPositions[index];

            float dx = starPositions[index].x - center.x;
            float dy = starPositions[index].y - center.y;
            float dz = starPositions[index].z - center.z;

            float distSqr = dx * dx + dy * dy + dz * dz;
            if (distSqr > radiusSqr)
            {
                unchanged[index] = 1;
                return;
            }

            float dist = Mathf.Sqrt(distSqr);
            if (dist < 0.0001f) dist = 0.0001f;

            float invDist = 1f / dist;
            float nx = dx * invDist;
            float ny = dy * invDist;
            float nz = dz * invDist;

            starPositions[index] = new Vector3(
                center.x + nx * globeRadius,
                center.y + ny * globeRadius,
                center.z + nz * globeRadius
            );
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

            vertex.y = positionType.GetY(globeRadius, vertex.x - globeRadius, vertex.z - globeRadius);
            vertices[i] = vertex;
        }
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

        if (operationDivisions < 1)
        {
            operationDivisions = 1;
        }

        if (Application.isPlaying)
        {
            InitializeMiniStars(operationDivisions);
        }

        if (GetComponent<MeshRenderer>() == null || GetComponent<MeshRenderer>().enabled == false)
        {
            return;
        }


        if (positionType == null)
        {
            positionType = new XZPosition();
        }

        myMesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = myMesh;

        DrawSphere();
    }
}
