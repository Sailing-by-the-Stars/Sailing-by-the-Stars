using UnityEngine;

namespace _Teams.World_Design.Scripts.Challenges.Ghost_Boat
{
    public class GhostBoatScript : MonoBehaviour
    {
        [Header("Follow Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -5f);
        [SerializeField, Min(0f)] private float followSpeed = 0.1f;

        [Header("Optional Rotation")]
        [SerializeField] private bool faceTarget;

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desiredPosition = target.position + offset;
            float t = followSpeed <= 0f ? 1f : followSpeed * Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, t);

            if (faceTarget)
            {
                Vector3 lookDirection = target.position - transform.position;
                if (lookDirection.sqrMagnitude > 0.0001f)
                {
                    transform.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
                }
            }
        }
    }
}
