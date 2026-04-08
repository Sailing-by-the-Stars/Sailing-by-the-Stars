using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public class Page
{
    public Texture leftPage;
    public Texture rightPage;
}

public class BookPageSwitcher : MonoBehaviour
{
    RawImage leftPage;
    RawImage rightPage;

    [SerializeField]
    List<Page> pages = new();
    [SerializeField]
    int pageNr = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        List<RawImage> images = GetComponentsInChildren<RawImage>().ToList();

        foreach (RawImage image in images)
        {
            if (image.name == "leftPage")
            {
                leftPage = image;
            }

            if (image.name == "rightPage")
            {
                rightPage = image;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            pageNr += 1;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            pageNr -= 1;
        }

        if (pageNr < 0)
        {
            pageNr = 0;
        }
        if (pageNr > pages.Count - 1)
        {
            pageNr = pages.Count - 1;
        }


        if (pages[pageNr].leftPage)
        {
            leftPage.enabled = true;
            leftPage.texture = pages[pageNr].leftPage;
        }
        else
        {
            leftPage.enabled = false;
        }


        if (pages[pageNr].rightPage)
        {
            rightPage.enabled = true;
            rightPage.texture = pages[pageNr].rightPage;
        }
        else
        {
            rightPage.enabled = false;
        }
    }
}
