using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class NavPage
{
    public Page page;
    public List<int> OpenedPages = new();
}

public class NavigationPages : JournalPagePickup
{
    [SerializeField]
    SectionName sectionName = SectionName.Navigation;
    [SerializeField]
    NavPage navPage;

    private void Start()
    {
        if(sectionName != SectionName.Navigation)
        {
            Debug.LogError("navigation page has the wrong section related to it! you should probably be using the Pages class instead!!!");
        }
    }

    public override void Grab(PickupController pickupController)
    {
        if (leftOnly)
        {
            navPage.page.rightPage = null;
        }

        base.Grab(pickupController);

        if (AddToJournal(sectionName, navPage))
        {
            Destroy(gameObject);
        }
    }


    protected bool AddToJournal(SectionName sectionName, NavPage navPage)
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

        if (section is NavigationSection navSection)
        {
            if (navPage.page.rightPage != null && pageIDHack == -1)
            {
                foreach (Page tPage in section.pages)
                {
                    if (tPage == null)
                    {
                        continue;
                    }
                    if (tPage.leftPage == navPage.page.leftPage)
                    {
                        pageIDHack = tPage.pageID;
                    }
                }

                if (pageIDHack == -1)
                {
                    Debug.LogWarning("failed to find corrosponding left page!!");
                }
            }

            navSection.AddPage(navPage, pageIDHack);

            return true;
        }
        else
        {
            Debug.LogError($"tried adding navigation page to the wrong section! {sectionName}");
            return false;
        }
    }
}
