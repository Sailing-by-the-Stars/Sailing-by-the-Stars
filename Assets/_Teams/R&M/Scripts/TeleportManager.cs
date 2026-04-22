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

        foreach (var t in FindObjectsByType<Transform>(FindObjectsSortMode.None))
        {
            if (t.name.StartsWith("TP_"))
            {
                string cleanName = t.name.Replace("TP_", "");
                teleportPoints[cleanName] = t;
            }
        }
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