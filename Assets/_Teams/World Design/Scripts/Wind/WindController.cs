using System;
using UnityEngine;

public class WindController : MonoBehaviour
{
    [Header("Direction")]
    [SerializeField] private bool horizontalOnly = true;
    [SerializeField] private bool randomizeOnStart = true;
    [SerializeField] private Vector3 windDirectionOnStart = Vector3.forward;
    [SerializeField] private Vector3? influencedWindDirectionOnStart = null;

    [Header("Auto Reroll")]
    [SerializeField] private bool autoReroll;
    [SerializeField] private float rerollIntervalSeconds = 10f;

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


        rerollTimer += Time.deltaTime;
        if (rerollTimer < rerollIntervalSeconds)
        {
            return;
        }

        rerollTimer = 0f;
        HandleWindDirectionChange();
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void HandleWindDirectionChange()
    {
        Debug.Log("WindDirectionChange");
        Vector3 newWindDirection;
        if (autoReroll)
        {
            Debug.Log("Auto rerolling wind direction.");
            newWindDirection = GenerateRandomDirection();
        }
        else if (influencedWindDirectionOnStart.HasValue)
        {
            Debug.Log("Using influenced wind direction on start: " + influencedWindDirectionOnStart.Value);
            newWindDirection = influencedWindDirectionOnStart.Value;
        }
        else
        {
            Debug.LogWarning("No wind direction source available. Defaulting to forward.");
            newWindDirection = Vector3.forward;
        }

        SetWindDirectionForObject(newWindDirection);
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

    public void SetInfluencedWindDirection(Vector3 newWindDirection)
    {
        influencedWindDirectionOnStart = newWindDirection;
    }
    
    public void SetAutoReroll(bool enable)
    {
        autoReroll = enable;
        if (!autoReroll)
        {
            rerollTimer = 0f;
        }
    }
}
