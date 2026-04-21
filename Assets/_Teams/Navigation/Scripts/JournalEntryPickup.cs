using UnityEngine;

public class JournalEntryPickup : PagePickup
{
    Page page = new();

    [SerializeField] Texture leftPage;
    [SerializeField] Texture rightPage;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        page.leftPage = leftPage;
        page.rightPage = rightPage;
    }
    
    public override void Grab(PickupController pickupController)
    {
        base.Grab(pickupController);
        
        //Destroy the pickup if the player has a journal
        if (AddToJournal(page))
        {
            Destroy(gameObject);
        }
    }
}
