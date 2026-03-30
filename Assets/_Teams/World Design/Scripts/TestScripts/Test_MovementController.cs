using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Test_MovementController : MonoBehaviour, IKnockable
{
    public float moveSpeed = 10f;
    public float turnSpeed = 50f;
    private Rigidbody rb;
    private Vector3 knockbackVelocity = Vector3.zero;
    private float spinVelocity = 0f;
    [SerializeField] private float knockbackDrag = 3f;

    private bool inputEnabled = true;

    void Awake() => rb = GetComponent<Rigidbody>();

    public void ApplyKnockback(Vector3 velocity, float spin)
    {
        knockbackVelocity = velocity;
        spinVelocity = spin;
        inputEnabled = false;
    }

    void FixedUpdate()
    {
        float forward = 0f;
        float turn = 0f;

        if (inputEnabled)
        {
            if (Keyboard.current.wKey.isPressed) forward += 1f;
            if (Keyboard.current.sKey.isPressed) forward -= 1f;
            if (Keyboard.current.aKey.isPressed) turn -= 1f;
            if (Keyboard.current.dKey.isPressed) turn += 1f;
        }

        rb.MovePosition(rb.position
            + transform.forward * forward * moveSpeed * Time.fixedDeltaTime
            + knockbackVelocity * Time.fixedDeltaTime);

        rb.MoveRotation(rb.rotation
            * Quaternion.Euler(0f, (turn * turnSpeed + spinVelocity) * Time.fixedDeltaTime, 0f));

        knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, knockbackDrag * Time.fixedDeltaTime);
        spinVelocity = Mathf.Lerp(spinVelocity, 0f, knockbackDrag * Time.fixedDeltaTime);

        if (knockbackVelocity.magnitude < 0.1f)
        {
            inputEnabled = true;
            knockbackVelocity = Vector3.zero;
        }
    }
}