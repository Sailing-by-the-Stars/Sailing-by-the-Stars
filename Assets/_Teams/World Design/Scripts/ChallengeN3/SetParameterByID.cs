using UnityEngine;
using FMODUnity;
using FMOD.Studio;

/// <summary>
/// Code by Alonso
/// </summary>

public class SetParameterByID : MonoBehaviour
{
    private EventInstance instance;
    private PARAMETER_ID windParameter;

    [SerializeField]
    private EventReference fmodEvent;

    [Header("Wind range")]
    [Range(-12f, 12f)]
    [HideInInspector] public float wind;
    void Start()
    {
        instance = FMODUnity.RuntimeManager.CreateInstance(fmodEvent);

        FMOD.Studio.EventDescription windEventDescription;
        instance.getDescription(out windEventDescription);
        FMOD.Studio.PARAMETER_DESCRIPTION windParameterDescription;
        windEventDescription.getParameterDescriptionByName("WeatherIntensity", out windParameterDescription);
        windParameter = windParameterDescription.id;

        instance.start();
    }

    void Update()
    {
        instance.setParameterByID(windParameter, wind);
    }
}
