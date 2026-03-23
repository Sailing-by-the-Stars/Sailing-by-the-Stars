using UnityEngine;

namespace Island2.Lava
{ 
    
    public class LavaParticle
    {
        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }
        public Vector3 Acceleration { get; set; }
        public float Temperature { get; set; }
        public float Age { get; set; }
        public float Mass { get; private set; }
        public float Radius { get; private set; }
        public float Lifetime { get; private set; }
        
        public LavaParticle(Vector3 position, Vector3 velocity, LavaConfiguration config)
        {
            Position = position;
            Velocity = velocity;
            Acceleration = Vector3.zero;
            Temperature = config.InitialTemperature;
            Age = 0f;
            Mass = config.ParticleMass;
            Radius = config.ParticleRadius;
            Lifetime = config.ParticleLifetime;
        }
        
        public void UpdatePhysics(float deltaTime, LavaConfiguration config)
        {
            // Apply gravity
            Acceleration += Vector3.down * config.Gravity;
            
            // Verlet integration
            Velocity = (Velocity + Acceleration * deltaTime) * config.Damping;
            
            // Clamp velocity to prevent instability
            if (Velocity.magnitude > config.MaxVelocity)
            {
                Velocity = Velocity.normalized * config.MaxVelocity;
            }
            
            Position += Velocity * deltaTime;
            Acceleration = Vector3.zero;
            
            // Update temperature with cooling
            Temperature -= config.CoolingRate * deltaTime;
            Age += deltaTime;
        }
        
        public void AddForce(Vector3 force)
        {
            Acceleration += force / Mass;
        }
        
        public bool IsAlive()
        {
            return Age < Lifetime && Temperature > 0f;
        }
    }
}
