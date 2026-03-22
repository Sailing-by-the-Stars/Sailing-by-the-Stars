using System;
using _Teams.World_Design.Scripts.ZoneEffects.Environmental.WeatherEvents;
using UnityEngine;
using UnityEngine.Serialization;

public class WindController : MonoBehaviour, IWeatherEventController
{
    [Header("Direction")]
    [SerializeField] private bool horizontalOnly = true;
    [SerializeField] private bool randomizeOnStart = true;
    [SerializeField] private Vector3 windDirectionOnStart = Vector3.forward;

    [Header("Auto Reroll")]
    [SerializeField] private float rerollIntervalSeconds = 10f;
    [SerializeField] private bool randomEventsActive = true;
    [SerializeField] private float autoRerollWindIntensity = 1f;

    [Header("Wind Object")]
    [SerializeField] private WindObject windObject;

    private float rerollTimer;

    private void Awake()
    {
        ResolveWindObjectReference();
        SetWindDirectionForObject(randomizeOnStart ? GenerateRandomDirection() : windDirectionOnStart);
    }

    private void Update()
    {
        if (!randomEventsActive )
        {
            return;
        }
        
        rerollTimer += Time.deltaTime;
        if (rerollTimer < rerollIntervalSeconds)
        {
            return;
        }

        rerollTimer = 0f;
        HandleWindReroll();
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void HandleWindReroll()
    {
        Vector3 newWindDirection = GenerateRandomDirection() * autoRerollWindIntensity;
        ChangeDirection(newWindDirection);
        Debug.Log("Wind Reroll. New Direction: " + newWindDirection);
    }


    private void SetWindDirectionForObject(Vector3 newWindDirection)
    {
        if (!windObject)
        {
            return;
        }

        windObject.SetWindDirection(newWindDirection);
    }

    private void ResolveWindObjectReference()
    {
        if (windObject != null)
        {
            return;
        }
        windObject = FindFirstObjectByType<WindObject>();
    }

    private Vector3 GenerateRandomDirection()
    {
        for (int i = 0; i < 8; i++)
        {
            Vector3 random = horizontalOnly
                ? new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f))
                : UnityEngine.Random.insideUnitSphere;

            if (random.sqrMagnitude > 0.0001f)
            {
                return random.normalized;
            }
        }

        return Vector3.forward;
    }

    public void ChangeWeatherEventValues(WeatherValues weatherValues)
    {
        // Debug.Log("Random events: " + weatherValues.windRandomEventsActive);
        float radians = weatherValues.windDirectionDegrees * Mathf.Deg2Rad;
        Vector3 currentWindDirection = new Vector3(Mathf.Sin(radians), 0f, Mathf.Cos(radians));
        
        Debug.Log(weatherValues.windRandomEventsActive);
        SetRandomEventsActive(weatherValues.windRandomEventsActive);
        ChangeDirection(currentWindDirection * weatherValues.windSpeed);
        // Debug.Log("LogWind Reroll. New Direction: " + currentWindDirection * weatherValues.windSpeed);
        ChangeAutoRerollWindIntensity(weatherValues.windAutoRerollIntensity);
        
        // Debug.Log("Wind Direction Changed: " + currentWindDirection);   
    }

    public void ChangeDirection(Vector3 direction)
    {
        if (!windObject)
        {
            return;
        }

        windObject.SetWindDirection(direction);
    }

    public void SetRandomEventsActive(bool isActive)
    {
        randomEventsActive = isActive;
    }
    
    public void ChangeAutoRerollWindIntensity(float intensity)
    {
        autoRerollWindIntensity = intensity;
    }
}
