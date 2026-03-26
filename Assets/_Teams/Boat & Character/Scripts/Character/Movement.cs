using UnityEngine;

//Creator: Joost
//Edited by: Johan
//edited by: Jardi (the sprint working, hacky though it is)
public class Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float movementSpeed = 4f;
    [SerializeField] float sprintMultiplier;
    [SerializeField] float jumpStrength = 20f;

    [Header("Camera Setting")]
    [SerializeField] float mouseSensitivity = 10f;
    [SerializeField] float minXRotation = -90f;
    [SerializeField] float maxXRotation = 90f;

    [Header("Collision Handling")]
    [SerializeField] Rigidbody rb;

    [SerializeField] Transform[] groundChecks;
    bool isGrounded = false;
    bool isOnBoat = false;

    PlayerControls playerControls;

    Camera cam;

    float xCamRotation = 0f;
    float yCamRotation = 0f;

    [Header("Player walk on boat")]
    [SerializeField] private GameObject boat;
    [SerializeField] private Transform seatPosition;

    private BoatController boatController;
    private BuoyancyController buoyancyController;

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
        rb = GetComponent<Rigidbody>();

        boatController = boat.GetComponent<BoatController>();
        buoyancyController = boat.GetComponent<BuoyancyController>();
    }

    private void Update()
    {
        if (!isOnBoat)
        {
            Jump();
            IsGrounded();

            if (isGrounded)
            {
                /*if (rb.linearVelocity.x > 0 || rb.linearVelocity.z > 0)
                {
                    rb.linearVelocity += new Vector3(-rb.linearVelocity.x, 0, -rb.linearVelocity.z);
                }*/
                /*if (rb.angularVelocity.x > 0 || rb.angularVelocity.z > 0)
                {
                    rb.angularVelocity += new Vector3(-rb.angularVelocity.x, 0, -rb.angularVelocity.z);
                }*/
                rb.AddForce(-rb.linearVelocity);
            }
            }
    }

    void FixedUpdate()
    {
        RotateCamera();

        if (!isOnBoat)
            MovePlayer();
    }

    void MovePlayer()
    {
        Vector2 moveDirection = playerControls.Land.Move.ReadValue<Vector2>();
        
        //replace everything in the //'s with your own code
        //this is just so we can test even if the ship doesn't work
        //>
        if (Input.GetKey(KeyCode.LeftShift))
        {
            rb.MovePosition(rb.position + rb.transform.forward * moveDirection.y * movementSpeed * sprintMultiplier * Time.deltaTime);
            rb.MovePosition(rb.position + rb.transform.right * moveDirection.x * movementSpeed * sprintMultiplier * Time.deltaTime);
        }
        else
        {
        //<
        rb.MovePosition(rb.position + rb.transform.forward * moveDirection.y * movementSpeed * Time.deltaTime);
        rb.MovePosition(rb.position + rb.transform.right * moveDirection.x * movementSpeed * Time.deltaTime);
        //>
        }
        //<
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

    //set the player to be able to go on top of the boat
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("boat"))
        {
            isOnBoat = true;
            Debug.LogWarning("IT HITS");
            transform.SetParent(boat.transform);
            transform.position = seatPosition.position;

            EntersBoat();
        }
    }

    void EntersBoat()
    {
        Destroy(rb);
        isOnBoat = true;

        buoyancyController.enabled = true;
        boatController.enabled = true;
    }

    public void ExitBoat()
    {
        gameObject.AddComponent(typeof(Rigidbody));
        rb = GetComponent<Rigidbody>();
        isOnBoat = false;

        buoyancyController.enabled = false;
        boatController.enabled = false;
    }

    void Jump()
    {
        if (isGrounded)
        {
            if (playerControls.Land.Jump.triggered)
            {
                print("Jumped");
                rb.AddForce(new Vector3(0, jumpStrength, 0));
            }
        }
    }

    void IsGrounded()
    {
        /*RaycastHit hit;
        if(Physics.Raycast(transform.position, -Vector3.up, out hit, gameObject.GetComponent<SphereCollider>().bounds.extents.y + 0.1f))
        {
            print(hit);
            return true;
        }*/
        isGrounded = false;

        for (int i = 0; i < groundChecks.Length; i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(groundChecks[i].position, -Vector3.up, out hit, gameObject.GetComponent<CapsuleCollider>().bounds.extents.y + 0.1f))
            {
                //print(hit);
                isGrounded = true;
                return;
            }
        }
        isGrounded = false;
    }
}
