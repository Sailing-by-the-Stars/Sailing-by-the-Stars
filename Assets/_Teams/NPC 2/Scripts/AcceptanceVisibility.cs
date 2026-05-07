using UnityEngine;

// Old

/// <summary>
/// Ensures only one static NPC is visible at a time.
/// Works with NPCs made of multiple mesh parts.
/// Can be set to trigger once or every time.
/// </summary>
/// Created by: Boas
public class AcceptanceVisibility : MonoBehaviour
{
    [Header("Model Root")]

    [Tooltip("Parent containing all model renderers.")]
    [SerializeField] private GameObject modelRoot;


    [Header("Settings")]

    [Tooltip("Should the NPC start hidden.")]
    [SerializeField] private bool startHidden = true;

    [Tooltip("If true, NPC only appears the first time it becomes visible. If false, it triggers every time.")]
    [SerializeField] private bool oneAndDone = false;


    private Renderer[] npcRenderers;
    private static AcceptanceVisibility currentVisibleNpc;
    private bool hasTriggeredOnce = false;


    private void Awake()
    {
        if (modelRoot == null)
        {
            Debug.LogError($"Model Root not assigned on {name}", this);
            return;
        }

        npcRenderers = modelRoot.GetComponentsInChildren<Renderer>();

        if (startHidden)
        {
            SetNpcVisible(false);
        }
    }


    private void OnBecameVisible()
    {
        if (oneAndDone && hasTriggeredOnce)
        {
            return;
        }

        if (currentVisibleNpc != null && currentVisibleNpc != this)
        {
            currentVisibleNpc.SetNpcVisible(false);
        }

        currentVisibleNpc = this;
        SetNpcVisible(true);

        if (oneAndDone)
        {
            hasTriggeredOnce = true;
        }
    }


    private void OnBecameInvisible()
    {
        if (oneAndDone && hasTriggeredOnce)
        {
            return;
        }

        if (currentVisibleNpc == this)
        {
            currentVisibleNpc = null;
        }

        SetNpcVisible(false);
    }


    /// <summary>
    /// Enables or disables model renderers.
    /// </summary>
    private void SetNpcVisible(bool isVisible)
    {
        foreach (Renderer rendererComponent in npcRenderers)
        {
            if (rendererComponent != null)
            {
                rendererComponent.enabled = isVisible;
            }
        }
    }
}