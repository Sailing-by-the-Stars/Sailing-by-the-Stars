using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


[RequireComponent(typeof(NavigationSection))]
public class MinigameNavPageCollector : MonoBehaviour
{
    [SerializeField]
    SectionName sectionName = SectionName.Navigation;
    [SerializeField]
    List<NavPage> navPages;

    PagePickupSound pickupSound;
    ShowJournalUpdateNotif notif;

    NavigationSection navSection;

    void Awake()
    {
        pickupSound = FindFirstObjectByType<PagePickupSound>();
        notif = FindFirstObjectByType<ShowJournalUpdateNotif>(FindObjectsInactive.Include);

        navSection = GetComponent<NavigationSection>();

        if (sectionName != SectionName.Navigation)
        {
            Debug.LogError("navigation page has the wrong section related to it! you should probably be using the Pages class instead!!!");
        }

        RewardItem.collectedReward += Collect;
    }

    private void OnDestroy()
    {
        RewardItem.collectedReward -= Collect;
    }

    public void Collect(int pageID)
    {

        int position = -1;
        bool gotoPos = false;
        switch (pageID)
        {
            case 0:
                position = 1;
                break;
            case 1:
                position = 1;
                gotoPos = true;
                break;
            case 2:
                position = 2;
                gotoPos = true;
                break;
            case 3:
                position = 2;
                gotoPos = true;
                break;
            default:
                break;
        }


        if(navPages.Count <= pageID)
        {
            Debug.LogWarning("tried picking up a page that doens't exist!");
            return;
        }

        if (pageID < 0)
        {
            Debug.LogError("id not assigned correctly on last picked up item!");
        }
        if (position < 0)
        {
            Debug.LogError("id not assigned correctly on last picked up item!");
        }

        if (pickupSound != null && notif != null)
        {
            notif.Show();
            pickupSound.PlaySound();
        }
        else
        {
            Debug.LogWarning($"{(pickupSound == null ? "couldn't find page sound" : "")}, {(pickupSound == null ? "couldn't find notification" : "")}!");
        }

        AddToJournal(navPages[pageID], position);

        if (gotoPos)
        {
            navSection.parentJournal.GoToPage(navSection.parentJournal.getFullPageNR(navPages[pageID].page));
        }
    }


    void AddToJournal(NavPage navPage, int position)
    {
        if (navSection == null)
        {
            Debug.LogError("no navsection found?????");
            return;
        }

        navSection.AddPage(navPage, position);
    }
}
