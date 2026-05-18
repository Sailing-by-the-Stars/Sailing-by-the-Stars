using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public abstract class JournalSection : MonoBehaviour
{
    public Journal parentJournal;

    [SerializeField]
    List<Page> pages = new();
    
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

    public void AddPage(Page page, int pageID)
    {
        if(pageID >= 0 && pages.Count - 1 >= pageID)
        {
            pages[pageID] = page;
        }
        else
        {
            Debug.LogWarning($"had to fall back! {pageID}");
            pages.Add(page);
        }
        
        nrOfPages++;

    }
    void FixScaling(Texture tex, Transform trans)
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
