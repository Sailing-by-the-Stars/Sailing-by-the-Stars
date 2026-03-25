using _Teams.World_Design.Scripts.ZoneEffects.Environmental.WeatherEvents;
using UnityEngine;

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

    [Header("Wind Audio (m/s -> 0..1)")]
    [SerializeField, Min(0.01f)] private float minAudibleWindSpeedMps = 1f;
    [SerializeField, Range(0f, 1f)] private float minAudibleAudioValue = 0.12f;
    [SerializeField, Min(0.02f)] private float maxWindSpeedForFullAudioMps = 20f;
    [SerializeField, Min(0.1f)] private float audioResponseExponent = 0.8f;

    private SetWind windAudioController;
    
    private float rerollTimer;
    public Vector3 currentWindVelocity { get; private set; }

    private void Awake()
    {
        ResolveWindObjectReference();
        windAudioController = FindFirstObjectByType<SetWind>();
        SetWindDirectionForObject(randomizeOnStart ? GenerateRandomDirection() : windDirectionOnStart);
        if (WeatherManager.Instance != null)
        {
            WeatherManager.Instance.Register(this);
        }
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
        float radians = weatherValues.windDirectionDegrees * Mathf.Deg2Rad;
        Vector3 currentWindDirection = new Vector3(Mathf.Sin(radians), 0f, Mathf.Cos(radians));
        
        SetRandomEventsActive(weatherValues.windRandomEventsActive);
        // TODO: These don't need to be updated every frame. Decide if they will be updated in Manager for
        // each new state or use event system / logic internally
        ChangeDirection(currentWindDirection * weatherValues.windSpeed);

        if (windAudioController != null)
        {
            float normalizedWindAudio = NormalizeWindSpeedToAudio(weatherValues.windSpeed);
            windAudioController.SetWindF(normalizedWindAudio);
        }
        
        SetWindObjectIntensity(weatherValues.windSpeed);
        
        ChangeAutoRerollWindIntensity(weatherValues.windAutoRerollIntensity);
    }

    // Converts physical wind speed (m/s) to range [0..1].
    private float NormalizeWindSpeedToAudio(float windSpeedMps)
    {
        float speed = Mathf.Max(0f, windSpeedMps);

        if (speed <= 0f)
        {
            return 0f;
        }

        float fullAudioSpeed = Mathf.Max(minAudibleWindSpeedMps + 0.01f, maxWindSpeedForFullAudioMps);

        if (speed <= minAudibleWindSpeedMps)
        {
            float nearZeroT = speed / minAudibleWindSpeedMps;
            return Mathf.Lerp(0f, minAudibleAudioValue, nearZeroT);
        }

        float t = Mathf.InverseLerp(minAudibleWindSpeedMps, fullAudioSpeed, speed);
        t = Mathf.Pow(t, audioResponseExponent);

        return Mathf.Lerp(minAudibleAudioValue, 1f, t);
    }

    public void ChangeDirection(Vector3 direction)
    {
        if (!windObject)
        {
            return;
        }
        currentWindVelocity = direction;
        windObject.SetWindDirection(direction);

    }

    public void SetRandomEventsActive(bool isActive)
    {
        randomEventsActive = isActive;
    }

    private void ChangeAutoRerollWindIntensity(float intensity)
    {
        autoRerollWindIntensity = intensity;
    }
    
    private void SetWindObjectIntensity(float windSpeed)
    {
        if (!windObject)
        {
            return;
        }
        
        windObject.SetWindIntensity(windSpeed);
    }
}
