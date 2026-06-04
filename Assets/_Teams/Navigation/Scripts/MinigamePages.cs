using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


[RequireComponent(typeof(NavigationSection))]
public class MinigamePages : MonoBehaviour
{
    [SerializeField]
    SectionName sectionName = SectionName.Lore;
    [SerializeField]
    List<Page> pages;

    LoreSection loreSection;

    void Awake()
    {
        loreSection = GetComponent<LoreSection>();

        if (sectionName != SectionName.Lore)
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


        if (pages.Count <= pageID)
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

        AddToJournal(pages[pageID], position);
    }


    void AddToJournal(Page page, int position)
    {
        if (loreSection == null)
        {
            Debug.LogError("no loresection found?????");
            return;
        }

        loreSection.AddPage(page, position);
    }
}
