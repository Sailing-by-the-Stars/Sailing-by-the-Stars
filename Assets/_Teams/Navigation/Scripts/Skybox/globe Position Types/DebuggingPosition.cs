using UnityEngine;
using static StarDataLoader;

[ExecuteAlways]
[CreateAssetMenu(fileName = "Debugging Position", menuName = "Globe Position Type/Debugging Position")]
public class DebuggingPosition : GlobePositionType
{
    public override float GetY(float radius, float x, float z)
    {
        float y = 0;
        y = Mathf.Sqrt(Mathf.Pow(radius, 2) - (Mathf.Pow(x, 2) + Mathf.Pow(z, 2)));

        if (y.ToString() == "NaN")
        {
            y = 0;
        }

        return y;
    }

    public override Vector3 GetY(float globeRadius, float x, float z, Vector3 offsetPosition)
    {
        // Star position in XZ plane
        float starX = x;
        float starZ = z;
        float starY = 0f;

        // Direction from sphere center to star
        float dx = starX - center.x;
        float dy = starY - center.y;
        float dz = starZ - center.z;

        // Squared distance for early exit
        float distSqr = dx * dx + dy * dy + dz * dz;

        if (distSqr > radiusSqr)
        {
            // Outside sphere radius, no distortion
            return Vector3.down;
        }

        // Normalize direction
        float dist = Mathf.Sqrt(distSqr);
        if (dist < 0.0001f) dist = 0.0001f; // avoid divide by zero

        float invDist = 1f / dist;
        float nx = dx * invDist;
        float ny = dy * invDist;
        float nz = dz * invDist;

        // Snap directly to sphere surface
        float sphereX = center.x + nx * globeRadius;
        float sphereY = center.y + ny * globeRadius;
        float sphereZ = center.z + nz * globeRadius;

        return new Vector3(sphereX, sphereY, sphereZ);
    }
}
