using System;
using UnityEngine;

public class WindController : MonoBehaviour
{
    [Header("Direction")]
    [SerializeField] private bool horizontalOnly = true;
    [SerializeField] private bool randomizeOnStart = true;
    [SerializeField] private Vector3 currentWindDirection = Vector3.forward;

    [Header("Auto Reroll")]
    [SerializeField] private bool autoReroll;
    [SerializeField] private float rerollIntervalSeconds = 10f;

    [Header("Wind Object")]
    [SerializeField] private WindObject windObject;

    private float rerollTimer;

    public Vector3 CurrentWindDirection => currentWindDirection;

    public event Action<Vector3> WindDirectionChanged;

    private void Awake()
    {
        ResolveWindObjectReference();

        if (randomizeOnStart || currentWindDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            RerollWindDirection();
            return;
        }

        currentWindDirection = currentWindDirection.normalized;
        SyncWindObject();
    }

    private void Update()
    {
        if (!autoReroll || rerollIntervalSeconds <= 0f)
        {
            return;
        }

        rerollTimer += Time.deltaTime;
        if (rerollTimer < rerollIntervalSeconds)
        {
            return;
        }

        rerollTimer = 0f;
        RerollWindDirection();
    }

    public void RerollWindDirection()
    {
        currentWindDirection = GenerateRandomDirection();
        WindDirectionChanged?.Invoke(currentWindDirection);
        SyncWindObject();
        // PrintWindDirection();
    }

    [ContextMenu("Print Wind Direction")]
    public void PrintWindDirection()
    {
        Debug.Log($"[{nameof(WindController)}] Wind direction: {currentWindDirection}", this);
    }

    private void ResolveWindObjectReference()
    {
        if (windObject != null)
        {
            return;
        }

        windObject = FindFirstObjectByType<WindObject>();
    }

    private void SyncWindObject()
    {
        if (windObject == null)
        {
            return;
        }

        windObject.SetWindDirection(currentWindDirection);
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
}
