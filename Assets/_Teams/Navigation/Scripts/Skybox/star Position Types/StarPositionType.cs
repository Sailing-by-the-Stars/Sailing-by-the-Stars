using UnityEngine;

public abstract class StarPositionType : ScriptableObject
{
    public abstract Vector3 GetBasePosition(double ra, double dec, out bool flag);

    public abstract Color SetColour(byte spectral_type, byte spectral_index, out bool flag);

    public abstract float SetSize(short magnitude, out bool flag);
}
