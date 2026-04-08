using UnityEngine;

public class AstrolabePointer : MonoBehaviour
{
    [SerializeField]
    LayerMask layerMask;

    void Update()
    {
        if (Astrolabe.zoomedIn)
        {
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask))
            {
                TwinklingStar twinklingStar = hit.transform.GetComponent<TwinklingStar>();
                if (twinklingStar != null)
                {
                    Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.red);

                    twinklingStar.Hit(Astrolabe.pointerAngleHax);
                }
            }
        }
    }


    void OnDrawGizmosSelected()
    {
        if (Camera.main == null) return;

        Ray ray = new Ray(transform.position, transform.forward);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(ray.origin, ray.direction * 6000);
    }
}
