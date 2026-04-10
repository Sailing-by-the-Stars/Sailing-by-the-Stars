using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class StarClumper : MonoBehaviour
{
    [SerializeField]
    bool updateClumping = false;

    [SerializeField]
    int resolution = new();

    [SerializeField]
    Vector2 leastCorner;

    [SerializeField]
    Vector2 mostCorner;


    private void OnValidate()
    {
        if (updateClumping)
        {
            updateClumping = false;
            ClumpStars(resolution);
        }
    }


    void ClumpStars(int resolution)
    {
        leastCorner = new Vector2(transform.GetChild(0).position.x, transform.GetChild(0).position.z);
        mostCorner = leastCorner;

        List<Transform> children = new();
        foreach (Transform t in transform)
        {
            children.Add(t);
        }

        foreach (Transform t in children)
        {
            if (t.position.x > mostCorner.x)
            {
                mostCorner.x = t.position.x;
            }
            if (t.position.z > mostCorner.y)
            {
                mostCorner.y = t.position.z;
            }


            if(t.position.x < leastCorner.x)
            {
                leastCorner.x = t.position.x;
            }
            if(t.position.z < leastCorner.y)
            {
                leastCorner.y = t.position.z;
            }
        }


        Vector2 difference = new Vector2(mostCorner.x - leastCorner.x, mostCorner.y - leastCorner.y);

        Vector2 stepsize = new Vector2(difference.x / resolution, difference.y / resolution);


        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                List<Transform> clumpObjects = new();

                float xMin = leastCorner.x + i * stepsize.x;
                float xMax = leastCorner.x + (i + 1) * stepsize.x;

                float yMin = leastCorner.y + j * stepsize.y;
                float yMax = leastCorner.y + (j + 1) * stepsize.y;



                foreach (Transform t in children)
                {
                    bool inX = false;
                    bool inY = false;

                    if (t.position.x >= xMin && t.position.x <= xMax)
                    {
                        inX = true;
                    }

                    if (t.position.z >= yMin && t.position.z <= yMax)
                    {
                        inY = true;
                    }

                    if (inX && inY)
                    {
                        clumpObjects.Add(t);
                    }
                }

                if(clumpObjects.Count == 0)
                {
                    continue;
                }

                GameObject clumpParent = new GameObject($"star clump {(i * resolution) + j}", typeof(StarInfo));

                clumpParent.transform.parent = transform;

                clumpParent.transform.position = GetMeanVector(clumpObjects);

                clumpParent.transform.Rotate(90, 0, 0);



                foreach (Transform t in clumpObjects)
                {
                    t.parent = clumpParent.transform;
                }
            }
        }



    }

    private Vector3 GetMeanVector(List<Transform> transforms)
    {
        List<Vector3> positions = new();

        foreach (Transform t in transforms)
        {
            positions.Add(t.position);
        }

        return GetMeanVector(positions);
    }



    private Vector3 GetMeanVector(List<Vector3> positions)
    {
        if (positions.Count == 0)
            return Vector3.zero;
        float x = 0f;
        float y = 0f;
        float z = 0f;
        foreach (Vector3 pos in positions)
        {
            x += pos.x;
            y += pos.y;
            z += pos.z;
        }
        return new Vector3(x / positions.Count, y / positions.Count, z / positions.Count);
    }
}
