using UnityEngine;

public interface IWorldSpawnable
{
    void Initialize(GameObject instigator, Vector3 zoneCenter);
    void Despawn();
}
