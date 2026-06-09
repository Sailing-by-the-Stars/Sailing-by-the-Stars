using System;
using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class PShadowPillar : MonoBehaviour
{
    private PShadowGridManager grid;
    
    [Header("Grid Coordinates")]
    [Tooltip("Which grid coordinate should this object spawn at.")]
    public Vector2Int startPosition;
    [Tooltip("Where this Pillar needs to be on the grid in order to be considered in the correct position.")]
    [SerializeField] private Vector2Int correctPosition;
    public bool isBracelet = true;
    
    [HideInInspector] public Vector2Int gridPos;

    private Coroutine moveRoutine;
    
    [SerializeField] private EventReference pillarDragEvent;
    [SerializeField] private float volume = 0.5f;

    private EventInstance pillarDragInstance;
    
    private void Start()
    {
        if (grid == null)
        {
            grid = FindFirstObjectByType<PShadowGridManager>();
        }
        if (grid == null)
        {
            Debug.LogError($"{name} could not find the PShadowGridManager!");
        }

        grid.RegisterPillar(this, startPosition);
    }

    public bool IsInCorrectPosition()
    {
        return gridPos == correctPosition;
    }

    public void MoveTo(Vector3 targetPosition, Quaternion targetRotation, Vector2Int newGridPos, float duration, Action onComplete)
    {
        if (moveRoutine != null) StopCoroutine(moveRoutine);

        if (duration <= 0f)
        {
            transform.position = targetPosition;
            onComplete?.Invoke();
            return;
        }

        StartAudio();
        moveRoutine = StartCoroutine(SmoothMove(targetPosition, targetRotation, newGridPos, duration, onComplete));
    }
    
    private void StartAudio()
    {
        if (!pillarDragInstance.isValid())
        {
            pillarDragInstance = RuntimeManager.CreateInstance(pillarDragEvent);
            pillarDragInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        pillarDragInstance.setVolume(volume);
        pillarDragInstance.start();
    }

    private IEnumerator SmoothMove(Vector3 targetPosition, Quaternion targetRotation, Vector2Int newGridPos, float duration, Action onComplete)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float timePassed = 0f;

        while (timePassed < duration)
        {
            timePassed += Time.deltaTime;
            float progress = Mathf.SmoothStep(0, 1, timePassed / duration);
            
            transform.position = Vector3.Lerp(startPos, targetPosition, progress);
            transform.rotation = Quaternion.Lerp(startRot, targetRotation, progress);
            
            yield return null;
        }

        transform.position = targetPosition;
        transform.rotation = targetRotation;
        gridPos = newGridPos;
        moveRoutine = null;
        onComplete?.Invoke();
    }
}