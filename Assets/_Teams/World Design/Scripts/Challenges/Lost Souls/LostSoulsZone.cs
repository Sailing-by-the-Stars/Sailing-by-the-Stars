using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Teams.World_Design.Scripts.Challenges.Lost_Souls
{
    public class LostSoulsZone : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string instigatorTag = "Player";
        [SerializeField] private Transform triggerZone;
        [SerializeField] private Transform spawnZone;
        
        [Header("Spawns")]
        [Tooltip("List of prefabs with LostSoulBoatScript attached")]
        [SerializeField] private List<LostSoulBoatScript> lostSoulBoatPrefabList = new List<LostSoulBoatScript>();
        
        [Tooltip("List to track active spawned boats in the scene")]
        [SerializeField] private List<LostSoulBoatScript> lostSoulBoatList = new List<LostSoulBoatScript>();
        
        [Tooltip("Maximum number of boats to spawn during this event.")]
        [SerializeField] private int maxBoatsToSpawn = 5;
        
        [Header("Spawn Timing")]
        [SerializeField] private float minSpawnInterval = 1f;
        [SerializeField] private float maxSpawnInterval = 3f;

        private GameObject spawnedGhost;
        private SetDenialEventMusic soundController;
        private Coroutine spawnCoroutine;
        
        private void Start()
        {
            if (triggerZone == null) triggerZone = transform.Find("TriggerZone");
            if (spawnZone == null) spawnZone = transform.Find("SpawnZone");

            if (triggerZone != null)
            {
                // Attach helper to forward trigger events from the child to this script
                TriggerForwarder forwarder = triggerZone.gameObject.AddComponent<TriggerForwarder>();
                forwarder.triggerHandler = this;
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(instigatorTag))
            {
                Debug.Log("Triggered");
                
                spawnCoroutine = StartCoroutine(SpawnBoatsRoutine());
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(instigatorTag))
            {
                if (spawnCoroutine != null)
                {
                    StopCoroutine(spawnCoroutine);
                    spawnCoroutine = null;
                }
            }
        }
        
        private IEnumerator SpawnBoatsRoutine()
        {
            if (lostSoulBoatPrefabList == null || lostSoulBoatPrefabList.Count == 0)
            {
                Debug.LogWarning("Lost Soul Boat Prefab List is empty. Cannot spawn boats.");
                yield break;
            }

            while (true)
            {
                if (lostSoulBoatList.Count < maxBoatsToSpawn)
                {
                    float delay = Random.Range(minSpawnInterval, maxSpawnInterval);
                    yield return new WaitForSeconds(delay);

                    // Double-check the limit after waiting
                    if (lostSoulBoatList.Count < maxBoatsToSpawn)
                    {
                        LostSoulBoatScript boatPrefab = lostSoulBoatPrefabList[Random.Range(0, lostSoulBoatPrefabList.Count)];
                        
                        if (boatPrefab != null)
                        {
                            Vector3 randomPosition = GetRandomPositionInsideSpawnZone();
                            Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                            LostSoulBoatScript spawnedBoat = Instantiate(boatPrefab, randomPosition, randomRotation);
                            lostSoulBoatList.Add(spawnedBoat);
                            spawnedBoat.OnFadeComplete += HandleBoatFaded;
                        }
                    }
                }
                else
                {
                    yield return null;
                }
            }
        }

        private void HandleBoatFaded(LostSoulBoatScript boat)
        {
            if (boat != null)
            {
                boat.OnFadeComplete -= HandleBoatFaded;
                lostSoulBoatList.Remove(boat);
                Destroy(boat.gameObject);
            }
        }

        private Vector3 GetRandomPositionInsideSpawnZone()
        {
            if (spawnZone != null && spawnZone.TryGetComponent(out Collider zoneCollider))
            {
                Bounds bounds = zoneCollider.bounds;
                return new Vector3(
                    Random.Range(bounds.min.x, bounds.max.x),
                    0,
                    Random.Range(bounds.min.z, bounds.max.z)
                );
            }
            
            return spawnZone != null ? spawnZone.position : transform.position;
        }


    }

    /// <summary>
    /// Helper class to forward trigger events from child colliders.
    /// </summary>
    public class TriggerForwarder : MonoBehaviour
    {
        public LostSoulsZone triggerHandler;

        private void OnTriggerEnter(Collider other)
        {
            if (triggerHandler != null)
            {
                triggerHandler.OnTriggerEnter(other);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (triggerHandler != null)
            {
                triggerHandler.OnTriggerExit(other);
            }
        }
    }
}