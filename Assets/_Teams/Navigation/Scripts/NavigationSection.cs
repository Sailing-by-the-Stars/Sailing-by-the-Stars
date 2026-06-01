using System;
using System.Collections.Generic;
using UnityEngine;

public class NavigationSection : JournalSection
{
    public List<NavPage> navPages = new();

    public static Action<List<int>> openedNavPage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        if(navPages.Count >= pages.Count)
        {
            pages.Clear();
            foreach (var navPage in navPages)
            {
                Page page = navPage.page;
                pages.Add(page);
            }
        }
        else
        {
            navPages.Clear();
            foreach (var page in pages)
            {
                NavPage navPage = new NavPage();
                navPage.page = page;
                navPages.Add(navPage);
            }
        }


        if(navPages.Count != pages.Count)
        {
            Debug.LogError("initializing the initial navPages went wrong somehow!!");
        } 
    }

    public override void OpenPage(int pageNr)
    {
        base.OpenPage(pageNr);

        if (navPages[pageNr] == null || navPages[pageNr].OpenedPages == null)
        {
            return;
        }

        openedNavPage?.Invoke(navPages[pageNr].OpenedPages);

        /*
        string result = "opened NavPages: ";
        foreach (var item in navPages[pageNr].OpenedPages)
        {
            result += item.ToString() + ", ";
        }
        Debug.Log(result);
        */
    }

    public void AddPage(NavPage navPage, int pageID)
    {
        if (pageID == -1)
        {
            pages.Add(navPage.page);
            navPages.Add(navPage);
            nrOfPages = pages.Count;
            return;
        }

        if (pageID < -1)
        {
            Debug.LogError($"Invalid page index: {pageID}");
            return;
        }

        int extraPages = 0;
        while (pages.Count <= pageID)
        {
            pages.Add(null);
            navPages.Add(null);
            extraPages++;
        }

        if (navPages[pageID] != null)
        {
            Debug.LogWarning($"Overwriting page at index {pageID}");
            foreach (int i in navPages[pageID].OpenedPages)
            {
                if (!navPage.OpenedPages.Contains(i))
                {
                    navPage.OpenedPages.Add(i);
                }
            }
        }
        
        OpenSection();

        pages[pageID] = navPage.page;
        navPages[pageID] = navPage;

        navPage.page.pageID = pageID;

        nrOfPages = pages.Count;

        int newPageNr = parentJournal.getFullPageNR(navPage.page);

        if (parentJournal.curPageNr > newPageNr - extraPages)
        {
            parentJournal.curPageNr += extraPages;
            parentJournal.prevPageNumber = parentJournal.curPageNr;
        }
        
        if (parentJournal.curPageNr >= newPageNr)
        {
            //Jump back to the new page
            parentJournal.curPageNr -= parentJournal.curPageNr - newPageNr;
            parentJournal.prevPageNumber = parentJournal.curPageNr - 1; 
        } 
        else if (parentJournal.curPageNr < newPageNr)
        {
            //Jump forward to the new page
            parentJournal.curPageNr += parentJournal.curPageNr + newPageNr;
            parentJournal.prevPageNumber = parentJournal.curPageNr - 1; 
        }
    }
}
