using System;
using Unity.VisualScripting;
using UnityEngine;


/*
 * To create a page pickup
 * - Create a new script and extend the class from this class, PagePickup
 * - Initialize a new Page object
 * - Initialize two SerializeField Textures: leftPage and rightPage
 * - in Start() assign page.leftPage and page.rightPage to the textures
 * initialized above respectively
 * - override the Grab() method and call base.Grab()
 * - In the overridden Grab() method,
 *  Add an if statement checking AddToJournal(page) is true,
 *  and then destroy the object
 * - Add the script to a new object and assign textures in the editor
 * Rever to the DebugPage prefab as an example
 */
public class JournalPagePickup : MonoBehaviour, IPickup
{
    private Journal journal;

    [SerializeField]
    protected int pageIDHack = -1;
    [SerializeField]
    protected bool leftOnly = false;

    public virtual string InteractMessage => "Press E to pickup";

    public void Interact(InteractionController interactionController)
    {
        var pickupController = interactionController.GetComponent<PickupController>();
        Grab(pickupController);
    }

    public virtual void Grab(PickupController pickupController)
    {
        if (!pickupController.transform
                .Find("Main Camera")
                .Find("journal")
                .IsUnityNull())
        {
            journal = pickupController.transform
                .Find("Main Camera")
                .Find("journal")
                .GetComponent<Journal>();
        }
    }

    public void Drop(PickupController pickupController)
    {
    }

    public void SetPositionInParent(Transform newParent)
    {
    }

    public void Use()
    {
    }

    //Add page to the journal if the player has a journal
    protected bool AddToJournal(Page page)
    {
        if (journal.IsUnityNull()) 
        {
            return false;
        }
        
        journal.GetComponent<Journal>().AddPage(page, pageIDHack);
        
        return true;
    }
}
