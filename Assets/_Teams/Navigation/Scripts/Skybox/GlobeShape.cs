using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class GlobeShape : MonoBehaviour
{
    [SerializeField] float globeRadius = 5;
    [SerializeField] int resolution = 10;
    [SerializeField] float offsetY = 5000f;
    [Tooltip("this determines over how manny frames the moving of the stars will be divided")]
    [SerializeField] int operationDivisions = 5;
    [SerializeField] int operationHeightCutoff = 10;
    [SerializeField] GlobePositionType positionType;

    List<Vector3> verticePositions = new();
    List<Vector3> vertices = new();
    List<int> triangles = new();
    Mesh myMesh;
    MeshFilter meshFilter;
    List<StarInfo> relatedMiniStars = new();
    List<List<StarInfo>> dividedMiniStars = new();
    int Iterator;

    private NativeArray<Vector3> initPositions;
    private NativeArray<Vector3> starPositions;
    private NativeArray<byte> changed;
    public int iterator
    {
        get => Iterator;
        set => Iterator = (value >= dividedMiniStars.Count) ? 0 : value;
    }


    Transform cam;

    void Awake()
    {
        cam = Camera.main?.transform;

        if (cam == null)
        {
            Debug.LogError("Main camera not found!");
        }
    }

    private void OnDestroy()
    {
        initPositions.Dispose();
        starPositions.Dispose();
        changed.Dispose();
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
        InitializeMiniStars();

        DrawSphere();
    }


    void InitializeMiniStars()
    {
        int divisions = operationDivisions;

        if (divisions < 1)
        {
            divisions = 1;
        }

        relatedMiniStars = GetComponentsInChildren<StarInfo>().ToList();
        dividedMiniStars.Clear();

        if (relatedMiniStars.Count <= divisions)
        {
            dividedMiniStars.Add(relatedMiniStars);
            divisions = 1;
            return;
        }

        int starsPerDiv = Mathf.CeilToInt((float)relatedMiniStars.Count / divisions);
        List<StarInfo> starsToDiv = new(relatedMiniStars);

        for (int i = 0; i < divisions && starsToDiv.Count > 0; i++)
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

        MoveStars(dividedMiniStars[iterator], cam);
        iterator++;
    }


    void MoveSphere()
    {
        Vector3 targetpos = cam.position;

        targetpos.x -= globeRadius;
        targetpos.z -= globeRadius;

        targetpos.y -= offsetY;

        transform.position = targetpos;
    }


    void EnsureCapacity(List<StarInfo> iStars, int count)
    {
        if (!initPositions.IsCreated || initPositions.Length != count)
        {
            if (initPositions.IsCreated)
                initPositions.Dispose();

            initPositions = new NativeArray<Vector3>(count, Allocator.Persistent);
            
            for (int i = 0; i < count; i++)
            {
                initPositions[i] = iStars[i].initpos;
            }
        }
        
        if (!starPositions.IsCreated || starPositions.Length != count)
        {
            if (starPositions.IsCreated)
                starPositions.Dispose();

            starPositions = new NativeArray<Vector3>(count, Allocator.Persistent);
        }

        if (!changed.IsCreated || changed.Length != count)
        {
            if (changed.IsCreated)
                changed.Dispose();

            changed = new NativeArray<byte>(count, Allocator.Persistent);
        }
    }

    public void MoveStars(List<StarInfo> iStars, Transform playerCamera)
    {
        int count = iStars.Count;

        // SETUP DATA
        EnsureCapacity(iStars, count);

        if (operationDivisions > 1)
        {
            for (int i = 0; i < count; i++)
            {
                initPositions[i] = iStars[i].initpos;
            }
        }

        Vector3 center = transform.position + new Vector3(globeRadius, -offsetY, globeRadius);
        float radiusSqr = globeRadius * globeRadius;

        
        // RUN JOBS
        var job = new StarSphereJob()
        {
            GlobeRadius = globeRadius,
            Center = center,
            RadiusSqr = radiusSqr,

            initialPoss = initPositions,
            starPoss = starPositions,
            changed = changed
        };

        JobHandle handle = job.Schedule(count, 64);
        handle.Complete();


        // APPLY RESULTS
        float starHeightCutoff = transform.position.y + offsetY + operationHeightCutoff;

        Vector3 StarPos;
        for (int i = 0; i < count; i++)
        {
            if (changed[i] == 1)
            {
                StarPos = starPositions[i];
                if (StarPos.y > starHeightCutoff)
                {
                    iStars[i].thisTransform.position = StarPos;
                }
            }
        }
    }

    // Burst-compiled job
    [BurstCompile(FloatMode = FloatMode.Default, FloatPrecision = FloatPrecision.Standard)]
    struct StarSphereJob : IJobParallelFor
    {
        public float GlobeRadius;
        public Vector3 Center;
        public float RadiusSqr;

        public NativeArray<Vector3> initialPoss;
        public NativeArray<Vector3> starPoss;
        public NativeArray<byte> changed;

        public void Execute(int index)
        {
            Vector3 starPos = initialPoss[index];

            float dx = starPos.x - Center.x;
            float dy = starPos.y - Center.y;
            float dz = starPos.z - Center.z;
            
            float distSqr = dx * dx + dy * dy + dz * dz;
            if (distSqr > RadiusSqr)
            {
                changed[index] = 0;
                return;
            }

            changed[index] = 1;

            float dist = Mathf.Sqrt(distSqr);
            if (dist < 0.0001f) dist = 0.0001f;

            float invDist = 1f / dist;
            float nx = dx * invDist;
            float ny = dy * invDist;
            float nz = dz * invDist;

            starPoss[index] = new Vector3(
                Center.x + nx * GlobeRadius,
                Center.y + ny * GlobeRadius,
                Center.z + nz * GlobeRadius
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
        if (cam == null)
        {
            cam = Camera.main?.transform;

            if (cam == null)
            {
                return;
            }
        }


        Vector3 targetpos = cam.position;

        targetpos.x -= globeRadius;
        targetpos.z -= globeRadius;

        targetpos.y -= offsetY;

        transform.position = targetpos;

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
