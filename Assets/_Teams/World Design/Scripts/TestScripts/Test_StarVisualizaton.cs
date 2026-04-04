using UnityEngine;

/// <summary>
/// This doesn't fully work for now. But still somewhat helpful to see while testing (I'll remove it later). 
/// </summary>
public class StarDebugVisualizer : MonoBehaviour
{
    [System.Serializable]
    public class StarEntry
    {
        public string label;
        public Transform star;
        [Tooltip("targetAngle value from TwinklingStar (enter manually)")]
        public float targetAngle;
        public Color color;
    }

    [Tooltip("Height of water surface.")]
    [SerializeField] private float waterHeight = 0.5f;
    [SerializeField] private int circleSegments = 64;
    [SerializeField] private StarEntry[] stars;

    // LineRenderers for in-game visibility
    private LineRenderer[] lineRenderers;

    private void Start()
    {
        lineRenderers = new LineRenderer[stars.Length];

        for (int i = 0; i < stars.Length; i++)
        {
            StarEntry entry = stars[i];
            if (entry.star == null)
            {
                continue;
            }

            float radius = GetRadius(entry.star, entry.targetAngle);
            if (radius <= 0f)
            {
                continue;
            }

            GameObject obj = new GameObject($"StarCircle_{entry.label}");
            obj.transform.SetParent(transform);

            LineRenderer lr = obj.AddComponent<LineRenderer>();
            lr.loop = true;
            lr.positionCount = circleSegments;
            lr.startWidth = 0.3f;
            lr.endWidth = 0.3f;
            lr.useWorldSpace = true;
            lr.startColor = entry.color;
            lr.endColor = entry.color;

            Vector3 center = new Vector3(entry.star.position.x, waterHeight, entry.star.position.z);

            for (int j = 0; j < circleSegments; j++)
            {
                float angle = (float)j / circleSegments * 2f * Mathf.PI;
                lr.SetPosition(j, new Vector3(center.x + Mathf.Cos(angle) * radius,
                                              waterHeight,
                                              center.z + Mathf.Sin(angle) * radius));
            }

            lineRenderers[i] = lr;
        }
    }

    private void OnDrawGizmos()
    {
        if (stars == null)
        {
            return;
        }

        foreach (StarEntry entry in stars)
        {
            if (entry.star == null)
            {
                continue;
            }

            float radius = GetRadius(entry.star, entry.targetAngle);
            if (radius <= 0f)
            {
                continue;
            }

            Vector3 center = new Vector3(entry.star.position.x, waterHeight, entry.star.position.z);

            Gizmos.color = entry.color;

            for (int i = 0; i < circleSegments; i++)
            {
                float angle = (float)i / circleSegments * 2f * Mathf.PI;
                float nextAngle = (float)(i + 1) / circleSegments * 2f * Mathf.PI;
                Vector3 a = new Vector3(center.x + Mathf.Cos(angle) * radius,
                                        waterHeight,
                                        center.z + Mathf.Sin(angle) * radius);
                Vector3 b = new Vector3(center.x + Mathf.Cos(nextAngle) * radius,
                                        waterHeight,
                                        center.z + Mathf.Sin(nextAngle) * radius);
                Gizmos.DrawLine(a, b);
            }

            // vertical line from star down to water
            Gizmos.color = new Color(entry.color.r, entry.color.g, entry.color.b, 0.3f);
            Gizmos.DrawLine(entry.star.position, center);
            Gizmos.DrawWireSphere(entry.star.position, 1f);

#if UNITY_EDITOR
            UnityEditor.Handles.color = entry.color;
            UnityEditor.Handles.Label(center + Vector3.up * 2f,
                                      $"{entry.label}\nAngle: {entry.targetAngle}\nRadius: {radius:F1}");
#endif
        }
    }

    private float GetRadius(Transform star, float targetAngle)
    {
        if (targetAngle <= 0f || targetAngle >= 90f)
        {
            return 0f;
        }

        float starHeight = star.position.y - waterHeight;
        if (starHeight <= 0f)
        {
            return 0f;
        }

        return starHeight / Mathf.Tan(targetAngle * Mathf.Deg2Rad);
    }
}