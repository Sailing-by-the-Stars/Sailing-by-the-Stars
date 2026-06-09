using System.Collections;
using UnityEngine;

public class WakeUp : MonoBehaviour
{
    private GameObject topLid;
    private GameObject bottomLid;

    void Start()
    {
        topLid    = transform.Find("Top Eyelid").gameObject;
        bottomLid = transform.Find("Bottom Eyelid").gameObject;

        StartCoroutine(PlayWakeUpAnimation());
    }

    /// <summary>
    /// Call this from any script to replay the wake-up blink, e.g. after
    /// the player is teleported back to the start of the Denial Island.
    /// </summary>
    public void Play()
    {
        //StartCoroutine(PlayWakeUpAnimation());
    }

    IEnumerator PlayWakeUpAnimation()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        topLid.GetComponent<Animation>().Play("WakeUpTop");
        bottomLid.GetComponent<Animation>().Play("WakeUpBottom");
    }
}