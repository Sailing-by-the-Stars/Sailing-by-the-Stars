using UnityEngine;

public interface IZoneEffect
{
    // remove instigator if not needed
    void OnEnter(GameObject instigator);
    void OnExit(GameObject instigator);
}
