using System.Collections;
using UnityEngine;

public class TwinklingStar : MonoBehaviour
{
    [SerializeField] AnimationCurve twinkleCurve;
    [SerializeField] Vector2 luminosityRange;
    [SerializeField] Vector2 positionRange;


    [SerializeField] bool twinkle = false;

    Coroutine routine;

    Light light;

    [SerializeField] float twinkleTime = 2f;





    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        

        light = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        if (twinkle && routine == null)
        {
            routine = StartCoroutine(Twinkle());
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

            float T = timer / twinkleTime;
        
            float curveOutput = twinkleCurve.Evaluate(T);
            light.intensity = Mathf.Lerp(luminosityRange.x, luminosityRange.y, T);

            Debug.Log(curveOutput);

            transform.position =  new Vector3(transform.position.x, Mathf.Lerp(positionRange.x, positionRange.y, curveOutput), transform.position.z);

            yield return new WaitForEndOfFrame();
        }

        routine = null;
        light.intensity = luminosityRange.x;
        StopCoroutine(routine);
    }
}
