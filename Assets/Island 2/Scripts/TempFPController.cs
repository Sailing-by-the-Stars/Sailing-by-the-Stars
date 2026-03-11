using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class TempFPController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Look")]
    public Transform playerCamera;
    public float mouseSensitivity = 0.1f;
    public float minPitch = -89f;
    public float maxPitch = 89f;

    private CharacterController charController;

    private PlayerInput input;
    InputAction moveAction;
    InputAction lookAction;
    InputAction sprintAction;
    InputAction jumpAction;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool jumpPressed;
    private bool sprintHeld;

    private float verticalVelocity;
    private float pitch;

    void Awake()
    {
        charController = GetComponent<CharacterController>();
        input = GetComponent<PlayerInput>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        moveAction = input.actions["Move"];
        lookAction = input.actions["Look"];
        sprintAction = input.actions["Sprint"];
        jumpAction = input.actions["Jump"];

        jumpAction.performed += ctx => OnJump(ctx);
    }

    void Update()
    {
        //Debug.Log(1f / Time.deltaTime);

        sprintHeld = sprintAction.IsPressed();
        moveInput = moveAction.ReadValue<Vector2>();
        lookInput = lookAction.ReadValue<Vector2>();

        HandleLook();
        HandleMove();
    }

    void HandleLook()
    {
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        playerCamera.localEulerAngles = new Vector3(pitch, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMove()
    {
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        float speed = sprintHeld ? sprintSpeed : walkSpeed;

        if (charController.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;

        if (jumpPressed && charController.isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpPressed = false;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move.normalized * speed + Vector3.up * verticalVelocity;
        charController.Move(velocity * Time.deltaTime);
    }

    // -------- Input Callbacks --------

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) jumpPressed = true;
    }
}