using System.Collections.Generic;
using UnityEngine;

public class RainRIpplenormals : MonoBehaviour
{
    private ParticleSystem rainSystem;
    private ParticleSystem rippleSystem;
    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

    void Start()
    {
        rainSystem = GetComponent<ParticleSystem>();
        rippleSystem = this.transform.GetChild(0).GetComponent<ParticleSystem>();
    }

    void OnParticleCollision(GameObject other)
    {
        // Get all collision points from the rain particles
        int numCollisionEvents = rainSystem.GetCollisionEvents(other, collisionEvents);

        for (int i = 0; i < numCollisionEvents; i++)
        {
            EmitRipple(collisionEvents[i]);
        }
    }

    void EmitRipple(ParticleCollisionEvent collisionEvent)
    {
        var emitParams = new ParticleSystem.EmitParams();

        Quaternion surfaceRotation = Quaternion.LookRotation(collisionEvent.normal);

        Vector3 spawnPos = collisionEvent.intersection;

        emitParams.position = spawnPos;
        emitParams.rotation3D = surfaceRotation.eulerAngles;

        rippleSystem.Emit(emitParams, 1);
    }
}