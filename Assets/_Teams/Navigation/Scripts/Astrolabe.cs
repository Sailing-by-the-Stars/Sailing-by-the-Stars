using System.Collections;
using UnityEngine;

public class Astrolabe : MonoBehaviour
{
    private Camera cam;
    
    private float initialFOV;
    private float zoomFOV = 30;
    private float zoomTime = 0.75f;

    private AnimationCurve zoomCurve;
    
    private bool zoomedIn;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    void Start()
    {
        cam = GetComponentInParent<Camera>();
        
        initialFOV = cam.fieldOfView;
        
        zoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        zoomedIn = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (zoomedIn)
            {
                StartCoroutine(ZoomOut());
                zoomedIn = false;
                transform.rotation = transform.parent.rotation;
            }
            else
            {
                StartCoroutine(ZoomIn());
                zoomedIn = true;
            }
        }

        if (zoomedIn)
        {
            if(Input.GetKey(KeyCode.A))
            {
                transform.Rotate(new Vector3(0, 0, 10) * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.D))
            {
                transform.Rotate(new Vector3(0, 0, -10) * Time.deltaTime);
            }

            //Reset astrolabe rotation
            if (Input.GetKey(KeyCode.Space))
            {
                transform.rotation = transform.parent.rotation;
            }
        }
    }

    private IEnumerator ZoomIn()
    {
        float timer = zoomTime;

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            
            float T =  timer / zoomTime;
            float curveOutput = zoomCurve.Evaluate(T);
            
            cam.fieldOfView = zoomFOV + curveOutput * (initialFOV - zoomFOV);
            
            yield return new WaitForEndOfFrame();
        }
    }
    
    private IEnumerator ZoomOut()
    {
        float timer = 0;

        while (timer < zoomTime)
        {
            timer += Time.deltaTime;
            
            float T =  timer / zoomTime;
            float curveOutput = zoomCurve.Evaluate(T);
            
            cam.fieldOfView = zoomFOV + curveOutput * (initialFOV - zoomFOV);
            
            yield return new WaitForEndOfFrame();
        }
    }
}
