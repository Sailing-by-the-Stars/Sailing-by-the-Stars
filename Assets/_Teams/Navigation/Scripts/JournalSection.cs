using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public abstract class JournalSection : MonoBehaviour
{
    public Journal parentJournal;

    public List<Page> pages = new();
    
    public int nrOfPages;
    
    public SectionName sectionName;
    public int currentPageNr = 0;
    [HideInInspector]
    public int firstPageNr = 0;

    [HideInInspector]
    public int lastPageNr = 0;


    public virtual void OpenSection(bool maxPage = false)
    {
        parentJournal.OpenBookmark(sectionName);
    }

    public virtual void CloseSection()
    {
        
    }

    public virtual void OpenPage(int pageNr)
    {
        if (pageNr > pages.Count - 1)
        {
            Debug.LogError($"tried opening a page that is out of range for this section! {pageNr}");
            parentJournal.leftPageQ.enabled = false;
            parentJournal.rightPageQ.enabled = false;
            return;
        }

        if (pages[pageNr] == null)
        {
            //Debug.LogWarning("tried opening empty page!");
            parentJournal.leftPageQ.enabled = false;
            parentJournal.rightPageQ.enabled = false;
            return;
        }

        if (pages[pageNr].leftPage)
        {
            parentJournal.leftPageQ.enabled = true;

            FixScaling(pages[pageNr].leftPage, parentJournal.leftPageQ.transform);

            parentJournal.leftPageQ.sharedMaterial.mainTexture = pages[pageNr].leftPage;
        }
        else
        {
            parentJournal.leftPageQ.enabled = false;
        }


        if (pages[pageNr].rightPage)
        {
            parentJournal.rightPageQ.enabled = true;

            FixScaling(pages[pageNr].rightPage, parentJournal.rightPageQ.transform);
            parentJournal.rightPageQ.sharedMaterial.mainTexture = pages[pageNr].rightPage;
        }
        else
        {
            parentJournal.rightPageQ.enabled = false;
        }
    }

    public virtual void GetPage()
    {

    }

    public virtual void AddPage(Page page, int pageID)
    {
        if (pageID == -1)
        {
            pages.Add(page);
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
            extraPages++;
        }

        if (pages[pageID] != null)
        {
            Debug.LogWarning($"Overwriting page at index {pageID}");
        }

        pages[pageID] = page;

        page.pageID = pageID;

        nrOfPages = pages.Count;

        if(parentJournal.curPageNr > parentJournal.getFullPageNR(page) - extraPages)
        {
            parentJournal.curPageNr += extraPages;
            parentJournal.prevPageNumber = parentJournal.curPageNr;
        }
    }

    protected virtual void FixScaling(Texture tex, Transform trans)
    {
        if (parentJournal.maxPageWidth == 0f || parentJournal.maxPageHeight == 0f)
        {
            parentJournal.maxPageWidth = trans.localScale.x;
            parentJournal.maxPageHeight = trans.localScale.y;
        }

        float texWidth = tex.width;
        float texHeight = tex.height;

        float texRatio = texWidth / texHeight;
        float maxRatio = parentJournal.maxPageWidth / parentJournal.maxPageHeight;

        float newWidth;
        float newHeight;

        if (texRatio > maxRatio)
        {
            newWidth = parentJournal.maxPageWidth;
            newHeight = parentJournal.maxPageWidth / texRatio;
        }
        else
        {
            newHeight = parentJournal.maxPageHeight;
            newWidth = parentJournal.maxPageHeight * texRatio;
        }

        trans.localScale = new Vector2(newWidth, newHeight);
    }
}
