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
        if (navPages.Count >= pages.Count)
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


        if (navPages.Count != pages.Count)
        {
            Debug.LogError("initializing the initial navPages went wrong somehow!!");
        }
    }

    public override void OpenPage(int pageNr)
    {
        base.OpenPage(pageNr);

        if (navPages[pageNr] == null || navPages[pageNr].OpenedPages == null || !TutorialSequence.Instance.finishedTutorial)
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

    /// <summary>
    /// Adds a page to the section at the given pageID slot.
    /// Pass pageID = -1 to append to the end.
    /// Navigation is left to the caller — use parentJournal.GoToPage() afterwards if needed.
    /// </summary>
    public void AddPage(NavPage navPage, int pageID)
    {
        if (pageID < -1)
        {
            Debug.LogError($"AddPage: invalid pageID {pageID}");
            return;
        }

        if (pageID == -1)
        {
            // Append to end
            navPage.page.pageID = pages.Count;
            pages.Add(navPage.page);
            navPages.Add(navPage);
        }
        else
        {
            // Pad with nulls up to the requested slot
            while (pages.Count <= pageID)
            {
                pages.Add(null);
                navPages.Add(null);
            }

            // Merge OpenedPages if overwriting an existing entry
            if (navPages[pageID] != null)
            {
                Debug.LogWarning($"AddPage: overwriting page at index {pageID}");
                foreach (int id in navPages[pageID].OpenedPages)
                {
                    if (!navPage.OpenedPages.Contains(id))
                        navPage.OpenedPages.Add(id);
                }
            }

            navPage.page.pageID = pageID;
            pages[pageID] = navPage.page;
            navPages[pageID] = navPage;
        }

        nrOfPages = pages.Count;
    }


    protected override void FixScaling(Texture tex, Transform trans)
    {
        if (parentJournal.maxPageWidth == 0f || parentJournal.maxPageHeight == 0f)
        {
            parentJournal.maxPageWidth = trans.localScale.x;
            parentJournal.maxPageHeight = trans.localScale.y;
        }

        float tempWidth = parentJournal.maxPageWidth;

        float texWidth = tex.width;
        float texHeight = tex.height;

        float texRatio = texWidth / texHeight;
        float maxRatio = tempWidth / parentJournal.maxPageHeight;

        float newWidth;
        float newHeight;

        if (texRatio > maxRatio)
        {
            newWidth = tempWidth;
            newHeight = tempWidth / texRatio;
        }
        else
        {
            newHeight = parentJournal.maxPageHeight;
            newWidth = parentJournal.maxPageHeight * texRatio;
        }

        trans.localScale = new Vector2(newWidth, newHeight);
    }
}