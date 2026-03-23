using UnityEngine;
using System.Collections.Generic;

namespace Island2.Lava
{

    public class LavaPhysics : MonoBehaviour
    {
        [SerializeField] private LavaConfiguration config = new LavaConfiguration();
        [SerializeField] private Material lavaMaterial;
        [SerializeField] private GameObject particlePrefab;
        [SerializeField] private float neighborSearchRadius = 0.5f;
        [SerializeField] private float collisionRestitution = 0.3f;
        [SerializeField] private bool useDebugRendering = true;
        [SerializeField] private bool showParticleCount = true;

        private List<LavaParticle> activeParticles;
        private LavaForce forceCalculator;
        private float emissionTimer;
        private Vector3 emissionPoint;
        private Mesh particleMesh;
        private int frameCount = 0;
        
        private void OnEnable()
        {
            InitializeSystem();
        }
        
        private void OnDisable()
        {
            CleanupSystem();
        }
        
        private void InitializeSystem()
        {
            activeParticles = new List<LavaParticle>();
            forceCalculator = new LavaForce(config);
            emissionTimer = 0f;
            emissionPoint = transform.position;

            if (particlePrefab != null && particleMesh == null)
            {
                MeshFilter meshFilter = particlePrefab.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    particleMesh = meshFilter.sharedMesh;
                }
            }
        }
        
        private void CleanupSystem()
        {
            activeParticles.Clear();
            activeParticles = null;
            forceCalculator = null;
        }
        
        public void SetEmissionPoint(Vector3 position)
        {
            emissionPoint = position;
        }
        
        private void Update()
        {
            emissionTimer -= Time.deltaTime;
            
            if (emissionTimer <= 0f)
            {
                EmitParticles();
                emissionTimer = config.EmissionRate;
            }
        }
        
        private void FixedUpdate()
        {
            UpdateSimulation();
        }

        private void LateUpdate()
        {
            RenderParticles();

            frameCount++;
            if (frameCount >= 30 && showParticleCount)
            {
                Debug.Log($"[Lava] Active Particles: {activeParticles.Count}");
                frameCount = 0;
            }
        }
        
        private void EmitParticles()
        {
            for (int i = 0; i < config.ParticlesPerEmission; i++)
            {
                Vector3 randomVelocity = config.EmissionVelocity + Random.insideUnitSphere * 2f;
                LavaParticle newParticle = new LavaParticle(emissionPoint, randomVelocity, config);
                activeParticles.Add(newParticle);
            }
        }
        
        private void UpdateSimulation()
        {
            // Remove dead particles
            activeParticles.RemoveAll(p => !p.IsAlive());
            
            // Update forces and physics for each particle
            foreach (LavaParticle particle in activeParticles)
            {
                UpdateParticleForces(particle);
                particle.UpdatePhysics(config.TimeStep, config);
                forceCalculator.ResolveCollisions(particle, collisionRestitution);
            }
        }
        
        private void UpdateParticleForces(LavaParticle particle)
        {
            List<LavaParticle> neighbors = forceCalculator.GetNeighbors(particle, activeParticles, neighborSearchRadius);
            
            // Apply spring and damping forces from neighbors
            foreach (LavaParticle neighbor in neighbors)
            {
                Vector3 springForce = forceCalculator.CalculateSpringForce(particle, neighbor);
                Vector3 dampingForce = forceCalculator.CalculateDampingForce(particle, neighbor);
                
                particle.AddForce(springForce);
                particle.AddForce(dampingForce);
            }
            
            // Apply pressure force
            Vector3 pressureForce = forceCalculator.CalculatePressureForce(particle, neighbors);
            particle.AddForce(pressureForce);
        }

        private void RenderParticles()
        {
            if (activeParticles.Count == 0 || lavaMaterial == null || particleMesh == null)
                return;

            RenderParticlesSimple();
        }

        private void RenderParticlesSimple()
        {
            foreach (LavaParticle particle in activeParticles)
            {
                Matrix4x4 matrix = Matrix4x4.TRS(
                    particle.Position,
                    Quaternion.identity,
                    Vector3.one * particle.Radius * 2f
                );

                Color particleColor = GetColorFromTemperature(particle.Temperature);

                MaterialPropertyBlock props = new MaterialPropertyBlock();
                props.SetColor("_Color", particleColor);

                Graphics.DrawMesh(
                    particleMesh,
                    matrix,
                    lavaMaterial,
                    0,
                    null,
                    0,
                    props
                );
            }
        }

        private void RenderParticlesInstanced()
        {
            List<Matrix4x4> matrices = new List<Matrix4x4>();
            List<Vector4> colors = new List<Vector4>();

            foreach (LavaParticle particle in activeParticles)
            {
                Matrix4x4 matrix = Matrix4x4.TRS(
                    particle.Position,
                    Quaternion.identity,
                    Vector3.one * particle.Radius * 2f
                );
                matrices.Add(matrix);

                Color particleColor = GetColorFromTemperature(particle.Temperature);
                colors.Add(particleColor);
            }

            // Enable instancing on material if needed
            if (!lavaMaterial.enableInstancing)
            {
                lavaMaterial.enableInstancing = true;
            }

            // Render in batches
            for (int i = 0; i < matrices.Count; i += 1023)
            {
                int count = Mathf.Min(1023, matrices.Count - i);
                MaterialPropertyBlock props = new MaterialPropertyBlock();
                props.SetVectorArray("_Colors", colors.GetRange(i, count).ToArray());

                Graphics.DrawMeshInstanced(
                    particleMesh,
                    0,
                    lavaMaterial,
                    matrices.GetRange(i, count),
                    props
                );
            }
        }
        
        // Not implemented yet
        private Color GetColorFromTemperature(float temperature)
        {
            float normalizedTemp = Mathf.Clamp01(temperature / config.InitialTemperature);
            
            // Gradient from black -> red -> yellow -> white
            if (normalizedTemp < 0.33f)
            {
                // Black to red
                float t = normalizedTemp / 0.33f;
                return Color.Lerp(Color.black, new Color(1f, 0.2f, 0f), t);
            }
            else if (normalizedTemp < 0.66f)
            {
                // Red to yellow
                float t = (normalizedTemp - 0.33f) / 0.33f;
                return Color.Lerp(new Color(1f, 0.2f, 0f), new Color(1f, 1f, 0f), t);
            }
            else
            {
                // Yellow to white
                float t = (normalizedTemp - 0.66f) / 0.34f;
                return Color.Lerp(new Color(1f, 1f, 0f), Color.white, t);
            }
        }
        
        public int GetParticleCount()
        {
            return activeParticles.Count;
        }
    }
}

// TODO: Implement temeprature for lava for a better look
