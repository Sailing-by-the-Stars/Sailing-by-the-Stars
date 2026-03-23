using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(Volume))]
/// <summary>
/// Helper class that assigns fog settings values to the attached local volume.
/// </summary>
public class LocalFog : MonoBehaviour
{ 
    [Tooltip("Fog settings to apply to this local volume.")]
    [SerializeField] private FogSettings fogSettings;
    private void Awake()
    {
        if (fogSettings == null)
        {
            Debug.Log(gameObject.name + ": No FogSettings assigned to LocalFog volume.");
            return;
        }

        Volume volume = GetComponent<Volume>();
        if (!volume.profile.TryGet<Fog>(out Fog fog))
        {
            return;
        }

        fog.enabled.Override(true);
        fog.meanFreePath.Override(fogSettings.meanFreePath);
        fog.baseHeight.Override(fogSettings.baseHeight);
        fog.maximumHeight.Override(fogSettings.maximumHeight);
        fog.albedo.Override(fogSettings.albedo);
        fog.anisotropy.Override(fogSettings.anisotropy);
    }
}