using System.Collections.Generic;
using UnityEngine;

public class GraphNode : MonoBehaviour
{
    public List<GraphNode> nextNodes = new List<GraphNode>();

    [HideInInspector]
    public List<GraphEdge> outgoingEdges = new List<GraphEdge>();
}