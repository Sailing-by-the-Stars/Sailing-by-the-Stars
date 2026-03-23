using UnityEngine;
using Island2.Lava;

public class LavaVisualRenderer : MonoBehaviour
{
    [SerializeField] private LavaPhysics lavaPhysics;
    [SerializeField] private Material particleMaterial;
    [SerializeField] private Mesh particleMesh;
    [SerializeField] private bool renderParticles = true;
    
    private void OnEnable()
    {
        if (lavaPhysics == null)
            lavaPhysics = GetComponent<LavaPhysics>();
    }
    
    private void OnRenderObject()
    {
        if (!renderParticles || lavaPhysics == null || particleMaterial == null)
            return;
        
        particleMaterial.SetPass(0);
        
        int particleCount = lavaPhysics.GetParticleCount();
        Debug.Log($"Rendering {particleCount} particles");
    }
}
