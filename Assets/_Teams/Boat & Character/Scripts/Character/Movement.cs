using System.Collections;           // ADDED: needed for coroutines
using Unity.Cinemachine;
using UnityEngine;

//Creator: Joost
//Edited by: Johan
//edited by: Jardi (the sprint working, hacky though it is)
//edited by: Vasilis (particle system on enter and exit boat)
//edited by: Jantina (dock boarding/disembarking via DockPoint)
//edited by: Alonso (ambience music)
public class Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float movementSpeed = 4f;
    [SerializeField] float sprintMultiplier = 1;
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
    //things added by jardi
    //>
    [SerializeField] float groundCheckRadius = 0.3f;
    [SerializeField] float groundCheckDistance = 0.2f;
    RaycastHit groundHit;
    [SerializeField] float maxSlope = 45;
    //<
    bool isGrounded = false;
    bool isOnBoat = false;

    PlayerControls playerControls;

    [SerializeField]
    Transform cam;

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


    [SerializeField] GameObject sailCrank;
    [SerializeField] GameObject anchorCrank;
    [Header("Debug")]
    [SerializeField] private bool overrideSprintMultiplier;
    [SerializeField] private float overriddenSprintMultiplier = 10f;

    TempStateMachine stateMachine;

    // ADDED BY JANTINA
    [Header("Dock Transition")]
    private bool _transitioning = false;
    private DockPoint[] _docks;
    private DockInteractionUI _dockUI;
    private BoatTutorialManager boatTutorialManager;

    //ADDED BY ALONSO
    private SetAmbienceMusicSail ambienceMusic;

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
        stateMachine = TempStateMachine.Instance;
 
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        //cam = GetComponentInChildren<CinemachineCamera>();
        rb = GetComponent<Rigidbody>();
 
        if (boat != null)
        {
            boatController = boat.GetComponent<BoatController>();
            boatTutorialManager = boat.GetComponent<BoatTutorialManager>(); // Added by Jantina
            buoyancyController = boat.GetComponent<BuoyancyController>();
        }
 
        // ADDED BY JANTINA
        _docks = FindObjectsByType<DockPoint>(FindObjectsSortMode.None);
        _dockUI = FindFirstObjectByType<DockInteractionUI>();

        //ADDED BY ALONSO
        ambienceMusic = transform.GetChild(5).GetComponent<SetAmbienceMusicSail>();
    }

    private void Update()
    {
        if (!isOnBoat)
        {
            //Jump();
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
            case "BoatSimple": // Added by Jantina for Simple Boat Movement
                MoveSimpleBoat();
                break;
            default:
                print("Wrong player state initialized.");
                break;        
        }
    }

    void Interact()
    {
        if (boatController != null && boatController.IsSimpleModeEnabled && isOnBoat) return;
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
        //this is all changed, the phasing through the terrain was anoying the shit out of me
        // - Jardi 
        //>
        Vector2 input = playerControls.Land.Move.ReadValue<Vector2>();

        float speed = movementSpeed;

        // Legs have been cut due to jank ~Joost
        if (playerControls.Land.Sprint.IsPressed())
        {
            speed *= sprintMultiplier;
        }

        Vector3 move = transform.forward * input.y + transform.right * input.x;

        if (move.sqrMagnitude > 1f)
        {
            move.Normalize();
        }

        if (isGrounded)
        {
            move = Vector3.ProjectOnPlane(move, groundHit.normal).normalized;
            float slopeAngle =Vector3.Angle(groundHit.normal, Vector3.up);

            if (slopeAngle > maxSlope)
            {
                Vector3 slopeDirection = Vector3.ProjectOnPlane(Vector3.down, groundHit.normal).normalized;
                float dot = Vector3.Dot(move, slopeDirection);

                if (dot < 0)
                {
                    move -= slopeDirection * dot;
                }
            }
        }

        rb.MovePosition(rb.position + move * speed * Time.fixedDeltaTime);
        //<
    }
    // Legs have been cut due to jank ~Joost
    
    public void SetSprintMultiplier(float multiplier)
    {
        sprintMultiplier = multiplier;
    }

    void RotateCamera()
    {
        Vector2 lookDirection = playerControls.Looking.Look.ReadValue<Vector2>();
        Vector2 cameraMoveDirection = lookDirection * mouseSensitivity * Time.fixedDeltaTime;

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

        ambienceMusic?.PlayAmbienceMusic(2f); //ADDED BY ALONSO

        buoyancyController.enabled = true;
        boatController.enabled = true;
        // Added by Jantina
        var boatRb = boatController.GetComponent<Rigidbody>();
        if (boatRb != null)
        {
            boatRb.linearVelocity = Vector3.zero;
            boatRb.angularVelocity = Vector3.zero;
        }

        boatController.SetSimpleThrottleInput(0f);

        if (boatController.IsSimpleModeEnabled)
        {
            SwitchState("BoatSimple");
            boatController.HaulAnchor();
        }
    }

    public void ExitBoat()
    {
        gameObject.AddComponent(typeof(Rigidbody));
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        if (boatController != null)
        {
            var boatRb = boatController.GetComponent<Rigidbody>();
            if (boatRb != null)
            {
                boatRb.linearVelocity = Vector3.zero;
                boatRb.angularVelocity = Vector3.zero;
            }

        ambienceMusic?.StopAmbienceMusic(); //ADDED BY ALONSO

            boatController.DropAnchor();
        }
        boatController.SetSimpleThrottleInput(0f);
        isOnBoat = false;
        boatController.DropAnchor();
        if (boatController != null)
        {
            boatController.HardStop();
        }
        buoyancyController.enabled = false;
        boatController.enabled = false;
    }
    

    // ADDED: Everything below up to Jump() is added by Jantina
    public void TeleportPlayer(Vector3 position)
    {
        if (isOnBoat)
        {
            SwitchState("Land");
            ExitBoat();
            transform.SetParent(null);
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = position;
        }

        transform.position = position;
    }
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
        Debug.Log($"[BoatTutorial] About to call OnPlayerBoarded. _boatTutorialManager={boatTutorialManager}");
        boatTutorialManager?.OnPlayerBoarded();
        yield return null;
        if (_dockUI != null) yield return StartCoroutine(_dockUI.FadeIn());
        _transitioning = false;
    }

    private IEnumerator DisembarkCoroutine(Transform exitSpot)
    {
        _transitioning = true;
        if (boatController != null) boatController.DropAnchor();
        SwitchState("Land");
        boatTutorialManager?.OnPlayerDisembarked();
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


    // Legs have been cut due to jank ~Joost
    /*void Jump()
    {
        if (isGrounded)
        {
            if (playerControls.Land.Jump.triggered)
            {
                //print("Jumped");
                rb.AddForce(new Vector3(0, jumpStrength, 0));
            }
        }
    }*/


    

    void IsGrounded()
    {
        //mostly changed to not phase through the terrain
        //>
        isGrounded = false;

        CapsuleCollider capsule = GetComponent<CapsuleCollider>();

        float castDistance = capsule.bounds.extents.y + groundCheckDistance;

        for (int i = 0; i < groundChecks.Length; i++)
        {
            RaycastHit hit;

            if (Physics.SphereCast(groundChecks[i].position, groundCheckRadius, Vector3.down, out hit, castDistance))
            {
                isGrounded = true;
                groundHit = hit;

                return;
            }
        }
        //<
    }

    void MoveSail()
    {
        sailCrank.GetComponent<Animator>().SetBool("isCranking", false);
        Vector2 moveDirection = playerControls.BoatSail.Move.ReadValue<Vector2>();
        if (moveDirection.y < 0)
        {
            sailCrank.GetComponent<Animator>().SetBool("isReverse", true);
            sailCrank.GetComponent<Animator>().SetBool("isCranking", true);
            boatController.mastAxis.OnNegative();
            // Moving Down

            // Added by Jantina — notify tutorial
            boatTutorialManager?.NotifySailAdjusted();
        } else if (moveDirection.y > 0)
        {
            sailCrank.GetComponent<Animator>().SetBool("isReverse", false);
            sailCrank.GetComponent<Animator>().SetBool("isCranking", true);
            // Moving Up
            boatController.mastAxis.OnPositive();
            // Added by Jantina — notify tutorial
            boatTutorialManager?.NotifySailAdjusted();
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
            // Added by Jantina — notify tutorial
            boatTutorialManager?.NotifyRudderSteeredComplex();
        }
        else if (moveDirection.x > 0)
        {
            // Moving right
            boatController.rudderAxis.OnPositive();
            // Added by Jantina — notify tutorial
            boatTutorialManager?.NotifyRudderSteeredComplex();
        }
        else
        {
            boatController.rudderAxis.ResetKeys();
        }

        animator.SetFloat("Movement", moveDirection.x);

        var rudderPercentage = (boatController.currentRudderAngle + boatController.maxRudderDeflection) / (boatController.maxRudderDeflection * 2);

        animator.SetFloat("RudderAngle", rudderPercentage);

        if (playerControls.BoatRudder.Leave.IsPressed())
        {
            SwitchState("Land");
        }
    }

    void MoveAnchor()
    {
        anchorCrank.GetComponent<Animator>().SetBool("isCranking", false);
        Vector2 moveDirection = playerControls.BoatAnchor.Move.ReadValue<Vector2>();
        if (moveDirection.y < 0)
        {
            anchorCrank.GetComponent<Animator>().SetBool("isReverse", true);
            anchorCrank.GetComponent<Animator>().SetBool("isCranking", true);
            // Moving down
            boatController.DropAnchor();
        }
        else if (moveDirection.y > 0)
        {
            anchorCrank.GetComponent<Animator>().SetBool("isReverse", false);
            anchorCrank.GetComponent<Animator>().SetBool("isCranking", true);
            // Moving up
            boatController.HaulAnchor();
            // Added by Jantina — notify tutorial
            boatTutorialManager?.NotifyAnchorHauled();

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

                stateMachine.SetState(GameState.Moving);

                animator.SetBool("IsRudder", false);
                animator.SetBool("IsSail", false);
                animator.SetBool("IsAnchor", false);

                limitCamMovement = false;
                if (playerState == "BoatAnchor") boatTutorialManager?.NotifyLeftAnchor();
                    else if (playerState == "BoatRudder") boatTutorialManager?.NotifyLeftRudder();
                    else if (playerState == "BoatSail")   boatTutorialManager?.NotifyLeftSail();
                SetPlayerState(newState);
                break;
            case "BoatSail":
                boatTutorialManager?.NotifyEnteredSail(); // Added by Jantina

                stateMachine.SetState(GameState.Sail);

                animator.SetBool("IsSail", true);

                limitCamMovement = true;SetPlayerState(newState);

                SetPlayerState(newState);
                break;
            case "BoatRudder":
                boatTutorialManager?.NotifyEnteredRudder(); // Added by Jantina

                stateMachine.SetState(GameState.Rudder);

                animator.SetBool("IsRudder", true);

                limitCamMovement = true;

                SetPlayerState(newState);
                break;
            case "BoatAnchor":
                boatTutorialManager?.NotifyEnteredAnchor(); // Added by Jantina

                stateMachine.SetState(GameState.Anchor);

                animator.SetBool("IsAnchor", true);

                limitCamMovement = true;

                SetPlayerState(newState);
                break;
            case "BoatSimple":
                stateMachine.SetState(GameState.Sailing); 
                limitCamMovement = false;
                SetPlayerState("BoatSimple");
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


    // Added by Jantina for Simple Boat Movement
    void MoveSimpleBoat()
    {
        Vector2 input = playerControls.Land.Move.ReadValue<Vector2>();

        boatController.SetSimpleTurnInput(input.x);

        if (Mathf.Abs(input.x) > 0.1f)
        {
            boatTutorialManager?.NotifyRudderSteered();
        }
        float throttle =
            input.y > 0.1f ? 1f : 0f;

        boatController.SetSimpleThrottleInput(throttle);

        if (throttle > 0f)
        {
            boatTutorialManager?.NotifyThrottleUsed();
        }
    }
    public void ToggleBoatMode(bool simpleMode)
    {
        boatController.SetSimpleMode(simpleMode);

        if (!simpleMode && playerState == "BoatSimple")
            SwitchState("Land");
    }
}