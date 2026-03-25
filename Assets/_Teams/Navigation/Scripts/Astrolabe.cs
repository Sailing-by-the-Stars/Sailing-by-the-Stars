using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Astrolabe : MonoBehaviour
{
    private Camera cam;
    
    private float initialFOV;
    private float zoomFOV = 30;
    private float zoomTime = 0.75f;

    private AnimationCurve zoomCurve;
    
    private bool zoomedIn;
    private bool visible = false;

    private List<Renderer> renderers = new();
    private List<TMP_Text> textBoxes = new();

    private Quaternion initialRot = new();

    private Transform pointer;
    float pointerAngle;

    private Coroutine coroutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
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

        renderers = GetComponentsInChildren<Renderer>().ToList();
        textBoxes = GetComponentsInChildren<TMP_Text>().ToList();

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
        
        zoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        zoomedIn = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        //Vector3 parentEuler = transform.parent.eulerAngles;
        //transform.rotation = Quaternion.Euler(parentEuler.x, parentEuler.y, 0f);

        //transform.rotation = transform.parent.rotation * initialRot;


        Vector3 forward = transform.parent.forward;
        forward.y = 0f; // remove vertical tilt
        forward.Normalize();

        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);



        if (Input.GetMouseButtonDown(1) && visible)
        {
            if (zoomedIn)
            {
                zoomedIn = false;
                if (coroutine == null)
                {
                    coroutine = StartCoroutine(ZoomOut());
                }
            }
            else
            {
                zoomedIn = true;
                if (coroutine == null)
                {
                    coroutine = StartCoroutine(ZoomIn());
                }
            }
        }


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

                if(visible == false)
                {
                    if (zoomedIn)
                    {
                        zoomedIn = false;
                        if (coroutine == null)
                        {
                            coroutine = StartCoroutine(ZoomOut());
                        }
                    }
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

        foreach (TMP_Text text in textBoxes)
        {
            text.text = Mathf.Round(pointerAngle).ToString();
        }

        if (zoomedIn)
        {

            float scroll = Input.mouseScrollDelta.y;

            if (scroll != 0f)
            {
                pointer.Rotate(new Vector3(scroll * 10f, 0, 0) * Time.deltaTime);
            }

            //Reset astrolabe rotation
            if (Input.GetKey(KeyCode.Space))
            {
                pointer.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
        }
    }

    private IEnumerator ZoomIn(float timer = 0)
    {
        if (timer == 0)
        {
            timer = zoomTime;
        }

        while (timer > 0)
        {
            timer -= Time.deltaTime;

            if (zoomedIn == false)
            {
                coroutine = StartCoroutine(ZoomOut(timer));
                yield break;
            }

            float T =  timer / zoomTime;
            float curveOutput = zoomCurve.Evaluate(T);
            
            cam.fieldOfView = zoomFOV + curveOutput * (initialFOV - zoomFOV);
            
            yield return new WaitForEndOfFrame();
        }

        coroutine = null;
    }
    
    private IEnumerator ZoomOut(float timer = 0)
    {
        if (timer == 0)
        {
            timer = 0;
        }

        while (timer < zoomTime)
        {
            timer += Time.deltaTime;

            if(zoomedIn == true)
            {
                coroutine = StartCoroutine(ZoomIn(timer));
                yield break;
            }
            
            float T =  timer / zoomTime;
            float curveOutput = zoomCurve.Evaluate(T);
            
            cam.fieldOfView = zoomFOV + curveOutput * (initialFOV - zoomFOV);
            
            yield return new WaitForEndOfFrame();
        }

        coroutine = null;
    }
}
