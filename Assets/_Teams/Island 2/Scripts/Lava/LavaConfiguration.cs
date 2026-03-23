using UnityEngine;

namespace Island2.Lava
{
    public class LavaConfiguration
    {
        // Simulation parameters
        public float Gravity { get; set; } = 9.81f;
        public float Damping { get; set; } = 0.99f;
        public float TimeStep { get; set; } = 0.016f;
        
        // Particle parameters
        public float ParticleMass { get; set; } = 1f;
        public float ParticleRadius { get; set; } = 0.1f;
        public float ParticleLifetime { get; set; } = 10f;
        
        // Physics parameters
        public float SpringForceCoefficient { get; set; } = 0.5f;
        public float DampingForceCoefficient { get; set; } = 0.3f;
        public float RestDistance { get; set; } = 0.15f;
        public float MaxVelocity { get; set; } = 50f;
        
        // Temperature and heat
        public float InitialTemperature { get; set; } = 1000f;
        public float EnvironmentTemperature { get; set; } = 20f;
        public float CoolingRate { get; set; } = 5f;
        
        // Emission parameters
        public int ParticlesPerEmission { get; set; } = 3;
        public float EmissionRate { get; set; } = 0.2f;
        public Vector3 EmissionVelocity { get; set; } = Vector3.up * 3f;
    }
}
