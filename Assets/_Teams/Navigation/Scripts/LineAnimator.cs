
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;


[RequireComponent(typeof(LineRenderer))]
public class LineAnimator : MonoBehaviour
{
    //set by the script
    private LineRenderer lineExample;
    List<TwinklingStar> stars = new List<TwinklingStar>();

    //set in the editor
    [SerializeField] private float segmentDuration = 1f;
    [SerializeField] private AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    void Start()
    {
        //get a reference to everything
        //all stars under this constilation
        foreach (Transform transform in transform)
        {
            TwinklingStar star = transform.GetComponent<TwinklingStar>();

            if (star != null)
            {
                stars.Add(star);
            }
        }
        
        
        //the linerenderer
        lineExample = GetComponent<LineRenderer>();
        
        //set initial values
        lineExample.positionCount = 1;
        lineExample.SetPosition(0, stars[0].transform.position);


        StartCoroutine(AnimateLine());
    }

    IEnumerator AnimateLine()
    {
        for (int i = 0; i < stars.Count - 1; i++)
        {
            Vector3 start = stars[i].transform.position;
            Vector3 end = stars[i + 1].transform.position;

            float time = 0;

            lineExample.positionCount = i + 2;
            lineExample.SetPosition(i + 1, start);

            while (time < segmentDuration)
            {
                time += Time.deltaTime;
                float t = time / segmentDuration;

                float curvedT = curve.Evaluate(t);
                Vector3 pos = Vector3.Lerp(start, end, curvedT);

                lineExample.SetPosition(i + 1, pos);

                yield return null;
            }

            lineExample.SetPosition(i + 1, end);
        }
    }
}
