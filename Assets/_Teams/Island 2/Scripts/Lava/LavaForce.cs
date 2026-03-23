using UnityEngine;
using System.Collections.Generic;

namespace Island2.Lava
{

    public class LavaForce
    {
        private LavaConfiguration config;
        
        public LavaForce(LavaConfiguration configuration)
        {
            config = configuration;
        }
        
        public Vector3 CalculateSpringForce(LavaParticle particleA, LavaParticle particleB)
        {
            Vector3 direction = particleB.Position - particleA.Position;
            float distance = direction.magnitude;
            
            if (distance < 0.001f)
                return Vector3.zero;
            
            float displacement = distance - config.RestDistance;
            float force = -config.SpringForceCoefficient * displacement;
            
            return (direction.normalized * force);
        }
        
        public Vector3 CalculateDampingForce(LavaParticle particleA, LavaParticle particleB)
        {
            Vector3 relativeVelocity = particleB.Velocity - particleA.Velocity;
            Vector3 direction = (particleB.Position - particleA.Position).normalized;
            
            float velocityAlongDirection = Vector3.Dot(relativeVelocity, direction);
            
            return -config.DampingForceCoefficient * velocityAlongDirection * direction;
        }
        
        public void ResolveCollisions(LavaParticle particle, float collisionRestitution = 0.5f)
        {
            RaycastHit hit;

            // Only raycast if particle is moving
            if (particle.Velocity.magnitude < 0.001f)
                return;

            Vector3 rayDirection = particle.Velocity.normalized;
            float rayDistance = particle.Velocity.magnitude * Time.deltaTime;

            // Raycast from current position in direction of velocity
            if (Physics.Raycast(particle.Position, rayDirection, out hit, rayDistance + particle.Radius))
            {
                if (hit.collider.isTrigger)
                    return;

                // Move particle outside of collision
                particle.Position = hit.point + hit.normal * (particle.Radius * 0.5f);

                // Reflect velocity based on surface normal
                particle.Velocity = Vector3.Reflect(particle.Velocity, hit.normal) * collisionRestitution;
            }
        }
        
        public Vector3 CalculatePressureForce(LavaParticle particle, List<LavaParticle> neighbors)
        {
            Vector3 pressureForce = Vector3.zero;
            float normalizedTemperature = Mathf.Clamp01(particle.Temperature / config.InitialTemperature);
            
            foreach (LavaParticle neighbor in neighbors)
            {
                if (neighbor == particle)
                    continue;
                
                Vector3 direction = (particle.Position - neighbor.Position).normalized;
                float distance = Vector3.Distance(particle.Position, neighbor.Position);
                
                if (distance > 0.001f)
                {
                    float pressure = normalizedTemperature * config.SpringForceCoefficient;
                    pressureForce += direction * pressure;
                }
            }
            
            return pressureForce;
        }
        
        public List<LavaParticle> GetNeighbors(LavaParticle particle, List<LavaParticle> allParticles, float searchRadius)
        {
            List<LavaParticle> neighbors = new List<LavaParticle>();
            
            foreach (LavaParticle other in allParticles)
            {
                if (other == particle)
                    continue;
                
                float distance = Vector3.Distance(particle.Position, other.Position);
                if (distance < searchRadius)
                {
                    neighbors.Add(other);
                }
            }
            
            return neighbors;
        }
    }
}
