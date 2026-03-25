using UnityEngine;


public class WakeUp : MonoBehaviour
{
    public GameObject topLid; //Declares a variable to hold the reference to the top eyelid game object
    public GameObject bottomLid; //Declares a variable to hold the reference to the bottom eyelid game object


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       topLid = GameObject.Find("Top_Eyelid"); //Finds the top eyelid game object in the scene and assigns it to the variable
       bottomLid = GameObject.Find("Bottom_Eyelid"); //Finds the bottom eyelid game object in the scene and assigns it to the variable
    }   
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) //Checks if the left mouse button is pressed down
        {
            topLid.animation.Play("WakeUpTop"); //Plays the animation on the top eyelid
            bottomLid.animation.Play("WakeUpBottom"); //Plays the animation on the bottom eyelid
        }
    }
}
