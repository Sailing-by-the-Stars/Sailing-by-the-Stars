using System;
using System.Collections;
using UnityEngine;

public class PShadowPillar : MonoBehaviour
{
    private PShadowGridManager grid;
    
    [Header("Grid Coordinates")]
    [Tooltip("Which grid coordinate should this object spawn at.")]
    public Vector2Int startPosition;
    [Tooltip("Where this Pillar needs to be on the grid in order to be considered in the correct position.")]
    [SerializeField] private Vector2Int correctPosition;
    [HideInInspector] public Vector2Int gridPos;

    private Coroutine moveRoutine;
    
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

        moveRoutine = StartCoroutine(SmoothMove(targetPosition, targetRotation, newGridPos, duration, onComplete));
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