using UnityEngine;

public abstract class JournalSection : MonoBehaviour
{
    public Journal parentJournal;

    public int nrOfPages = 1;
    public SectionName sectionName;
    public int currentPageNr = 1;


    public virtual void OpenSection(bool maxPage = false)
    {

    }

    public virtual void CloseSection()
    {

    }

    public virtual void OpenPage (int pageNr)
    {

    }

    public virtual void GetPage()
    {

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
