using UnityEngine;

namespace _Teams.World_Design.Scripts.ZoneEffects.Environmental.WeatherEvents.Thunder
{
    public class ThunderObject : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float despawnDelaySeconds = 1f;

        private void OnEnable()
        {
            Destroy(gameObject, despawnDelaySeconds);
        }
    }
}