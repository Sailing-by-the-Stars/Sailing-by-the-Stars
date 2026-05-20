using UnityEngine;

/// Created by: Boas

/// <summary>
/// Destroys NPC when off screen.
/// </summary>
public class AcceptanceKiller : MonoBehaviour
{
    [SerializeField] private Renderer visibilityRenderer;
    private float screenMargin = 0.15f;

    private bool hasBeenSeen = false;

    private void Update()
    {
        if (visibilityRenderer == null) return;

        Vector3 viewportPos =
            Camera.main.WorldToViewportPoint(
                visibilityRenderer.transform.position
            );

        bool visible =
            viewportPos.z > 0 &&
            viewportPos.x > -screenMargin &&
            viewportPos.x < 1f + screenMargin &&
            viewportPos.y > -screenMargin &&
            viewportPos.y < 1f + screenMargin;

        if (visible)
        {
            hasBeenSeen = true;
            return;
        }

        if (hasBeenSeen && !visible)
        {
            Destroy(gameObject, 0.1f);
        }
    }
}