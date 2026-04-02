using System.Collections.Generic;
using UnityEngine;

public class FireflyPath : MonoBehaviour
{
    [SerializeField] private List<Transform> points;
    [SerializeField] private int samplesPerSegment = 10;

    private readonly List<Vector3> sampledPoints = new();
    private readonly List<float> sampledDistances = new();
    private Vector3[] lastPointPositions;

    [SerializeField] private float samplePointSize = 0.05f;
    [SerializeField] private bool showSamples = true;

    private float totalLength;

    void Awake()
    {
        BuildSamples();
    }

    void BuildSamples()
    {
        sampledPoints.Clear();
        sampledDistances.Clear();

        float total = 0f;

        for (int i = 0; i < points.Count - 1; i++)
        {
            for (int j = 0; j <= samplesPerSegment; j++)
            {
                float t = j / (float)samplesPerSegment;

                Vector3 p0 = points[Mathf.Max(i - 1, 0)].position;
                Vector3 p1 = points[i].position;
                Vector3 p2 = points[i + 1].position;
                Vector3 p3 = points[Mathf.Min(i + 2, points.Count - 1)].position;

                Vector3 pos = CatmullRom(p0, p1, p2, p3, t);

                if (sampledPoints.Count > 0)
                {
                    total += Vector3.Distance(sampledPoints[^1], pos);
                }

                sampledPoints.Add(pos);
                sampledDistances.Add(total);
            }
        }

        totalLength = total;
    }

    public Vector3 GetPositionAtDistance(float distance)
    {
        distance = Mathf.Clamp(distance, 0, totalLength);

        for (int i = 0; i < sampledDistances.Count - 1; i++)
        {
            if (sampledDistances[i + 1] >= distance)
            {
                float t = Mathf.InverseLerp(
                    sampledDistances[i],
                    sampledDistances[i + 1],
                    distance
                );

                return Vector3.Lerp(sampledPoints[i], sampledPoints[i + 1], t);
            }
        }

        return sampledPoints[^1];
    }

    public float GetClosestDistance(Vector3 position)
    {
        float bestSqrDist = float.MaxValue;
        float closestDistance = 0f;

        for (int i = 0; i < sampledPoints.Count - 1; i++)
        {
            Vector3 a = sampledPoints[i];
            Vector3 b = sampledPoints[i + 1];

            Vector3 ab = b - a;
            float t = Vector3.Dot(position - a, ab) / ab.sqrMagnitude;
            t = Mathf.Clamp01(t);

            Vector3 closest = a + ab * t;
            float sqrDist = (position - closest).sqrMagnitude;

            if (sqrDist < bestSqrDist)
            {
                bestSqrDist = sqrDist;

                float segmentDistance = Mathf.Lerp(
                    sampledDistances[i],
                    sampledDistances[i + 1],
                    t
                );

                closestDistance = segmentDistance;
            }
        }

        return closestDistance;
    }

    Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            t * t * (2f * p0 - 5f * p1 + 4f * p2 - p3) +
            t * t * t * (-p0 + 3f * p1 - 3f * p2 + p3)
        );
    }

    // Editor Only
    void OnValidate()
    {
        if (points == null || points.Count < 2) return;
        BuildSamples();
    }

    void OnDrawGizmos()
    {
        if (!showSamples || points == null || points.Count < 2) return;

        bool needsRebuild = false;

        // Initialize cache if needed
        if (lastPointPositions == null || lastPointPositions.Length != points.Count)
        {
            lastPointPositions = new Vector3[points.Count];
            needsRebuild = true;
        }

        // Check if any point moved
        for (int i = 0; i < points.Count; i++)
        {
            if (points[i] == null) continue;

            if (lastPointPositions[i] != points[i].position)
            {
                needsRebuild = true;
                lastPointPositions[i] = points[i].position;
            }
        }

        if (needsRebuild)
        {
            BuildSamples();
        }

        // Draw spline
        Gizmos.color = Color.yellow;
        for (int i = 0; i < sampledPoints.Count - 1; i++)
        {
            Gizmos.DrawLine(sampledPoints[i], sampledPoints[i + 1]);
        }

        // Draw sample points
        Gizmos.color = Color.red;
        foreach (var p in sampledPoints)
        {
            Gizmos.DrawSphere(p, samplePointSize);
        }
    }
}
