using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

[Serializable]
public class Page
{
    public Texture leftPage;
    public Texture rightPage;
}

public class Journal : ToolPickup
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private bool isEquipped;
    private bool bookVisible;
    private bool bookOpened;
    private Quaternion initalRot;


    RawImage leftPage;
    RawImage rightPage;

    [SerializeField]
    List<Page> pages = new();
    [SerializeField]
    int pageNr = 0;

    float bookangle = 0;
    [SerializeField]
    float angelPerSecond = 0;

    SkinnedMeshRenderer bookRenderer;


    public override void Grab(PickupController pickupController)
    {
        base.Grab(pickupController);

        isEquipped = true;
        
        //Disable collider to hide interact text
        GetComponent<BoxCollider>().enabled = false;
        bookVisible = true;
        bookangle = 100;
        bookOpened = true;
        
        initalRot = Quaternion.Euler(90, 90, 90);
        transform.localRotation = initalRot;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (isEquipped)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                pageNr -= 1;
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                pageNr += 1;
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

                Texture tex = pages[pageNr].leftPage;
                RectTransform rt = leftPage.rectTransform;

                float width = tex.width;
                float height = tex.height;

                if (width > height)
                {
                    float ratio = height / width;
                    rt.sizeDelta = new Vector2(1f, ratio);
                }
                else
                {
                    float ratio = width / height;
                    rt.sizeDelta = new Vector2(ratio, 1f);
                }

                leftPage.texture = tex;
            }
            else
            {
                leftPage.enabled = false;
            }


            if (pages[pageNr].rightPage)
            {
                rightPage.enabled = true;

                Texture tex = pages[pageNr].rightPage;
                RectTransform rt = rightPage.rectTransform;

                float width = tex.width;
                float height = tex.height;

                if (width > height)
                {
                    float ratio = height / width;
                    rt.sizeDelta = new Vector2(1f, ratio);
                }
                else
                {
                    float ratio = width / height;
                    rt.sizeDelta = new Vector2(ratio, 1f);
                }

                rightPage.texture = tex;
            }
            else
            {
                rightPage.enabled = false;
            }


            if (Input.GetKeyDown(KeyCode.J))
            {
                if (bookVisible)
                { 
                    bookVisible = false;
                    bookOpened = false;
                }
                else
                {
                    transform.GetChild(0).gameObject.SetActive(true);
                    bookVisible = true;
                    bookOpened = true;
                    bookangle = 100;
                }
            }

            if (bookOpened)
            {
                bookangle -= angelPerSecond * Time.deltaTime;
                if (bookangle < 0)
                {
                    bookangle = 0;
                }
                else
                {
                    leftPage.enabled = false;
                    rightPage.enabled = false;
                }

            }
            else
            {
                leftPage.enabled = false;
                rightPage.enabled = false;

                bookangle += angelPerSecond * Time.deltaTime;
                if (bookangle > 100)
                {
                    bookangle = 100;
                    transform.GetChild(0).gameObject.SetActive(false);
                }
            }

                bookRenderer.SetBlendShapeWeight(0, bookangle);
        }
        else
        {
            leftPage.enabled = false;
            rightPage.enabled = false;
            bookangle = 100;
            bookRenderer.SetBlendShapeWeight(0, bookangle);
        }
    }

    public void AddPage(Page page)
    {
        pages.Add(page);
    }
    
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

        bookRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        leftPage.enabled = false;
        leftPage.enabled = false;
    }
}
