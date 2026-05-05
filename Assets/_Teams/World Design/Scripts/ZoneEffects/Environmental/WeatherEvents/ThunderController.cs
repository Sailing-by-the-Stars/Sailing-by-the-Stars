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
        
        [FormerlySerializedAs("thunderStrickCheckTimeMin")]
        [FormerlySerializedAs("chanceOfThunderStrikePerSecond")]
        
        [Tooltip("How many seconds to wait before checking for the next thunder strike attempt (randomized between min and max).")]
        [FormerlySerializedAs("thunderStrickCheckTime")]
        [SerializeField, Min(0f)] private float thunderStrickTimeMin = 7f;

        [FormerlySerializedAs("thunderStrickCheckTimeMax")] 
        [SerializeField, Min(0f)] private float thunderStrickTimeMax = 12f;
        
        [Header("Liked thunder objects")]
        [Tooltip("List of Thunder objects that are currently linked to a controller. Only objects that are linked to this weather controller will be used to spawn thunder")]
        [SerializeField] private List<GameObject> thunderSpawnerObjects;
        
        private SetThunderStrike thunderAudioController;

        private float thunderCheckTimer;
        private float nextThunderStrickCheckTime;

        private void OnEnable()
        {
            ScheduleNextThunderCheck();
            thunderAudioController = FindFirstObjectByType<SetThunderStrike>();
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
            if (!enable)
            {
                return;
            }

            thunderCheckTimer += Time.deltaTime;
            
            if (thunderCheckTimer >= nextThunderStrickCheckTime)
            {
                thunderCheckTimer = 0f;
                ScheduleNextThunderCheck();

                TriggerThunderStrike();
                thunderAudioController?.SetThunderStrikeF(1f, 1f, 100f);
            }
        }

        private void TriggerThunderStrike()
        {
            if (thunderSpawnerObjects == null || thunderSpawnerObjects.Count == 0)
            {
                return;
            }

            // Select a random thunder spawner object from the list
            GameObject selectedSpawner = thunderSpawnerObjects[Random.Range(0, thunderSpawnerObjects.Count)];
            if (selectedSpawner.TryGetComponent(out ThunderSpawnObject thunderSpawn))
            {
                thunderSpawn.TriggerSpawn();
                
            }
        }
        
        public void ChangeWeatherEventValues(WeatherValues weatherValues)
        {
            enable = weatherValues.thunderActive;
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
            var minTime = Mathf.Min(thunderStrickTimeMin, thunderStrickTimeMax);
            var maxTime = Mathf.Max(thunderStrickTimeMin, thunderStrickTimeMax);
            nextThunderStrickCheckTime = Random.Range(minTime, maxTime);
        }
        
        public void SetThunderSpawnerObjects(List<GameObject> thunderSpawnObjects)
        {
            
            thunderSpawnerObjects = thunderSpawnObjects;
        }
        
        public void ClearThunderSpawnerObjects()
        {
            thunderSpawnerObjects = null;
        }
    }
}