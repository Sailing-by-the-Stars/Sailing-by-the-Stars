using UnityEngine;

[ExecuteAlways]
[CreateAssetMenu(fileName = "XZ Position", menuName = "Globe Position Type/XZ Position")]
public class XZPosition : GlobePositionType
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

    public override Vector3 GetY(float radius, float x, float z, Vector3 offsetPosition)
    {
        float xDiv = x;
        float zDiv = z;
        xDiv -= radius + offsetPosition.x;
        zDiv -= radius + offsetPosition.z;

        float y = 0;
        y = Mathf.Sqrt(Mathf.Pow(radius, 2) - (Mathf.Pow(xDiv, 2) + Mathf.Pow(zDiv, 2)));

        if (float.IsNaN(y))
        {
            y = 0;
        }

        y += offsetPosition.y;

        return new Vector3(x, y, z);
    }
}
