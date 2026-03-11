using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BranchingLineAnimator : MonoBehaviour
{
    public LineRenderer linePrefab;
    public float segmentDuration = 1f;
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private HashSet<string> visitedConnections = new HashSet<string>();

    public static Action<BranchingLineAnimator> endAnim;

    private void Start()
    {
        PathNode firstChild = transform.GetComponentsInChildren<PathNode>()[3];
        StartPath(firstChild);
    }

    public void StartPath(PathNode startNode)
    {
        foreach (var next in startNode.nextNodes)
        {
            TryStartSegment(startNode, next);
        }
    }

    void TryStartSegment(PathNode start, PathNode end)
    {
        string connectionID = start.GetInstanceID() + "_" + end.GetInstanceID();

        if (visitedConnections.Contains(connectionID))
            return;

        visitedConnections.Add(connectionID);
        StartCoroutine(AnimateSegment(start, end));
    }

    IEnumerator AnimateSegment(PathNode start, PathNode end)
    {
        LineRenderer lr = Instantiate(linePrefab, transform);

        lr.positionCount = 2;
        lr.SetPosition(0, start.transform.position);
        lr.SetPosition(1, start.transform.position);

        float time = 0f;

        while (time < segmentDuration)
        {
            time += Time.deltaTime;
            float t = time / segmentDuration;

            float curvedT = curve.Evaluate(t);

            Vector3 pos = Vector3.Lerp(
                start.transform.position,
                end.transform.position,
                curvedT
            );

            lr.SetPosition(1, pos);

            yield return new WaitForEndOfFrame();
        }

        lr.SetPosition(1, end.transform.position);

        if(end.nextNodes.Count == 0)
        {
            endAnim.Invoke(this);

            Debug.Log("ending animation!");
        }


        foreach (var next in end.nextNodes)
        {
            TryStartSegment(end, next);
        }
    }
}