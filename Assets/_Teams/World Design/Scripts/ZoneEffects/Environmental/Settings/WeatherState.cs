/*
 * Created by Christina Pence
 * Contributed to by:
 */

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Use this class to add weather specific parameters.
/// </summary>
[System.Serializable]
public struct WeatherValues
{
    [Header("Wind")]
    [Tooltip("Speed in m/s")]
    public float windSpeed;

    [Tooltip("0 = north, 90 = east, 180 = south, 270 = west")]
    [Range(0f, 360f)] public float windDirectionDegrees;

    [Tooltip("If true, wind direction will randomly change at intervals.")]
    public bool windRandomEventsActive;

    [FormerlySerializedAs("autoRerollWindIntensity")]
    [Tooltip("Intensity of random wind direction changes in m/s.")]
    public float windAutoRerollIntensity;

    [Header("Rain")]
    [Tooltip("0 = no rain. 1 = heaviest rain")]
    [Range(0f, 1f)] public float rainIntensity;

    [Header("Thunder")]
    [Tooltip("Thunder and lightning: 0 = no thunder")]
    [Range(0f, 1f)] public float thunderIntensity;

    [Header("Ocean")]
    [Tooltip("0 = calmest 1 = highest waves (max driven by HDRP water surface values)")]
    public float waveIntensity;

    [Tooltip("Ocean current speed in m/s")]
    public float oceanCurrentSpeed;
}

/// <summary>
/// Defines a sustained weather state at full intensity used as the destination for weather transitions
/// </summary>
[CreateAssetMenu(fileName = "WeatherState", menuName = "World Environment Effects/Weather State")]
public class WeatherState : ScriptableObject
{
    [Tooltip("The target values at peak intensity for each weather parameter")]
    public WeatherValues values;
}