using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

//Creator: Joost
public class Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float movementSpeed = 4f;
    [SerializeField] float sprintMultiplier;

    [Header("Camera Setting")]
    [SerializeField] float mouseSensitivity = 10f;
    [SerializeField] float minXRotation = -90f;
    [SerializeField] float maxXRotation = 90f;

    [Header("Collision Handling")]
    [SerializeField] Rigidbody rb;

    PlayerControls playerControls;

    Camera cam;

    float xCamRotation = 0f;
    float yCamRotation = 0f;

    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cam = GetComponentInChildren<Camera>();
    }

    private void Update()
    {

    }

    void FixedUpdate()
    {
        RotateCamera();
        MovePlayer();
    }

    void MovePlayer()
    {
        Vector2 moveDirection = playerControls.Land.Move.ReadValue<Vector2>();
        
        rb.MovePosition(rb.position + rb.transform.forward * moveDirection.y * movementSpeed * Time.deltaTime);
        rb.MovePosition(rb.position + rb.transform.right * moveDirection.x * movementSpeed * Time.deltaTime);
    }

    void RotateCamera()
    {
        Vector2 lookDirection = playerControls.Land.Look.ReadValue<Vector2>();
        Vector2 cameraMoveDirection = lookDirection * mouseSensitivity * Time.deltaTime;

        // Rotating the Y rotation
        yCamRotation += cameraMoveDirection.x;
        yCamRotation = FixRotationLimit(yCamRotation);
        gameObject.transform.rotation = Quaternion.Euler(0, yCamRotation, 0);


        // Rotating the X rotation
        xCamRotation -= cameraMoveDirection.y;
        xCamRotation = Mathf.Clamp(xCamRotation, minXRotation, maxXRotation);
        cam.transform.localRotation = Quaternion.Euler(xCamRotation, 0, 0);
 }

    /// <summary>
    /// This function makes sure that IF the game gets played long enough
    /// to the point the player manages to rotate enough to reach the float limit on 1 rotation axis, 
    /// it will prevent it by keeping it between -180 and 180.
    /// </summary>
    /// <param name="rotation"></param>
    /// <returns></returns>
    float FixRotationLimit(float rotation)
    {
        if (rotation < -180)
        {
            rotation = 180;
        }
        else if (rotation > 180)
        {
            rotation = -180;
        }
        return rotation;
    }
}
