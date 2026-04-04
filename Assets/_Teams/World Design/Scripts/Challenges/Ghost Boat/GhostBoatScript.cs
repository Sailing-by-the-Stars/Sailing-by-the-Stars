using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace _Teams.World_Design.Scripts.Challenges.Ghost_Boat
{
    public class GhostBoatScript : MonoBehaviour
    {
        [Header("Follow Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -5f);
        [SerializeField, Min(0f)] private float followSpeed = 0.1f;

        [Header("View detection")]
        [SerializeField] private Camera playerCamera;
        [SerializeField, Range(0.01f, 0.5f)] private float centerTolerance = 0.1f;

        [Header("Optional Rotation")]
        [SerializeField] private bool faceTarget;

        [Header("FMOD Audio")]
        [SerializeField] private EventReference ghostBoatEvent;
        [SerializeField, Tooltip("Optional FMOD parameter name that receives 1 when the boat is centered in view and 0 otherwise.")]
        private string inViewParameterName = "InCenterView";

        private EventInstance audioInstance;
        private PARAMETER_ID inViewParameterId;
        private bool hasInViewParameter;

        private void LateUpdate()
        {
            bool isCentered = IsInCenterView();

            if (target == null)
            {
                return;
            }

            if (!isCentered)
            {
                MoveToTarget();
            }
        }

        private void MoveToTarget()
        {
            Vector3 desiredPosition = target.position + offset;
            float speed = followSpeed <= 0f ? 1f : followSpeed * Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, speed);

            if (faceTarget)
            {
                Vector3 lookDirection = target.position - transform.position;
                if (lookDirection.sqrMagnitude > 0.0001f)
                {
                    transform.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
                }
            }
        }

        private bool IsInCenterView()
        {
            Camera cam = playerCamera != null ? playerCamera : Camera.main;
            if (cam == null)
            {
                return false;
            }

            Vector3 viewportPoint = cam.WorldToViewportPoint(transform.position);

            if (viewportPoint.z <= 0f)
            {
                return false;
            }

            float dx = Mathf.Abs(viewportPoint.x - 0.5f);
            float dy = Mathf.Abs(viewportPoint.y - 0.5f);

            return dx <= centerTolerance && dy <= centerTolerance;
        }
    }
}
