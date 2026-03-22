using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[CreateAssetMenu(fileName = "FogSettings", menuName = "World Environment Effects/Fog Settings")]
public class FogSettings : ScriptableObject
{
    [Header("Fog Attributes")]
    [Tooltip("Lower = thicker fog")]
    public float meanFreePath = 25f;
    public float maximumHeight = 100f;
    public float baseHeight = 0f;
    public Color albedo = new Color(0.78f, 0.82f, 0.86f);
    [Tooltip("0 = uniform scatter, 1 = forward scatter")]
    [Range(0,1)] public float anisotropy = 0.6f;

    
}