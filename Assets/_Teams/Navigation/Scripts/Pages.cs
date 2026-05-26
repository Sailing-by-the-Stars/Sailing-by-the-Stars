using UnityEngine;

public class Pages : JournalPagePickup
{
    [SerializeField]
    SectionName sectionName;
    [SerializeField]
    Page page;

    public override void Grab(PickupController pickupController)
    {
        if (leftOnly)
        {
            page.rightPage = null;
        }

        base.Grab(pickupController);

        if (AddToJournal(sectionName, page))
        {
            Destroy(gameObject);
        }
    }
}
