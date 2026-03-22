using UnityEngine;

namespace _Teams.World_Design.Scripts.ZoneEffects.Environmental.WeatherEvents
{
    /// <summary>
    /// This interface is used for all weather events communication with weather manager.
    /// Only those methods should be used to communicate with weather events.
    /// If weather even needs a specific method, it should be added to this interface and implemented in weather event controller (as not implemented).
    /// </summary>
    public interface IWeatherEventController
    {
        // Main method that should be implemented in all weather events, used to change weather event values (like intensity, duration, etc.)
        void ChangeWeatherEventValues(WeatherValues weatherValues);

        // 
        void ChangeDirection(Vector3 direction);
        
        void SetRandomEventsActive(bool isActive);
    }
}