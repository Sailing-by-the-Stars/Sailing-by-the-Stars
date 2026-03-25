using System.Collections;
using UnityEditor;
using UnityEngine;

public class TwinklingStar : MonoBehaviour
{
    [SerializeField] AnimationCurve twinkleCurve;
    [SerializeField] AnimationCurve dimCurve;

    [SerializeField] Vector2 luminosityRange;
    [SerializeField] Vector2 positionRange;

    [SerializeField] bool twinkle = false;
    [SerializeField] bool dim = false;

    private Coroutine twinkleRoutine;
    private Coroutine dimRoutine;
    private Coroutine brightenRoutine;

    [SerializeField] Light light;

    [SerializeField] float twinkleTime = 2f;
    [SerializeField] float dimTime = 2f;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        light = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        //Start the twinkle animation if twinkling is enabled
        if (twinkle && twinkleRoutine == null)
        {
            twinkleRoutine = StartCoroutine(Twinkle());
        }
    }



    private IEnumerator Twinkle()
    {
        float timer = 0;

        while (twinkle)
        {
            timer += Time.deltaTime;

            if (timer > twinkleTime)
            {
                timer -= twinkleTime;
            }

            //Find current position on the animation curve
            float T = timer / twinkleTime;
        
            float curveOutput = twinkleCurve.Evaluate(T);
            
            //Change the light intensity based on the position on the animation curve
            light.intensity = Mathf.Lerp(luminosityRange.x, luminosityRange.y, T);

            transform.position =  new Vector3(transform.position.x, Mathf.Lerp(positionRange.x, positionRange.y, curveOutput), transform.position.z);

            yield return new WaitForEndOfFrame();
        }

        //Reset the luminosity to the value set in the inspector
        StopCoroutine(twinkleRoutine);
        twinkleRoutine = null;
        light.intensity = luminosityRange.x;
    }

    private IEnumerator Brighten()
    {
        float timer = 0;

        //Play the animation as long as the value of dimTime
        while (timer < dimTime)
        {
            timer += Time.deltaTime;

            //Find position on the animation curve
            float T = timer / dimTime;
            float curveOutput = dimCurve.Evaluate(T);

            //Change the luminosity based on position on the animation curve (changes position for now as a test)
            transform.position = new Vector3(transform.position.x, Mathf.Lerp(positionRange.x, positionRange.y, curveOutput), transform.position.z);
            
            yield return new WaitForEndOfFrame();

        }

        StopCoroutine(brightenRoutine);
        brightenRoutine = null;
    }

    private IEnumerator Dim()
    {
        float timer = dimTime;

        while (timer > 0)
        {
            timer -= Time.deltaTime;

            //Find position on the animation curve
            float T = timer / dimTime;
            float curveOutput = dimCurve.Evaluate(T);

            //Change the luminosity based on position on the animation curve (changes position for now as a test)
            transform.position = new Vector3(transform.position.x, Mathf.Lerp(positionRange.x, positionRange.y, curveOutput), transform.position.z);

            yield return new WaitForEndOfFrame();
        }

        StopCoroutine(dimRoutine);
        dimRoutine = null;
    }

    private void OnValidate()
    {
        //If the value of the dim boolean changes, start the dim or brighten animation
        if (!dim && brightenRoutine == null)
        {
            brightenRoutine = StartCoroutine(Brighten());
        }

        if (dim && dimRoutine == null)
        {
            dimRoutine = StartCoroutine(Dim());
        }
    }
}