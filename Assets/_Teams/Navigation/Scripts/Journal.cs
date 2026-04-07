using UnityEngine;

public class Journal : ToolPickup
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private bool isEquipped;
    private bool bookVisible;
    private bool bookOpened;
    private Quaternion initalRot;

    public override void Grab(PickupController pickupController)
    {
        base.Grab(pickupController);

        isEquipped = true;
        
        //Disable collider to hide interact text
        GetComponent<BoxCollider>().enabled = false;
        bookVisible = true;
        bookOpened = true;
        
        initalRot = Quaternion.Euler(90, 90, 90);
        transform.localRotation = initalRot;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (isEquipped)
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                if (bookVisible)
                {
                    GetComponent<Renderer>().enabled = false;
                    bookVisible = false;
                }
                else
                {
                    GetComponent<Renderer>().enabled = true;
                    bookVisible = true;
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (bookOpened)
                {
                    transform.localRotation = initalRot;
                    bookOpened = false;
                }
                else
                {
                    transform.localRotation = Quaternion.Euler(90, 0, 90);
                    bookOpened = true;
                }
            }
        }
    }
}
