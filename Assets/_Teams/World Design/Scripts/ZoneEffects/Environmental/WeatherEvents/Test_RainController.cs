/*
 * Created by Christina Pence
 * Contributed to by:
 */
using UnityEngine;
using UnityEngine.UIElements;
/// <summary>
/// Drives rain particle system intensity from WeatherManager.
/// Reads baseline values from the particle system on Awake (used as lightest rain)
/// Rain intensity of 0 represents no rain
/// </summary>
public class TestRainController : MonoBehaviour
{
    [SerializeField] private ParticleSystem rainParticleSystem;
    [SerializeField] private float windInfluence = 0.3f;
    [Header("Location Settings")]
    [Tooltip("Target for the rain system to follow for positioning")]
    [SerializeField] private Transform followTarget;
    [Tooltip("How many units above target the particle system should be")]
    [SerializeField] private float emitterHeight = 5f;

    [Header("Max values at full intensity")]
    [Tooltip("Particles per second at full rain intensity.")]
    [SerializeField] private float maxRateOverTime = 500f;

    [Tooltip("Particle start speed at full rain intensity.")]
    [SerializeField] private float maxStartSpeed = 15f;

    [Tooltip("Particle start size at full rain intensity.")]
    [SerializeField] private float maxStartSize = 0.15f;
    [Tooltip("Increase speed scale for more streaky effect at max intensity")]
    [SerializeField] private float maxSpeedScale = 0.05f;

    private ParticleSystemRenderer rainRenderer;
    private ParticleSystem.MainModule main;
    private ParticleSystem.EmissionModule emission;
    private ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime;
    // Baseline values read from particle system on Awake
    private float minRateOverTime;
    private float minStartSpeed;
    private float minStartSize;
    private float minSpeedScale;

    private const float rainStopThreshold = 0.01f; // rain stops when intensity is below this value

    private void Start()
    {
        if (rainParticleSystem == null)
        {
            Debug.LogError("No ParticleSystem assigned to rain controller.");
            return;
        }

        // cache references
        main = rainParticleSystem.main;
        emission = rainParticleSystem.emission;
        rainRenderer = rainParticleSystem.GetComponent<ParticleSystemRenderer>();
        velocityOverLifetime = rainParticleSystem.velocityOverLifetime;

        velocityOverLifetime.enabled = true;
        // Read baseline values from particle system
        minRateOverTime = rainParticleSystem.emission.rateOverTime.constant;
        minSpeedScale = rainRenderer.velocityScale;
        minStartSpeed = main.startSpeed.constant;
        minStartSize = main.startSize.constant;

        WeatherManager.Instance.Register(this);
    }
    private void LateUpdate()
    {
        if (followTarget != null)
            transform.position = followTarget.position + Vector3.up * emitterHeight;

        Vector3 wind = WeatherManager.Instance.windVelocity;
        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(wind.x * windInfluence);
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(0f);
        velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(wind.z * windInfluence);
    }
    /// <summary>
    /// Receives 0-1 value already blended by WeatherManager and applies rain intensity to particle system properties.
    /// </summary>
    public void SetRainIntensity(float intensity)
    {
        if (rainParticleSystem == null)
        {
            return;
        }
        // Debug.Log($"Rain controller SetRainIntensity called: {intensity:F2}");
        // turn rain off below threshold
        if (intensity < rainStopThreshold)
        {
            rainParticleSystem.Stop();
            return;
        }
        if (!rainParticleSystem.isPlaying)
        {
            rainParticleSystem.Play();
        }

        // Remap 0-1 intensity to particle system property ranges
        emission.rateOverTime = Mathf.Lerp(minRateOverTime, maxRateOverTime, intensity);
        main.startSpeed = Mathf.Lerp(minStartSpeed, maxStartSpeed, intensity);
        main.startSize = Mathf.Lerp(minStartSize, maxStartSize, intensity);
        rainRenderer.velocityScale = Mathf.Lerp(minSpeedScale, maxSpeedScale, intensity);
    }
}
