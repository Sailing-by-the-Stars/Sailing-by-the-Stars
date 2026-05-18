using System;
using Unity.VisualScripting;
using UnityEngine;

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

        if (!pickupController.transform
                .Find("Main Camera")
                .Find("journal(Clone)")
                .IsUnityNull() && journal.IsUnityNull())
        {
                journal = pickupController.transform
                .Find("Main Camera")
                .Find("journal(Clone)")
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
    protected bool AddToJournal(SectionName sectionName, Page page)
    {
        if (journal.IsUnityNull()) 
        {
            Debug.LogWarning("there is no journal to add pages to!");
            return false;
        }

        JournalSection section = journal.GetSection(sectionName);
        
        if (!section)
        {
            Debug.LogError($"couldn't find {sectionName} in the journal to add the page to!");
            return false;
        }
        
        section.AddPage(page, pageIDHack);
        
        return true;
    }
}
