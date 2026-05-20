using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class TeleportManager : MonoBehaviour
{
    public static TeleportManager Instance;

    private Dictionary<string, Transform> teleportPoints = new();
    private Transform player;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
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
        StartCoroutine(TeleportBoatRoutine(go));
    }

    private IEnumerator TeleportBoatRoutine(GameObject go)
    {
        Debug.Log("[BoatTP] Coroutine started");
        
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        float distance = 3f;
        Vector3 spawnPos = player.position + player.forward * distance;

        var rb = go.GetComponent<Rigidbody>();
        var buoyancy = go.GetComponent<BuoyancyController>();
        var boatController = go.GetComponent<BoatController>();

        if (buoyancy) buoyancy.enabled = false;
        if (boatController) boatController.enabled = false;

        rb.constraints = RigidbodyConstraints.None;
        rb.isKinematic = false;

        yield return new WaitForFixedUpdate();
        Debug.Log("[BoatTP] After first WaitForFixedUpdate");

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.position = spawnPos;
        go.transform.position = spawnPos;
        Debug.Log($"[BoatTP] Position set to {spawnPos}, actual pos: {go.transform.position}");

        yield return new WaitForFixedUpdate();
        Debug.Log("[BoatTP] After second WaitForFixedUpdate");

        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeAll;

        if (buoyancy) buoyancy.enabled = true;
        if (boatController) boatController.enabled = true;
        
        Debug.Log("[BoatTP] Done");
    }
    public Dictionary<string, Transform> GetTeleportPoints()
    {
        if (teleportPoints.Count == 0)
            FindTeleportPoints();
        return teleportPoints;
    }

    public void TeleportTo(string name)
    {
        if (teleportPoints.Count == 0)
            FindTeleportPoints();

        if (teleportPoints.TryGetValue(name, out var target))
        {
            var movement = player.GetComponent<Movement>();
            Vector3 destination = target.position + Vector3.up * 1.2f;

            if (movement != null)
            {
                movement.TeleportPlayer(destination);
            }
            else
            {
                player.position = destination;
            }
        }
    }
}