using System;
using System.Collections;
using FMODUnity;
using UnityEngine;

namespace _Teams.World_Design.Scripts.Challenges.Lost_Souls
{
    public class LostSoulFollowerZone : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string instigatorTag = "Player";
        [SerializeField] private string boatTag = "Boat";
        [SerializeField] private string lostSoulTag = "LostSoulFollower"; 
        [SerializeField] private GameObject lostSoulPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform triggerZone;
        [SerializeField] private bool spawnOnlyOnce = true;
        [SerializeField, Range(0, 1)] private float musicIntensity= 0.5f;
        

        private GameObject spawnedLostSoul;
        private SetDenialEventMusic soundController;
        
        private void Awake()
        {
            if (triggerZone == null)
            {
                triggerZone = transform.Find("TriggerZone");
            }

            if (triggerZone != null && triggerZone != transform)
            {
                TriggerForwarder relay = triggerZone.gameObject.AddComponent<TriggerForwarder>();
                relay.onTriggerEnterAction = HandleTriggerEnter;
                relay.onTriggerExitAction = HandleTriggerExit;
            }
            
            if(spawnPoint == null)
            {
                spawnPoint = transform.Find("SpawnPoint");
            }
        }

        public void HandleTriggerEnter(Collider other)
        {
            soundController = FindFirstObjectByType<SetDenialEventMusic>();
            if (!other.CompareTag(instigatorTag))
            {
                return;
            }

            if (lostSoulPrefab == null)
            {
                Debug.LogWarning("Lost Soul follower prefab is not assigned.", this);
                return;
            }

            if (spawnOnlyOnce && spawnedLostSoul != null)
            {
                return;
            }

            if (soundController != null)
            {
                soundController.SetDenialMusic(musicIntensity);
            }

            Transform point = spawnPoint != null ? spawnPoint : transform;
            spawnedLostSoul = Instantiate(lostSoulPrefab, point.position, point.rotation);

            if (spawnedLostSoul.TryGetComponent(out LostSoulFollower lostSoulFollower))
            {
                GameObject boat = GameObject.FindGameObjectWithTag(boatTag);
                lostSoulFollower.SetTarget(boat != null ? boat.transform : other.transform);
            }
            else
            {
                Debug.LogWarning("Spawned lost soul object has no LostSoulFollower component.", spawnedLostSoul);
            }

        }

        public void HandleTriggerExit(Collider other)
        {
            if (other.CompareTag(instigatorTag))
            {
                soundController = FindFirstObjectByType<SetDenialEventMusic>();
                if (soundController != null)
                {
                    soundController.SetDenialMusic(0f);
                }

                LostSoulFollower[] lostSoulFollowers = FindObjectsByType<LostSoulFollower>(FindObjectsSortMode.None);

                foreach (LostSoulFollower lostSoulFollower in lostSoulFollowers)
                {
                    if (string.IsNullOrEmpty(lostSoulTag) || lostSoulTag == "Untagged" || lostSoulFollower.gameObject.CompareTag(lostSoulTag))
                    {
                        Destroy(lostSoulFollower.gameObject);
                    }
                }

            }
        }
    }
}