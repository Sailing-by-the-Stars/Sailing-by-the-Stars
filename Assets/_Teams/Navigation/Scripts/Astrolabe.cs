using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Astrolabe : ToolPickup, AstroTutorialStep
{
    PlayerControls playerControls;
    GameState prevState;

    [NonSerialized]
    public TutorialSequence currentSequence;
    [NonSerialized]
    public int currentTutorialStep = -1;
    //[NonSerialized]
    public List<int> completedSteps = new();

    private Camera cam;
    
    private float initialFOV;
    private float zoomFOV = 30;
    private float animationTime = 0.75f;

    private AnimationCurve animationCurve;
    
    public static bool zoomedIn;
    private bool isEquipped;
    private bool sideView;
    private bool visible;

    private List<Renderer> renderers = new();
    private List<TMP_Text> textBoxes = new();

    private Quaternion initialRot = new();

    private Transform pointer;
    [SerializeField]
    float pointerAngle;

    public static float pointerAngleHax;

    private Coroutine zoomCoroutine;
    private Coroutine turnCoroutine;

    [SerializeField]
    int tutorialStep = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        playerControls = TempStateMachine.Instance.PlayerControls;

        renderers = GetComponentsInChildren<Renderer>().ToList();
        textBoxes = GetComponentsInChildren<TMP_Text>().ToList();
        
        //Prevent numbers showing on the astrolabe as it lies on the floor
        foreach (TMP_Text text in textBoxes)
        {
            text.enabled = false;
        }
    }

    public void EnterStep(TutorialSequence sequence)
    {
        if (currentSequence != null && currentSequence != sequence)
        {
            Debug.LogError("this tutorialDialogue object is already in a different sequence!!");
            return;
        }
        currentSequence = sequence;
        currentTutorialStep = sequence.index;

        if (tutorialStep == 0)
        {
            playerControls.Astrolabe.Open.Disable();
            playerControls.Astrolabe.Rotate.Disable();
            playerControls.Astrolabe.ChangeAngle.Disable();
        }
    }

    public void EnableControl(int nr)
    {
        if (nr > tutorialStep)
        {
            tutorialStep = nr;
        }


        if (nr == 1)
        {
            playerControls.Astrolabe.Disable();
            playerControls.Astrolabe.Open.Enable();
        }
        else if (nr == 2)
        {
            playerControls.Astrolabe.Disable();
            playerControls.Astrolabe.Open.Enable();
            playerControls.Astrolabe.Rotate.Enable();
        } else if (nr == 3)
        {
            playerControls.Astrolabe.Disable();
            playerControls.Astrolabe.Open.Enable();
            playerControls.Astrolabe.ChangeAngle.Enable();
        }
        else if (nr == 4)
        {
            playerControls.Astrolabe.Disable();
            playerControls.Astrolabe.Open.Enable();
            playerControls.Astrolabe.Rotate.Enable();
        }
        else if (nr == 5)
        {
            playerControls.Astrolabe.Disable();
            playerControls.Astrolabe.Open.Enable();
        }
        else if (nr == 6)
        {
            playerControls.Astrolabe.Enable();
        }
        else
        {
            playerControls.Astrolabe.Enable();
        }
    }

    public void ExitStep()
    {
        if (!currentSequence)
        {
            Debug.LogError("tried continueing a tutorial while none was assigned!");
            return;
        }

        if (tutorialStep >= 0)
        {
            if (completedSteps.Contains(tutorialStep))
            {
                return;
            }
            else
            {
                completedSteps.Add(tutorialStep);
            }
        }

        currentSequence.FinishStep(currentTutorialStep);
    }

    public override void Grab(PickupController pickupController)
    {
        base.Grab(pickupController);

        //GameEvents.ExecOnPickup(this); // Needed for the tutorial popup ui, just make sure to call this when the Astrolable is picked up

        EnableControl(0);
        TutorialSequence.Instance.NextStep(0);

        isEquipped = true;
        
        //Hide pickup prompt after grabbing
        GetComponent<BoxCollider>().enabled = false;
        
        initialRot = transform.localRotation;
        
        if(pointer == null)
        {
            foreach (Transform child in transform)
            {
                if (child.name == "Pointer")
                {
                    pointer = child;
                }
            }
        }
        
        //Astrolabe should be visible when picked up
        visible = false;

        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }
        
        foreach (TMP_Text text in textBoxes)
        {
            text.enabled = false;
        }
        
        cam = GetComponentInParent<Camera>();
        
        initialFOV = cam.fieldOfView;
        
        animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        zoomedIn = false;
        if (zoomCoroutine == null)
        {
            zoomCoroutine = StartCoroutine(ZoomOut(animationTime - Time.deltaTime));
        }
        sideView = true;
    }

    // Update is called once per frame
    void Update()
    {
        //Vector3 parentEuler = transform.parent.eulerAngles;
        //transform.rotation = Quaternion.Euler(parentEuler.x, parentEuler.y, 0f);

        //transform.rotation = transform.parent.rotation * initialRot;

        if (isEquipped)
        {
            Vector3 forward;

            //Flip the astrolabe to the right
            if (sideView)
            {
                forward = transform.parent.right;
                if(tutorialStep >= 2 && playerControls.Astrolabe.ChangeAngle.enabled)
                {
                    playerControls.Astrolabe.Rotate.Enable();
                }
            }
            else
            {
                forward = transform.parent.forward;

                if (tutorialStep == 3 && playerControls.Astrolabe.ChangeAngle.enabled)
                {
                    playerControls.Astrolabe.Rotate.Disable();
                }
            }

            forward.y = 0f; // remove vertical tilt
            forward.Normalize();

            //
            if (playerControls.Astrolabe.Rotate.triggered && visible)
            {
                if (currentSequence != null && (tutorialStep == 2 || tutorialStep == 4))
                {
                    ExitStep();
                }

                if (zoomedIn)
                {
                    if (sideView)
                    {
                        sideView = false;
                        if (turnCoroutine == null)
                        {
                            turnCoroutine = StartCoroutine(TurnAstrolabeRight());
                        }
                    }
                    else
                    {
                        sideView = true;
                        if (turnCoroutine == null)
                        {
                            turnCoroutine = StartCoroutine(TurnAstrolabeLeft());
                        }
                    }
                }
            }

            if (turnCoroutine == null)
            {
                transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
            }

            /*if (Input.GetMouseButtonDown(1) && visible)
            {
                if (zoomedIn)
                {
                    zoomedIn = false;
                    if (zoomCoroutine == null)
                    {
                        zoomCoroutine = StartCoroutine(ZoomOut());
                    }
                }
                else
                {
                    zoomedIn = true;
                    if (zoomCoroutine == null)
                    {
                        zoomCoroutine = StartCoroutine(ZoomIn());
                    }
                }
            }*/

            if (playerControls.Astrolabe.Open.triggered)
            { 
                if (currentSequence != null && tutorialStep == 1)
                {
                    ExitStep();
                }

                visible = !visible;

                if (visible == false)
                {
                    foreach (Renderer rend in renderers)
                    {
                        rend.enabled = visible;
                    }

                    foreach (TMP_Text text in textBoxes)
                    {
                        text.enabled = visible;
                    }


                    if (zoomedIn)
                    {
                        zoomedIn = false;
                        if (zoomCoroutine == null)
                        {
                            zoomCoroutine = StartCoroutine(ZoomOut());
                        }
                    }

                    TempStateMachine.Instance.SetState(prevState);

                }
                else
                {
                    if (TempStateMachine.Instance.gameState == GameState.Journal)
                    {
                        visible = !visible;
                        return;
                    }

                    foreach (Renderer rend in renderers)
                    {
                        rend.enabled = visible;
                    }

                    foreach (TMP_Text text in textBoxes)
                    {
                        text.enabled = visible;
                    }


                    prevState = TempStateMachine.Instance.gameState;
                    TempStateMachine.Instance.SetState(GameState.Astrolabe);
                    EnableControl(tutorialStep);
                    zoomedIn = true;
                    if (zoomCoroutine == null)
                    {
                        zoomCoroutine = StartCoroutine(ZoomIn());
                    }
                }
            }


            pointerAngle = pointer.transform.localRotation.eulerAngles.x;
            pointerAngle = 360 - pointerAngle;

            if (pointerAngle < 0)
            {
                pointerAngle = 0;
            }

            if (pointerAngle >= 360)
            {
                pointerAngle = 0;
            }

            pointerAngleHax = Mathf.Round(pointerAngle);

            foreach (TMP_Text text in textBoxes)
            {
                text.text = Mathf.Round(pointerAngle).ToString();
            }

            if (zoomedIn)
            {

                int scroll = (int)playerControls.Astrolabe.ChangeAngle.ReadValue<Vector2>().y;

                if (scroll != 0 && !sideView)
                {
                    // Current rotation
                    float currentX = pointer.localEulerAngles.x;

                    // Convert 0-360 into -180 to 180
                    if (currentX > 180f)
                        currentX -= 360f;

                    // Add scroll input
                    currentX += scroll;

                    // Clamp between -90 and 0
                    currentX = Mathf.Clamp(currentX, -90f, 0f);

                    // Apply rotation
                    pointer.localRotation = Quaternion.Euler(currentX, 0f, 0f);

                    if (currentSequence != null && tutorialStep == 3)
                    {
                        ExitStep();
                    }
                }

                /*
                //Reset astrolabe rotation
                if (Input.GetKey(KeyCode.Space))
                {
                    pointer.transform.localRotation = Quaternion.Euler(0, 0, 0);
                }
                */
            }
        }
    }

    private IEnumerator ZoomIn(float timer = 0)
    {
        if (timer == 0)
        {
            timer = animationTime;
        }

        while (timer > 0)
        {
            timer -= Time.deltaTime;

            if (zoomedIn == false)
            {
                zoomCoroutine = StartCoroutine(ZoomOut(timer));
                yield break;
            }

            float T =  timer / animationTime;
            float curveOutput = animationCurve.Evaluate(T);
            
            cam.fieldOfView = zoomFOV + curveOutput * (initialFOV - zoomFOV);
            
            yield return new WaitForEndOfFrame();
        }

        zoomCoroutine = null;
    }
    
    private IEnumerator ZoomOut(float timer = 0)
    {
        if (timer == 0)
        {
            timer = 0;
        }

        while (timer < animationTime)
        {
            timer += Time.deltaTime;

            if(zoomedIn == true)
            {
                zoomCoroutine = StartCoroutine(ZoomIn(timer));
                yield break;
            }
            
            float T =  timer / animationTime;
            float curveOutput = animationCurve.Evaluate(T);
            
            cam.fieldOfView = zoomFOV + curveOutput * (initialFOV - zoomFOV);
            
            yield return new WaitForEndOfFrame();
        }

        zoomCoroutine = null;
    }
    
    private IEnumerator TurnAstrolabeLeft(float timer = 0)
    {
        if (timer == 0)
        {
            timer = 0;
        }
        
        while (timer < animationTime)
        {
            timer += Time.deltaTime;

            if(sideView == false)
            {
                turnCoroutine = StartCoroutine(TurnAstrolabeRight(timer));
                yield break;
            }
            
            float T =  timer / animationTime;
            float curveOutput = animationCurve.Evaluate(T);

            Vector3 forward = transform.parent.forward;
            forward.y = 0f; // remove vertical tilt

            Vector3 right = transform.parent.right;
            right.y = 0f;

            Quaternion forwardLookRotation = Quaternion.LookRotation(forward, Vector3.up);
            Quaternion rightLookRotation = Quaternion.LookRotation(right, Vector3.up);
            
            transform.rotation = Quaternion.Slerp(forwardLookRotation, rightLookRotation, curveOutput);
            
            yield return new WaitForEndOfFrame();
        }

        turnCoroutine = null;
    }
    
    private IEnumerator TurnAstrolabeRight(float timer = 0)
    {
        if (timer == 0)
        {
            timer = animationTime;
        }
        
        while (timer > 0)
        {
            timer -= Time.deltaTime;

            if(sideView == true)
            {
                turnCoroutine = StartCoroutine(TurnAstrolabeLeft(timer));
                yield break;
            }
            
            float T =  timer / animationTime;
            float curveOutput = animationCurve.Evaluate(T);

            Vector3 forward = transform.parent.forward;
            forward.y = 0f; // remove vertical tilt

            Vector3 right = transform.parent.right;
            right.y = 0f;
            
            Quaternion forwardLookRotation = Quaternion.LookRotation(forward, Vector3.up);
            Quaternion rightLookRotation = Quaternion.LookRotation(right, Vector3.up);
            
            transform.rotation = Quaternion.Slerp(forwardLookRotation, rightLookRotation, curveOutput);
            
            yield return new WaitForEndOfFrame();
        }

        turnCoroutine = null;
    }
}
