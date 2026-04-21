using System;
using Unity.VisualScripting;
using UnityEngine;

public class PagePickup : MonoBehaviour, IPickup
{
    private Journal journal;

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
        
        journal.GetComponent<Journal>().AddPage(page);
        
        return true;
    }
}
