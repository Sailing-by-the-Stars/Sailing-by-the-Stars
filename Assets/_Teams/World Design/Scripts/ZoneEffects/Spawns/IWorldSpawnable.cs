/*
 * Created by Christina Pence
 * Contributed to by:
 */
using UnityEngine;

public interface IWorldSpawnable
{
    void Initialize(GameObject instigator, Vector3 zoneCenter);
    void Despawn();
}
