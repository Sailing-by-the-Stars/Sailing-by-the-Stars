using UnityEngine;


public class WakeUp : MonoBehaviour
{
    private GameObject topLid; //Declares a variable to hold the reference to the top eyelid game object
    private GameObject bottomLid; //Declares a variable to hold the reference to the bottom eyelid game object


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        topLid = transform.Find("Top Eyelid").gameObject; //Finds the child game object named "Top Eyelid" and assigns it to the topLid variable
        bottomLid = transform.Find("Bottom Eyelid").gameObject; //Finds the child game object named "Bottom Eyelid" and assigns it to the bottomLid variable
    }   
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) //Checks if the left mouse button is pressed down
        {
            topLid.GetComponent<Animation>().Play("WakeUpTop"); //Plays the animation on the top eyelid
            bottomLid.GetComponent<Animation>().Play("WakeUpBottom"); //Plays the animation on the bottom eyelid
        }
    }
}