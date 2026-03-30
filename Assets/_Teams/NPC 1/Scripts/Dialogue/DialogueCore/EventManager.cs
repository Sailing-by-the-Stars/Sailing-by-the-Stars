using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class EventManager : MonoBehaviour
{
    [System.Serializable]
    public class SceneEvent
    {
        public string eventID;       // Unique ID, e.g., "GiveRelic"
        public UnityEvent onTrigger; // Designers assign methods here
    }

    public List<SceneEvent> events = new List<SceneEvent>();
    private Dictionary<string, UnityEvent> eventLookup;

    private void Awake()
    {
        eventLookup = new Dictionary<string, UnityEvent>();
        foreach (var e in events)
        {
            if (!string.IsNullOrEmpty(e.eventID))
                eventLookup[e.eventID] = e.onTrigger;
        }
    }

    public void TriggerEvent(string id)
    {
        if (eventLookup.TryGetValue(id, out var evt))
        {
            evt.Invoke();
            Debug.Log($"Triggered event: {id}");
        }
        else
        {
            Debug.LogWarning($"Event not found: {id}");
        }
    }
}