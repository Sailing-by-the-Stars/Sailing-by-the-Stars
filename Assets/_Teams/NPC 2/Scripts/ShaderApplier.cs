using UnityEngine;

// Programmer: Boas

public class MaterialManager : MonoBehaviour
{
    [Header("Materials")]

    [Tooltip("the materials to apply to each hair bushel")]

    [SerializeField] private Material[] materialsToApply;

    /// <summary>
    /// Applies the assigned materials to all child renderers of this object.
    /// </summary>
    private void Start()
    {
        ApplyMaterialsToChildren();
    }

    /// <summary>
    /// Finds all child renderers and assigns the configured materials to them.
    /// </summary>
    private void ApplyMaterialsToChildren()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            renderer.materials = materialsToApply;
        }
    }
}