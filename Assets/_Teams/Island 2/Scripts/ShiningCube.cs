using UnityEngine;

public class ShiningCube : MonoBehaviour
{
    private const float DefaultEmissionIntensity = 5f;
    private const float DefaultLightIntensity = 3f;
    private const float DefaultLightRange = 80f;

    [SerializeField] private float emissionIntensity = DefaultEmissionIntensity;
    [SerializeField] private bool addLightComponent = true;

    private void Start()
    {
        SetupCube();
    }

    private void SetupCube()
    {
        SetupMaterial();

        if (addLightComponent)
        {
            SetupLight();
        }
    }

    private void SetupMaterial()
    {
        Renderer renderer = GetComponent<Renderer>();

        if (renderer == null)
        {
            renderer = gameObject.AddComponent<MeshRenderer>();
        }

        Material material = renderer.material;
        material.color = Color.white;
        material.SetColor("_EmissionColor", Color.white * emissionIntensity);
        material.EnableKeyword("_EMISSION");
        material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
    }

    private void SetupLight()
    {
        Light light = GetComponent<Light>();

        if (light == null)
        {
            light = gameObject.AddComponent<Light>();
        }

        light.type = LightType.Point;
        light.intensity = DefaultLightIntensity;
        light.range = DefaultLightRange;
        light.color = Color.white;
        light.enabled = true;
    }
}
