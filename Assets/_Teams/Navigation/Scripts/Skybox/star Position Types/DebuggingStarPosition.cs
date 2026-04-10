using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[CreateAssetMenu(fileName = "Debugging Star Position", menuName = "Star Position Type/Debugging Star Position")]
public class DebuggingStarPosition : StarPositionType
{
    [SerializeField]
    float colorBoost = 1.2f;

    //[SerializeField]
    //List<bool> hasColorBeenUsed = new();

    private void Awake()
    {
        /*hasColorBeenUsed.Clear();
        hasColorBeenUsed.Capacity = 8;*/
    }
    

    public override Vector3 GetBasePosition(double ra, double dec, out bool flag)
    {
        // Place stars on a cylinder using 2D trigonometry.
        double x = System.Math.Cos(ra);
        double y = 0;
        double z = System.Math.Sin(ra);

        double distance = 1 - ((dec + (Mathf.PI / 2)) / Mathf.PI);
        //d = 

        // Map stars onto a plane
        // Work out the distance from the north pole and use this to scale
        x *= distance;
        z *= distance;

        flag = false;
        // Return as float
        return new((float)x, (float)y, (float)z);
    }

    public override Color SetColour(byte spectral_type, byte spectral_index, out bool flag)
    {
        flag = false;

        float temperature = GetTemperature(spectral_type, spectral_index);

        Color color = BlackbodyToRGB(temperature);

        //color = Color.Lerp(new Color(color.grayscale, color.grayscale, color.grayscale), color, colorBoost);
        //color *= colorBoost;
        color = BoostChroma(color, colorBoost);

        return color;
    }

    Color BoostChroma(Color color, float amount)
    {
        float gray = color.grayscale;
        Color grayColor = new Color(gray, gray, gray);

        return grayColor + (color - grayColor) * amount;
    }


    float GetTemperature(byte spectral_type, byte spectral_index)
    {
        // Base temperatures (in Kelvin)
        float t0 = 0f, t1 = 0f;

        switch (spectral_type)
        {
            case (byte)'O': t0 = 40000f; t1 = 30000f; break;
            case (byte)'B': t0 = 30000f; t1 = 10000f; break;
            case (byte)'A': t0 = 10000f; t1 = 7500f; break;
            case (byte)'F': t0 = 7500f; t1 = 6000f; break;
            case (byte)'G': t0 = 6000f; t1 = 5200f; break;
            case (byte)'K': t0 = 5200f; t1 = 3700f; break;
            case (byte)'M': t0 = 3700f; t1 = 2400f; break;
            default: return 6500f; // fallback = white-ish
        }

        float percent = (spectral_index - 0x30) / 10.0f;
        return Mathf.Lerp(t0, t1, percent);
    }

    Color BlackbodyToRGB(float temperature)
    {
        float t = temperature / 100f;

        float r, g, b;

        // Red
        if (t <= 66f)
            r = 1f;
        else
            r = Mathf.Clamp01(1.292936186062745f * Mathf.Pow(t - 60f, -0.1332047592f));

        // Green
        if (t <= 66f)
            g = Mathf.Clamp01(0.3900815787690196f * Mathf.Log(t) - 0.6318414437886275f);
        else
            g = Mathf.Clamp01(1.129890860895294f * Mathf.Pow(t - 60f, -0.0755148492f));

        // Blue
        if (t >= 66f)
            b = 1f;
        else if (t <= 19f)
            b = 0f;
        else
            b = Mathf.Clamp01(0.5432067891101961f * Mathf.Log(t - 10f) - 1.19625408914f);

        return new Color(r, g, b);
    }

    public override float SetSize(short magnitude, out bool flag)
    {
        flag = false;

        // Linear isn't factually accurate, but the effect is sufficient.
        return 1 - Mathf.InverseLerp(-146, 1000, magnitude);
    }
}

