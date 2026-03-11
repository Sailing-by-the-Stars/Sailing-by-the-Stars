using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GraphEdge
{
    public GraphNode start;
    public GraphNode end;
    public LineRenderer line;
}

public class GraphSignal
{
    public GraphEdge edge;
    public float time;
}

public class GraphAnimator : MonoBehaviour
{
    public LineRenderer linePrefab;
    public float segmentDuration = 1f;
    public AnimationCurve curve;

    private List<GraphEdge> edges = new List<GraphEdge>();
    private List<GraphSignal> activeSignals = new List<GraphSignal>();
    private HashSet<GraphEdge> visitedEdges = new HashSet<GraphEdge>();

    public GraphNode startNode;

    public bool stepping = false;
    public bool NeedsActivation = false;
    List<GraphNode> nodes;

    public static Action<GraphAnimator> endAnim;

    private void Start()
    {
        nodes = GetComponentsInChildren<GraphNode>().ToList<GraphNode>();
        BuildGraph(nodes);
        StartFrom(startNode);
    }



    void BuildGraph(List<GraphNode> nodes)
    {
        foreach (GraphEdge edge in edges)
        {
            Destroy(edge.line.gameObject);
        }

        edges.Clear();

        foreach (var node in nodes)
            node.outgoingEdges.Clear();

        foreach (var node in nodes)
        {
            foreach (var next in node.nextNodes)
            {
                LineRenderer lr = Instantiate(linePrefab, transform);

                lr.positionCount = 2;
                lr.SetPosition(0, node.transform.position);
                lr.SetPosition(1, node.transform.position);

                GraphEdge edge = new GraphEdge
                {
                    start = node,
                    end = next,
                    line = lr
                };

                edges.Add(edge);
                node.outgoingEdges.Add(edge);
            }
        }
    }

    public void StartFrom(GraphNode start)
    {
        foreach (var edge in start.outgoingEdges)
            StartSignal(edge);
    }

    void StartSignal(GraphEdge edge)
    {
        if (visitedEdges.Contains(edge))
            return;

        visitedEdges.Add(edge);

        activeSignals.Add(new GraphSignal
        {
            edge = edge,
            time = 0
        });
    }


    public void ResetSteps()
    {
        if (enabled == false)
            return;

        visitedEdges = new HashSet<GraphEdge>();

        foreach (var edge in edges)
        {
            Destroy(edge.line.gameObject);
        }
        edges.Clear();

        foreach (var node in nodes)
            node.outgoingEdges.Clear();

        activeSignals.Clear();

        BuildGraph(nodes);
        StartFrom(startNode);
    }


    private void Update()
    {
        if (!NeedsActivation)
        {
            Step();
        }
    }



    public void Step()
    {
        if (stepping && enabled)
        {
            for (int i = activeSignals.Count - 1; i >= 0; i--)
            {
                var signal = activeSignals[i];

                signal.time += Time.deltaTime;
                float t = signal.time / segmentDuration;

                float curvedT = curve.Evaluate(t);

                Vector3 pos = Vector3.Lerp(
                    signal.edge.start.transform.position,
                    signal.edge.end.transform.position,
                    curvedT
                );

                signal.edge.line.SetPosition(1, pos);

                if (t >= 1f)
                {
                    signal.edge.line.SetPosition(1, signal.edge.end.transform.position);

                    if (signal.edge.end.nextNodes.Count == 0)
                    {
                        endAnim?.Invoke(this);

                        this.enabled = false;
                    }

                    foreach (var nextEdge in signal.edge.end.outgoingEdges)
                    {
                        StartSignal(nextEdge);
                    }

                    activeSignals.RemoveAt(i);
                }
            }
        }
        else
        {
            stepping = true;
        }
    }
}