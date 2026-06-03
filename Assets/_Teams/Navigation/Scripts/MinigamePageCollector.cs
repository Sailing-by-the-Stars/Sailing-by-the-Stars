using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(NavigationSection))]
public class MinigamePageCollector : MonoBehaviour
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

        RewardItem.collectedReward += Grab;
    }

    private void OnDestroy()
    {
        RewardItem.collectedReward -= Grab;
    }

    public void Grab(int id)
    {
        if(navPages.Count <= id)
        {
            Debug.LogWarning("tried picking up a page that doens't exist!");
            return;
        }

        if (id < 0)
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

        AddToJournal(navPages[id], id);
    }


    void AddToJournal(NavPage navPage, int pageId)
    {
        if (navSection == null)
        {
            Debug.LogError("no navsection found?????");
            return;
        }

        navSection.AddPage(navPage, pageId);
    }
}
