using UnityEngine;

[ExecuteAlways]
[CreateAssetMenu(fileName = "Declination Position", menuName = "Globe Position Type/Declination Position")]
public class DeclinationPosition : GlobePositionType
{
    [SerializeField]
    Vector3 xdiv;
    [SerializeField]
    Vector3 xdir;
    [SerializeField]
    Vector3 xout;


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

        xDiv /= radius;
        zDiv /= radius;

        float distance = Mathf.Sqrt(xDiv * xDiv + zDiv * zDiv);


        if(distance > 1)
        {
            return new Vector3(x, offsetPosition.y, z);
        }

        // Avoid division by zero when normalizing
        float xDir = 0f;
        float zDir = 0f;

        if (distance > 0.0001f)
        {
            xDir = xDiv / distance;
            zDir = zDiv / distance;
        }

        float dec = ((1 - distance) * Mathf.PI - Mathf.PI / 2f);

        // Spherical projection
        float y = Mathf.Sin(dec);
        float xOut = xDir * Mathf.Cos(dec);
        float zOut = zDir * Mathf.Cos(dec);

        // Scale to sphere radius
        xOut *= radius;
        y *= radius;
        zOut *= radius;

        xdiv = new Vector3(xDiv, 0, zDiv);

        xdir = new Vector3(xDir, dec, zDir);



        // Apply offset
        xOut += radius;
        y += offsetPosition.y;
        zOut += radius;

        xout = new Vector3(xOut, y, zOut);

        return new Vector3(xOut, y, zOut);
    }
}
