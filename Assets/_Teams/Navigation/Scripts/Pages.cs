using UnityEngine;

public class Pages : JournalPagePickup
{
    [SerializeField]
    Page page;


    public override void Grab(PickupController pickupController)
    {
        if (leftOnly)
        {
            page.rightPage = null;
        }

        base.Grab(pickupController);

        if (AddToJournal(page))
        {
            Destroy(gameObject);
        }
    }
}
