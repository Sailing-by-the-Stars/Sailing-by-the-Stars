using System;
using UnityEngine;

public class JournalPagePickup : MonoBehaviour, IPickup
{
    protected Journal journal = null;

    [SerializeField]
    protected int pageIDHack = -1;
    [SerializeField]
    protected bool leftOnly = false;
    
    PagePickupSound pickupSound;
    ShowJournalUpdateNotif notif;
    
    public virtual string InteractMessage => "Press E to pickup";
    public virtual GameObject ShouldHighlight(InteractionController interactionController) => gameObject;

    void Awake()
    {
        pickupSound = FindFirstObjectByType<PagePickupSound>();
        notif = FindFirstObjectByType<ShowJournalUpdateNotif>(FindObjectsInactive.Include);
    }
    
    public void Interact(InteractionController interactionController)
    {
        var pickupController = interactionController.GetComponent<PickupController>();
        Grab(pickupController);
    }

    public virtual void Grab(PickupController pickupController)
    {
        if (pickupController.transform
                .Find("Main Camera")
                .Find("journal"))
        {
            journal = pickupController.transform
                .Find("Main Camera")
                .Find("journal")
                .GetComponent<Journal>();
            
            if (pickupSound != null) 
            {
                pickupSound.PlaySound();
            }
            else
            {
                Debug.LogWarning("couldn't find page pickup sound! make sure it's added to the AudioManager");
            }

            if (notif != null)
            {
                notif.Show();
            }
            else
            {
                Debug.LogWarning("couldnt't find page pickup notification! make sure it's added to Canvas[The Big One]");
            }

        }

        if (pickupController.transform
                .Find("Main Camera")
                .Find("journal(Clone)")
                && journal == null)
        {
                journal = pickupController.transform
                .Find("Main Camera")
                .Find("journal(Clone)")
                .GetComponent<Journal>();
                
            if (pickupSound != null) 
            {
                pickupSound.PlaySound();
            }
            else
            {
                Debug.LogWarning("couldn't find page pickup sound! make sure it's added to the AudioManager");
            }

            if (notif != null)
            {
                notif.Show();
            }
            else
            {
                Debug.LogWarning("couldnt't find page pickup notification! make sure it's added to Canvas[The Big One]");
            }
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
        if (journal == null) 
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



        if (page.rightPage != null && pageIDHack == -1)
        {
            foreach(Page tPage in section.pages)
            {
                if (tPage == null)
                {
                    continue;
                }
                if (tPage.leftPage == page.leftPage)
                {
                    pageIDHack = tPage.pageID;
                }
            }

            if(pageIDHack == -1)
            {
                Debug.LogWarning("failed to find corrosponding left page!!");
            }
        }
        
        section.AddPage(page, pageIDHack);
        
        return true;
    }
}
