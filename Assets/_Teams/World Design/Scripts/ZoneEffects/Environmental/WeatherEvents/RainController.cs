/*
 * Created by Christina Pence
 * Contributed to by:
 */
using _Teams.World_Design.Scripts.ZoneEffects.Environmental.WeatherEvents;
using UnityEngine;

/// <summary>
/// Drives rain particle system intensity from WeatherManager.
/// Motion is entirely velocity-over-lifetime driven (Start Speed is 0).
/// Emitter is rotated -90 degrees on X; local Z is world down (fall direction).
/// VoL space is Local — wind is transformed into local space before applying.
/// Rain intensity of 0 represents no rain.
/// </summary>
public class RainController : MonoBehaviour, IWeatherEventController
{
    [SerializeField] private ParticleSystem rainParticleSystem;
    [SerializeField] private float windInfluence = 2f;

    [Header("Location Settings")]
    [Tooltip("Tag of the GameObject the rain system should follow (e.g. MainCamera, Player). Leave empty to disable following.")]
    [SerializeField] private string followTargetTag = "Player";
    [Tooltip("How many units above target the particle system should be")]
    [SerializeField] private float emitterHeight = 5f;

    [Header("Rain Fall Speed (Velocity over Lifetime, local Z)")]
    [Tooltip("Fall speed at minimum rain intensity")]
    [SerializeField] private float minFallSpeed = 12f;
    [Tooltip("Fall speed at full rain intensity")]
    [SerializeField] private float maxFallSpeed = 16f;

    [Header("Emission")]
    [Tooltip("Particles per second at full rain intensity")]
    [SerializeField] private float maxRateOverTime = 800f;

    [Header("Visual Intensity Scaling")]
    [Tooltip("Stretched billboard length scale at minimum intensity")]
    [SerializeField] private float minLengthScale = 2f;
    [Tooltip("Stretched billboard length scale at full intensity")]
    [SerializeField] private float maxLengthScale = 6f;
    [Tooltip("Particle width (start size X) at minimum intensity")]
    [SerializeField] private float minStartSizeX = 0.035f;
    [Tooltip("Particle width (start size X) at full intensity")]
    [SerializeField] private float maxStartSizeX = 0.06f;
    [Tooltip("Particle length (start size Y) at minimum intensity")]
    [SerializeField] private float minStartSizeY = 0.15f;
    [Tooltip("Particle length (start size Y) at full intensity")]
    [SerializeField] private float maxStartSizeY = 0.35f;

    private ParticleSystemRenderer rainRenderer;
    private ParticleSystem.MainModule main;
    private ParticleSystem.EmissionModule emission;
    private ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime;

    private float minRateOverTime;
    private float currentIntensity;

    private Transform followTarget;
    private SetRainAndThunder audioController;

    private const float rainStopThreshold = 0.01f;

    private void Awake()
    {
        if (rainParticleSystem == null)
            rainParticleSystem = GetComponent<ParticleSystem>();

        if (rainParticleSystem == null)
        {
            Debug.LogError("RainController: No ParticleSystem found.");
            return;
        }

        main = rainParticleSystem.main;
        emission = rainParticleSystem.emission;
        rainRenderer = rainParticleSystem.GetComponent<ParticleSystemRenderer>();
        velocityOverLifetime = rainParticleSystem.velocityOverLifetime;

        velocityOverLifetime.enabled = true;
        // Keep Local space — emitter is rotated -90x, local Z is world down
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;

        minRateOverTime = emission.rateOverTime.constant;

        if (WeatherManager.Instance != null)
            WeatherManager.Instance.Register(this);
    }

    private void Start()
    {
        audioController = FindFirstObjectByType<SetRainAndThunder>();
    }

    private void LateUpdate()
    {
        if (WeatherManager.Instance != null)
            ChangeDirection(WeatherManager.Instance.WindVelocity);

        UpdateFollowTarget();

        if (followTarget != null)
            transform.position = followTarget.position + Vector3.up * emitterHeight;
    }

    /// <summary>
    /// Receives 0-1 intensity already blended by WeatherManager.
    /// Drives emission rate, fall speed, and visuals — wind is applied separately in LateUpdate.
    /// </summary>
    public void ChangeWeatherEventValues(WeatherValues values)
    {
        if (rainParticleSystem == null) return;

        currentIntensity = values.rainIntensity;

        audioController?.SetRainAudioImmediate(currentIntensity);

        if (currentIntensity < rainStopThreshold)
        {
            if (rainParticleSystem.isPlaying)
                rainParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            return;
        }

        if (!rainParticleSystem.isPlaying)
            rainParticleSystem.Play();

        emission.rateOverTime = Mathf.Lerp(minRateOverTime, maxRateOverTime, currentIntensity);

        float fallSpeed = Mathf.Lerp(minFallSpeed, maxFallSpeed, currentIntensity);
        velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-fallSpeed);

        rainRenderer.lengthScale = Mathf.Lerp(minLengthScale, maxLengthScale, currentIntensity);
        main.startSizeX = Mathf.Lerp(minStartSizeX, maxStartSizeX, currentIntensity);
        main.startSizeY = Mathf.Lerp(minStartSizeY, maxStartSizeY, currentIntensity);
    }

    /// <summary>
    /// Transforms world-space wind into emitter local space and applies to VoL X and Y.
    /// Local Z (fall) is not touched.
    /// </summary>
    public void ChangeDirection(Vector3 worldWind)
    {
        if (rainParticleSystem == null) return;

        Vector3 localWind = transform.InverseTransformDirection(worldWind);

        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(localWind.x * windInfluence);
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(localWind.y * windInfluence);
    }

    private void UpdateFollowTarget()
    {
        if (string.IsNullOrEmpty(followTargetTag)) return;
        if (followTarget != null) return; // cache once, don't search every frame

        GameObject found = GameObject.FindWithTag(followTargetTag);
        if (found != null)
        {
            followTarget = found.transform;
        }
        else
        {
            Debug.LogWarning($"RainController: No GameObject found with tag '{followTargetTag}'. Following disabled.");
        }
    }

    public void SetRandomEventsActive(bool isActive) { } // not applicable for rain
}