using System.Collections.Generic;
using _Teams.World_Design.Scripts.ZoneEffects.Environmental.WeatherEvents.Thunder;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Teams.World_Design.Scripts.ZoneEffects.Environmental.WeatherEvents
{
    public class ThunderController:  MonoBehaviour, IWeatherEventController
    {
        [Tooltip("")]
        [SerializeField] private bool enable = true;
        
        [FormerlySerializedAs("chanceOfThunderStrikePerSecond")]
        [Tooltip("Chance of thunder to stick")]
        [SerializeField, Range(0f, 1f)] private float chanceOfThunderStrikePerInterval = 0.1f;
        
        [Tooltip("How many seconds to wait before checking for the next thunder strike attempt (randomized between min and max).")]
        [FormerlySerializedAs("thunderStrickCheckTime")]
        [SerializeField, Min(0f)] private float thunderStrickCheckTimeMin = 5f;

        [SerializeField, Min(0f)] private float thunderStrickCheckTimeMax = 5f;
        
        [Header("Liked thunder objects")]
        [Tooltip("List of Thunder objects that are currently linked to a controller. Only objects that are linked to this weather controller will be used to spawn thunder")]
        [SerializeField] private List<GameObject> thunderSpawnerObjects;

        private float thunderCheckTimer;
        private float nextThunderStrickCheckTime;

        private void OnEnable()
        {
            ScheduleNextThunderCheck();
        }
        
        private void Awake()
        {
            if (WeatherManager.Instance != null)
            {
                WeatherManager.Instance.Register(this);
            }
        }

        private void Update()
        {
            Debug.Log("test 1");
            if (!enable)
            {
                return;
            }

            Debug.Log("test 1");
            thunderCheckTimer += Time.deltaTime;
            
            if (thunderCheckTimer >= nextThunderStrickCheckTime)
            {
                Debug.Log("test 1");
                // Debug.Log("Thunder list: " + thunderSpawnerObjects?.Count);
                thunderCheckTimer = 0f;
                ScheduleNextThunderCheck();

                if (Random.value < chanceOfThunderStrikePerInterval)
                {
                    Debug.Log("Thunder strike!");
                    TriggerThunderStrike();
                }
            }
        }

        private void TriggerThunderStrike()
        {
            if (thunderSpawnerObjects == null || thunderSpawnerObjects.Count == 0)
            {
                Debug.LogWarning("No thunder spawner objects linked to ThunderController on " + gameObject.name);
                return;
            }

            // Select a random thunder spawner object from the list
            GameObject selectedSpawner = thunderSpawnerObjects[Random.Range(0, thunderSpawnerObjects.Count)];
            if (selectedSpawner.TryGetComponent(out ThunderSpawnObject thunderSpawn))
            {
                Debug.Log("Thunder stick spawning from ");
                thunderSpawn.TriggerSpawn();
            }
            else
            {
                Debug.LogWarning("Selected thunder spawner object does not have a ThunderSpawnObject component: " + selectedSpawner.name);
            }
        }
        
        public void ChangeWeatherEventValues(WeatherValues weatherValues)
        {
            // throw new System.NotImplementedException();
        }

        public void ChangeDirection(Vector3 direction)
        {
            throw new System.NotImplementedException();
        }

        public void SetRandomEventsActive(bool isActive)
        {
            throw new System.NotImplementedException();
        }
        
        // Selecting random time between 2 values to select the new time interval for a thunder strick
        private void ScheduleNextThunderCheck()
        {
            var minTime = Mathf.Min(thunderStrickCheckTimeMin, thunderStrickCheckTimeMax);
            var maxTime = Mathf.Max(thunderStrickCheckTimeMin, thunderStrickCheckTimeMax);
            nextThunderStrickCheckTime = Random.Range(minTime, maxTime);
        }
        
        public void SetThunderSpawnerObjects(List<GameObject> thunderSpawnObjects)
        {
            
            thunderSpawnerObjects = thunderSpawnObjects;
            Debug.Log("Trying to add thunder spawner objects to weather manager: " + thunderSpawnObjects.Count);
        }
        
        public void ClearThunderSpawnerObjects()
        {
            Debug.Log("Clearing thunder spawner objects");
            thunderSpawnerObjects = null;
        }
    }
}