using UnityEngine;
using System.Collections.Generic;

public class TeleportManager : MonoBehaviour
{
    public static TeleportManager Instance;

    private Dictionary<string, Transform> teleportPoints = new();
    private Transform player;

    private void Awake()
    {
        Instance = this;
        player = GameObject.FindGameObjectWithTag("Player").transform;

        FindTeleportPoints();
    }

    private void FindTeleportPoints()
    {
        teleportPoints.Clear();

        foreach (var go in GameObject.FindGameObjectsWithTag("TeleportPoint"))
        {
            if (go.name.StartsWith("TP_"))
            {
                string cleanName = go.name.Replace("TP_", "");
                teleportPoints[cleanName] = go.transform;
            }
        }
    }
    public void TeleportObjectNextToPlayer(GameObject go)
    {
        go.transform.position = player.position + Vector3.right * 1.5f;
    }

    public Dictionary<string, Transform> GetTeleportPoints() => teleportPoints;

    public void TeleportTo(string name)
    {
        if (teleportPoints.TryGetValue(name, out var target))
        {
            player.position = target.position + Vector3.up * 1.2f;
        }
    }
}