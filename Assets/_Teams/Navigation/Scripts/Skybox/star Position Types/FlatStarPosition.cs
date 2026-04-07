using UnityEngine;

[CreateAssetMenu(fileName = "Flat Star Position", menuName = "Star Position Type/Flat Star Position")]
public class FlatStarPosition : StarPositionType
{
    public override Vector3 GetBasePosition(double ra, double dec, out bool flag)
    {
        // Place stars on a cylinder using 2D trigonometry.
        double x = System.Math.Cos(ra);
        double y = 0;
        double z = System.Math.Sin(ra);

        double distance = 1 - ((dec + (Mathf.PI / 2)) / Mathf.PI);

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
        Color IntColour(int r, int g, int b)
        {
            return new Color(r / 255f, g / 255f, b / 255f);
        }
        // OBAFGKM colours from: https://arxiv.org/pdf/2101.06254.pdf
        Color[] col = new Color[8];
        col[0] = IntColour(0x5c, 0x7c, 0xff); // O1
        col[1] = IntColour(0x5d, 0x7e, 0xff); // B0.5
        col[2] = IntColour(0x79, 0x96, 0xff); // A0
        col[3] = IntColour(0xb8, 0xc5, 0xff); // F0
        col[4] = IntColour(0xff, 0xef, 0xed); // G1
        col[5] = IntColour(0xff, 0xde, 0xc0); // K0
        col[6] = IntColour(0xff, 0xa2, 0x5a); // M0
        col[7] = IntColour(0xff, 0x7d, 0x24); // M9.5

        int col_idx = -1;
        if (spectral_type == 'O')
        {
            col_idx = 0;
        }
        else if (spectral_type == 'B')
        {
            col_idx = 1;
        }
        else if (spectral_type == 'A')
        {
            col_idx = 2;
        }
        else if (spectral_type == 'F')
        {
            col_idx = 3;
        }
        else if (spectral_type == 'G')
        {
            col_idx = 4;
        }
        else if (spectral_type == 'K')
        {
            col_idx = 5;
        }
        else if (spectral_type == 'M')
        {
            col_idx = 6;
        }

        flag = false;
        // If unknown, make white.
        if (col_idx == -1)
        {
            return Color.white;
        }

        // Map second part 0 -> 0, 10 -> 100
        float percent = (spectral_index - 0x30) / 10.0f;
        return Color.Lerp(col[col_idx], col[col_idx + 1], percent);
    }

    public override float SetSize(short magnitude, out bool flag)
    {
        flag = false;
        // Linear isn't factually accurate, but the effect is sufficient.
        return 1 - Mathf.InverseLerp(-146, 796, magnitude);
    }
}
