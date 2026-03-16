using System.Collections;
using UnityEngine;

public class Pillar : MonoBehaviour
{
    private PShadowGridManager grid;
    [HideInInspector] public Vector2Int gridPos;
    
    [Tooltip("Which grid coordinate should this object spawn at.")]
    [SerializeField] private Vector2Int startPosition;

    private Coroutine moveRoutine;
    
    private void Start()
    {
        if (grid == null)
        {
            grid = FindFirstObjectByType<PShadowGridManager>();
        }
        
        grid.RegisterPillar(this, startPosition);
    }

    public void MoveTo(Vector3 targetPosition, Vector2Int newGridPos, float duration)
    {
        if (moveRoutine != null) StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(SmoothMove(targetPosition, newGridPos, duration));
    }

    private IEnumerator SmoothMove(Vector3 targetPosition, Vector2Int newGridPos, float duration)
    {
        Vector3 start = transform.position;

        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = Mathf.SmoothStep(0, 1, t / duration);

            transform.position = Vector3.Lerp(start, targetPosition, progress);

            yield return null;
        }

        transform.position = targetPosition;
        gridPos = newGridPos;
    }
}