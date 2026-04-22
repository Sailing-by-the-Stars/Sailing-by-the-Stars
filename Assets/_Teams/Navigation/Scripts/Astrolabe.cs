using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Astrolabe : ToolPickup
{
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>().ToList();
        textBoxes = GetComponentsInChildren<TMP_Text>().ToList();
        
        //Prevent numbers showing on the astrolabe as it lies on the floor
        foreach (TMP_Text text in textBoxes)
        {
            text.enabled = false;
        }
    }

    public override void Grab(PickupController pickupController)
    {
        base.Grab(pickupController);

        GameEvents.ExecOnPickup(this); // Needed for the tutorial popup ui, just make sure to call this when the Astrolable is picked up

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
        visible = true;

        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = true;
        }
        
        foreach (TMP_Text text in textBoxes)
        {
            text.enabled = true;
        }
        
        cam = GetComponentInParent<Camera>();
        
        initialFOV = cam.fieldOfView;
        
        animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        zoomedIn = true;
        if (zoomCoroutine == null)
        {
            zoomCoroutine = StartCoroutine(ZoomIn());
        }
        sideView = false;
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
            }
            else
            {
                forward = transform.parent.forward;
            }

            forward.y = 0f; // remove vertical tilt
            forward.Normalize();

            //
            if (Input.GetMouseButtonDown(0) && visible)
            {
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

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                visible = !visible;
                foreach (Renderer rend in renderers)
                {
                    rend.enabled = visible;
                }

                foreach (TMP_Text text in textBoxes)
                {
                    text.enabled = visible;
                }

                if (visible == false)
                {
                    if (zoomedIn)
                    {
                        zoomedIn = false;
                        if (zoomCoroutine == null)
                        {
                            zoomCoroutine = StartCoroutine(ZoomOut());
                        }
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

                float scroll = Input.mouseScrollDelta.y;

                if (scroll != 0f && !sideView)
                {
                    pointer.Rotate(new Vector3(scroll * 1f, 0, 0));
                }

                //Reset astrolabe rotation
                if (Input.GetKey(KeyCode.Space))
                {
                    pointer.transform.localRotation = Quaternion.Euler(0, 0, 0);
                }
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
