using System.Collections;
using UnityEngine;


public class WakeUp : MonoBehaviour
{
    private GameObject topLid; //Declares a variable to hold the reference to the top eyelid game object
    private GameObject bottomLid; //Declares a variable to hold the reference to the bottom eyelid game object


    void Start()
    {
        topLid = transform.Find("Top Eyelid").gameObject; //Finds the child game object named "Top Eyelid" and assigns it to the topLid variable
        bottomLid = transform.Find("Bottom Eyelid").gameObject; //Finds the child game object named "Bottom Eyelid" and assigns it to the bottomLid variable

        // TODO: Change this to a more appropriate trigger for the wake up animation, such as a specific event or condition in the game
        //if (Input.GetMouseButtonDown(0)) //Checks if the left mouse button is pressed down
        StartCoroutine(PlayWakeUpAnimation()); // Start after a small delay, just for testing purposes
    }

    IEnumerator PlayWakeUpAnimation()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        topLid.GetComponent<Animation>().Play("WakeUpTop"); //Plays the animation on the top eyelid
        bottomLid.GetComponent<Animation>().Play("WakeUpBottom"); //Plays the animation on the bottom 
    }
}