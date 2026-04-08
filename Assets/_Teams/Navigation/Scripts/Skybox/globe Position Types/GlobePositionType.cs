using UnityEngine;

public abstract class GlobePositionType : ScriptableObject
{
    public float radiusSqr; // squared radius for early exit
    public float invRadius;          // used for t calculation
    public Vector3 center;

    public abstract float GetY(float radius, float x, float z);
    public abstract Vector3 GetY(float radius, float x, float z, Vector3 offsetPosition);
}
