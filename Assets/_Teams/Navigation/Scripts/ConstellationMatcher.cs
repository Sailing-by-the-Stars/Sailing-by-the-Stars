using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ConstellationMatcher : MonoBehaviour
{
    public RawImage overlayImage;

    [Header("Distance Fade")]
    public float fadeStartDistance = 60f;
    public float fadeEndDistance = 10f;
    public float interactDistance = 8f;
    public float maxAlpha = 0.25f;

    Camera cam;
    GraphAnimator animator;

    void Start()
    {
        cam = Camera.main;
        animator = GetComponent<GraphAnimator>();
    }

    void Update()
    {
        if (cam == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        Vector3 toObject = transform.position - ray.origin;
        float projection = Vector3.Dot(toObject, ray.direction);

        Vector3 closestPoint = ray.origin + ray.direction * projection;

        float rayDistance = Vector3.Distance(transform.position, closestPoint);

        float alpha = 0f;

        if (rayDistance <= fadeStartDistance && projection > 0)
        {
            ConstellationCamera.Instance.OrientCam(transform);
            
            alpha = Mathf.InverseLerp(fadeStartDistance, fadeEndDistance, rayDistance);
        }

        if (rayDistance <= interactDistance)
        {
            animator.Step();
            alpha = maxAlpha / 2;
        }
        else
        {
            if (animator.stepping)
            {
                animator.stepping = false;
                animator.ResetSteps();
            }
        }

            Color c = overlayImage.color;
        c.a = Mathf.Lerp(c.a, alpha, Time.deltaTime * 6f);
        if (c.a > maxAlpha)
        {
            c.a = maxAlpha;
        }
        overlayImage.color = c;
    }

    void OnDrawGizmos()
    {
        if (Camera.main == null) return;

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);


        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, fadeStartDistance);

        Gizmos.color = Color.red;
        Vector3 toObject = transform.position - ray.origin;
        float projection = Vector3.Dot(toObject, ray.direction);
        Vector3 closestPoint = ray.origin + ray.direction * projection;

        if(projection > 0)
        {
            Gizmos.DrawSphere(closestPoint, 5f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(ray.origin, ray.direction * projection);

        }
    }
}
