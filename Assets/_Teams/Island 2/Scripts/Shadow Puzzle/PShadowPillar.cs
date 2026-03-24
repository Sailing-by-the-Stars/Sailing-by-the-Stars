using System;
using System.Collections;
using UnityEngine;

public class PShadowPillar : MonoBehaviour
{
    private PShadowGridManager grid;
    
    [Tooltip("Which grid coordinate should this object spawn at.")]
    public Vector2Int startPosition;
    [HideInInspector] public Vector2Int correctPosition;

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

    public void MoveTo(Vector3 targetPosition, float duration, Action onComplete)
    {
        if (moveRoutine != null) StopCoroutine(moveRoutine);

        if (duration <= 0f)
        {
            transform.position = targetPosition;
            onComplete?.Invoke();
            return;
        }

        moveRoutine = StartCoroutine(SmoothMove(targetPosition, duration, onComplete));
    }

    private IEnumerator SmoothMove(Vector3 targetPosition, float duration, Action onComplete)
    {
        Vector3 start = transform.position;
        float timePassed = 0f;

        while (timePassed < duration)
        {
            timePassed += Time.deltaTime;
            float progress = Mathf.SmoothStep(0, 1, timePassed / duration);
            transform.position = Vector3.Lerp(start, targetPosition, progress);
            
            yield return null;
        }

        transform.position = targetPosition;
        moveRoutine = null;
        onComplete?.Invoke();
    }
}