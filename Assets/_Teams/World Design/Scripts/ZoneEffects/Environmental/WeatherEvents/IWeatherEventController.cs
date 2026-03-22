using UnityEngine;

namespace _Teams.World_Design.Scripts.ZoneEffects.Environmental.WeatherEvents
{
    public interface IWeatherEventController
    {
        void ChangeWeatherEventValues( WeatherValues weatherValues);

		// Optional maybe
        void ChangeDirection(Vector3 direction);
        
        void SetRandomEventsActive(bool isActive);
        
    }
}