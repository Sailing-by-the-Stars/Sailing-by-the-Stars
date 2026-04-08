using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

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


            if (Input.GetKeyDown(KeyCode.J))
            {
                if (bookVisible)
                {
                    GetComponent<Renderer>().enabled = false;
                    bookVisible = false;
                }
                else
                {
                    GetComponent<Renderer>().enabled = true;
                    bookVisible = true;
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (bookOpened)
                {
                    //transform.localRotation = initalRot;
                    bookOpened = false;
                }
                else
                {
                    //transform.localRotation = Quaternion.Euler(90, 0, 90);
                    bookOpened = true;
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
                }
            }

                bookRenderer.SetBlendShapeWeight(0, bookangle);
        }
        else
        {
            bookRenderer.SetBlendShapeWeight(0, bookangle);
        }
    }




    
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

        bookRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        leftPage.enabled = false;
        leftPage.enabled = false;
    }
}
