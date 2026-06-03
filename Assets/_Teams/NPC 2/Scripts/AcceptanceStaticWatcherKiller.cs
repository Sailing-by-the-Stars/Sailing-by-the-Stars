using UnityEngine;
using FMODUnity;

public class AcceptanceKiller : MonoBehaviour
{
    [SerializeField] private EventReference destroySound;
    [SerializeField] private GameObject targetToDestroy;
    [SerializeField] private LayerMask occlusionMask;
    [SerializeField] private ParticleSystem particles;

    private Camera cam;
    private bool hasBeenSeen = false;
    private bool destroyed = false;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (destroyed) return;

        bool visible = IsVisibleToPlayer();

        if (visible)
        {
            hasBeenSeen = true;
        }

        if (hasBeenSeen && !visible)
        {
            DestroyTarget();
        }
    }

    bool IsVisibleToPlayer()
    {
        Vector3 camPos = cam.transform.position;
        Vector3 toObj = transform.position - camPos;

        float dot = Vector3.Dot(cam.transform.forward, toObj.normalized);

        if (dot < 0)
        {
            return false;
        }

        Vector3 viewPos = cam.WorldToViewportPoint(transform.position);

        float margin = 0.1f;

        if (viewPos.z <= 0)
            return false;

        if (viewPos.x < -margin || viewPos.x > 1 + margin ||
            viewPos.y < -margin || viewPos.y > 1 + margin)
        {
            return false;
        }

        float dist = toObj.magnitude;

        bool hitSomething = Physics.Raycast(camPos, toObj.normalized, out RaycastHit hit, dist, occlusionMask);

        if (hitSomething)
        {
            if (hit.transform != transform && !hit.transform.IsChildOf(transform))
            {
                return false;
            }
        }

        return true;
    }

    void DestroyTarget()
    {
        destroyed = true;

        if (particles != null)
        {
            particles.Stop();
        }

        if (!destroySound.IsNull)
        {
            RuntimeManager.PlayOneShot(destroySound, transform.position);
        }

        if (targetToDestroy != null)
        {
            Destroy(targetToDestroy);
        }
    }
}