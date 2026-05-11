using System.Collections;           // ADDED: needed for coroutines
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

//Creator: Joost
//Edited by: Johan
//edited by: Jardi (the sprint working, hacky though it is)
//edited by: Vasilis (particle system on enter and exit boat)
//edited by: Jantina (dock boarding/disembarking via DockPoint)
public class Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float movementSpeed = 4f;
    [SerializeField] float sprintMultiplier;
    [SerializeField] float jumpStrength = 20f;

    [Header("Camera Setting")]
    [SerializeField] float mouseSensitivity = 10f;
    [SerializeField] float minYRotation = -90f;
    [SerializeField] float maxYRotation = 90f;
    bool limitCamMovement = false;

    [Header("Collision Handling")]
    [SerializeField] Rigidbody rb;
    [SerializeField] float InteractionRange = 1.0f;

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

    [Header("Animation")]
    [SerializeField] Animator animator;

    [SerializeField] string playerState = "Land";


    [Header("Particle System")]
    [SerializeField] private ParticleSystem boatFoam;
    [SerializeField] private ParticleSystem sideFoamRight;
    [SerializeField] private ParticleSystem sideFoamLeft;

    // ADDED BY JANTINA
    [Header("Dock Transition")]
    private bool _transitioning = false;
    private DockPoint[] _docks;
    private DockInteractionUI _dockUI;

    // private void Awake()
    // {
    //     playerControls = new PlayerControls();
    // }

    // private void OnEnable()
    // {
    //     playerControls.Land.Enable();
    //     playerControls.Looking.Enable();
    // }

    // private void OnDisable()
    // {
    //     playerControls.Land.Disable();
    //     playerControls.BoatSail.Disable();
    //     playerControls.BoatRudder.Disable();
    //     playerControls.Looking.Disable();
    // }

    void Start()
    {
        playerControls = TempStateMachine.Instance.PlayerControls;
 
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cam = GetComponentInChildren<Camera>();
        rb = GetComponent<Rigidbody>();
 
        if (boat != null)
        {
            boatController = boat.GetComponent<BoatController>();
            buoyancyController = boat.GetComponent<BuoyancyController>();
        }
 
        // ADDED BY JANTINA
        _docks = FindObjectsByType<DockPoint>(FindObjectsSortMode.None);
        _dockUI = FindFirstObjectByType<DockInteractionUI>();
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

        // ADDED BY JANTINA
        foreach (DockPoint dock in _docks)
            dock.UpdateUI(isOnBoat, transform.position, boat.transform.position);

        if (!_transitioning && playerControls.Land.DockInteract.WasPressedThisFrame())
        {
            foreach (DockPoint dock in _docks)
                dock.TryInteract(isOnBoat, transform.position, boat.transform.position);
        }
    }

    void FixedUpdate()
    {
        RotateCamera();

        Interact();
        switch (playerState)
        {
            case "Land":
                if (!isOnBoat)
                    MovePlayer();
                break;
            case "BoatSail":
                MoveSail();
                break;
            case "BoatRudder":
                MoveRudder();
                break;
            case "BoatAnchor":
                MoveAnchor();
                break;
            default:
                print("Wrong player state initialized.");
                break;        
        }
    }

    void Interact()
    {
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, InteractionRange))
        {
            Debug.DrawLine(cam.transform.position, hit.point, Color.red);
            if(hit.collider.gameObject.GetComponent<BoatInteractor>() != null)
            {
                if (playerControls.Land.Interact.IsPressed())
                {
                    SwitchState(hit.collider.gameObject.GetComponent<BoatInteractor>().type);
                    if (hit.collider.gameObject.GetComponent<BoatInteractor>().playerSpot != null)
                    {
                        transform.position = hit.collider.gameObject.GetComponent<BoatInteractor>().playerSpot.position;
                        transform.rotation = hit.collider.gameObject.GetComponent<BoatInteractor>().playerSpot.rotation;
                    }

                    //xCamRotation = hit.collider.gameObject.GetComponent<BoatInteractor>().playerSpot.eulerAngles.x;
                }
            }
            //print(hit.collider.gameObject.name);
        }
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
        Vector2 lookDirection = playerControls.Looking.Look.ReadValue<Vector2>();
        Vector2 cameraMoveDirection = lookDirection * mouseSensitivity * Time.deltaTime;

        if (limitCamMovement != true)
        {
            // Rotating the X rotation
            xCamRotation += cameraMoveDirection.x;
            xCamRotation = FixRotationLimit(xCamRotation);
            gameObject.transform.rotation = Quaternion.Euler(0, xCamRotation, 0);
        }

        // Rotating the Y rotation
        yCamRotation -= cameraMoveDirection.y;
        yCamRotation = Mathf.Clamp(yCamRotation, minYRotation, maxYRotation);
        cam.transform.localRotation = Quaternion.Euler(yCamRotation, 0, 0);
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
    // private void OnTriggerEnter(Collider other)
    // {
    //     if (other.CompareTag("boat"))
    //     {
    //         isOnBoat = true;
    //         Debug.LogWarning("IT HITS");
    //         transform.SetParent(boat.transform);
    //         transform.position = seatPosition.position;

    //         EntersBoat();
    //     }
    // }

    // private void OnCollisionEnter(Collision collision)
    // {
    //     if (collision.gameObject.CompareTag("boat"))
    //     {
    //         isOnBoat = true;
    //         Debug.LogWarning("IT HITS");
    //         transform.SetParent(boat.transform);
    //         transform.position = seatPosition.position;

    //         EntersBoat();
    //     }
    // }

    void EntersBoat()
    {
        if (rb != null) Destroy(rb);
        isOnBoat = true;

        buoyancyController.enabled = true;
        boatController.enabled = true;

        boatFoam.gameObject.SetActive(true);
        sideFoamLeft.gameObject.SetActive(true);
        sideFoamRight.gameObject.SetActive(true);
        boatFoam.Play();
        sideFoamRight.Play();
        sideFoamLeft.Play();
    }

    public void ExitBoat()
    {
        gameObject.AddComponent(typeof(Rigidbody));
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        isOnBoat = false;

        buoyancyController.enabled = false;
        boatController.enabled = false;

        boatFoam.gameObject.SetActive(false);
        sideFoamLeft.gameObject.SetActive(false);
        sideFoamRight.gameObject.SetActive(false);

        boatFoam.Stop();
        sideFoamLeft.Stop();
        sideFoamRight.Stop();

    }

    // ADDED: Everything below up to Jump() is added by Jantina

    public void BoardBoat(Transform boardSpot)
    {
        if (_transitioning) return;
        StartCoroutine(BoardCoroutine(boardSpot));
    }

    public void DisembarkBoat(Transform exitSpot)
    {
        if (_transitioning) return;
        StartCoroutine(DisembarkCoroutine(exitSpot));
    }

    private IEnumerator BoardCoroutine(Transform boardSpot)
    {
        _transitioning = true;
        if (_dockUI != null) yield return StartCoroutine(_dockUI.FadeOut());

        transform.SetParent(boat.transform);
        transform.position = boardSpot.position;
        transform.rotation = boardSpot.rotation;
        EntersBoat();
        yield return null;
        if (_dockUI != null) yield return StartCoroutine(_dockUI.FadeIn());
        _transitioning = false;
    }

    private IEnumerator DisembarkCoroutine(Transform exitSpot)
    {
        _transitioning = true;
        if (boatController != null) boatController.DropAnchor();
        SwitchState("Land");

        if (_dockUI != null) yield return StartCoroutine(_dockUI.FadeOut());

        ExitBoat();
        transform.SetParent(null);
        transform.position = exitSpot.position;
        transform.rotation = exitSpot.rotation;
        yield return null;
        if (_dockUI != null) yield return StartCoroutine(_dockUI.FadeIn());
        _transitioning = false;
    }

    // END ADDED BY JANTINA

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

    void MoveSail()
    {
        Vector2 moveDirection = playerControls.BoatSail.Move.ReadValue<Vector2>();
        if (moveDirection.y < 0)
        {
            boatController.mastAxis.OnNegative();
            // Moving Down
        } else if (moveDirection.y > 0)
        {
            // Moving Up
            boatController.mastAxis.OnPositive();
        }
        else
            boatController.mastAxis.ResetKeys();


        animator.SetFloat("Movement", moveDirection.y);

        if (playerControls.BoatSail.Leave.IsPressed())
        {
            SwitchState("Land");
        }
    }

    void MoveRudder()
    {
        Debug.Log("TEST!!!");
        Vector2 moveDirection = playerControls.BoatRudder.Move.ReadValue<Vector2>();
        if (moveDirection.x < 0)
        {
            // Moving Left
            boatController.rudderAxis.OnNegative();
        }
        else if (moveDirection.x > 0)
        {
            // Moving right
            boatController.rudderAxis.OnPositive();
        }
        else
            boatController.rudderAxis.ResetKeys();

        animator.SetFloat("Movement", moveDirection.x);

        if (playerControls.BoatRudder.Leave.IsPressed())
        {
            SwitchState("Land");
        }
    }

    void MoveAnchor()
    {
        Vector2 moveDirection = playerControls.BoatAnchor.Move.ReadValue<Vector2>();
        if (moveDirection.y < 0)
        {
            // Moving down
            boatController.DropAnchor();
        }
        else if (moveDirection.y > 0)
        {
            // Moving up
            boatController.HaulAnchor();
        }

        if (playerControls.BoatAnchor.Leave.IsPressed())
        {
            SwitchState("Land");
        }
    }

    void SwitchState(string newState)
    {
        switch (newState)
        {
            case "Land":
                playerControls.BoatSail.Disable();
                playerControls.BoatRudder.Disable();
                playerControls.Land.Enable();
                playerControls.BoatAnchor.Disable();

                animator.SetBool("IsRudder", false);
                animator.SetBool("IsSail", false);
                animator.SetBool("IsAnchor", false);

                limitCamMovement = false;

                SetPlayerState(newState);
                break;
            case "BoatSail":
                playerControls.BoatSail.Enable();
                playerControls.BoatRudder.Disable();
                playerControls.Land.Disable();
                playerControls.BoatAnchor.Disable();

                animator.SetBool("IsSail", true);

                limitCamMovement = true;SetPlayerState(newState);

                SetPlayerState(newState);
                break;
            case "BoatRudder":
                playerControls.BoatSail.Disable();
                playerControls.BoatRudder.Enable();
                playerControls.Land.Disable();
                playerControls.BoatAnchor.Disable();

                animator.SetBool("IsRudder", true);

                limitCamMovement = true;

                SetPlayerState(newState);
                break;
            case "BoatAnchor":
                playerControls.BoatSail.Disable();
                playerControls.BoatRudder.Disable();
                playerControls.Land.Disable();
                playerControls.BoatAnchor.Enable();

                animator.SetBool("IsAnchor", true);

                limitCamMovement = true;

                SetPlayerState(newState);
                break;
            default:
                print("Wrong player state initialized.");
                break;
        }
    }

    void SetPlayerState(string state)
    {
        playerState = state;
    }
}