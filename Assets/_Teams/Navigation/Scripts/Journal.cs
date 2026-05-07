using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using static UnityEngine.Rendering.DebugUI;

[Serializable]
public class Page
{
    public Texture leftPage;
    public Texture rightPage;
}

public class Journal : ToolPickup, AstroTutorialStep
{
    [NonSerialized]
    public TutorialSequence currentSequence;
    [NonSerialized]
    public int currentTutorialStep = -1;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private bool isEquipped;
    private bool bookVisible;
    private bool bookOpened;
    private Quaternion initalRot;

    MeshRenderer leftPageQ;
    MeshRenderer rightPageQ;


    private float maxPageWidth = 0f;
    private float maxPageHeight = 0f;

    [SerializeField]
    List<Page> pages = new();
    [SerializeField]
    int pageNr = 0;
    float bookangle = 0;
    [SerializeField]
    float angelPerSecond = 0;
    
    [SerializeField]
    int sectionOnePageNr;
    
    [SerializeField]
    int sectionTwoPageNr;
    
    [SerializeField]
    int sectionThreePageNr;
    
    [SerializeField]
    int sectionFourPageNr;

    [SerializeField]
    int sectionFivePageNr;
    
    [SerializeField]
    int sectionSixPageNr;
    
    [SerializeField]
    int sectionSevenPageNr;
    
    [SerializeField]
    int sectionEightPageNr;

    [SerializeField]
    int sectionNinePageNr;
    
    SkinnedMeshRenderer bookRenderer;

    private Transform bookmarkTransform;
    List<int> sectionPageNrs = new List<int>();

    public override void Grab(PickupController pickupController)
    {
        base.Grab(pickupController);

        isEquipped = true;
        
        //Disable collider to hide interact text
        GetComponent<BoxCollider>().enabled = false;
        bookVisible = false;
        bookangle = 101;
        bookOpened = false;

        transform.GetChild(1).gameObject.SetActive(false);

        initalRot = Quaternion.Euler(90, 90, 90);
        transform.localRotation = initalRot;
    }

    public void EnterStep(TutorialSequence sequence)
    {
        if (currentSequence != null && currentSequence != sequence)
        {
            Debug.LogError("this tutorialDialogue object is already in a different sequence!!");
            return;
        }
        currentSequence = sequence;
        currentTutorialStep = sequence.index;
    }

    public void ExitStep()
    {
        if (!currentSequence)
        {
            Debug.LogError("tried continueing a tutorial while none was assigned!");
            return;
        }

        currentSequence.FinishStep(currentTutorialStep);
        currentSequence = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (isEquipped)
        {
            if (Input.GetKeyDown(KeyCode.Q) && bookVisible && bookOpened)
            {
                pageNr -= 1;
            }

            if (Input.GetKeyDown(KeyCode.E) && bookVisible && bookOpened)
            {
                pageNr += 1;
            }

            if (bookOpened)
            {
                //Jump to the first page of the section mapped to that key
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    if (sectionPageNrs[0] <= pages.Count - 1)
                    {
                        pageNr = sectionOnePageNr;
                        
                        for (int i = 0; i < bookmarkTransform.childCount; i++)
                        {
                            bookmarkTransform.GetChild(i).GetComponent<Renderer>().material.color = Color.white;
                        }
                        
                        bookmarkTransform.GetChild(0).GetComponent<Renderer>().material.color = Color.green;
                    }
                }

                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    if (sectionPageNrs[1] <= pages.Count - 1)
                    {
                        pageNr = sectionTwoPageNr;
                        
                        for (int i = 0; i < bookmarkTransform.childCount; i++)
                        {
                            bookmarkTransform.GetChild(i).GetComponent<Renderer>().material.color = Color.white;
                        }
                        
                        bookmarkTransform.GetChild(1).GetComponent<Renderer>().material.color = Color.green;
                    }
                }
                
                if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    if (sectionTwoPageNr <= pages.Count - 1)
                    {
                        pageNr = sectionThreePageNr;
                        
                        for (int i = 0; i < bookmarkTransform.childCount; i++)
                        {
                            bookmarkTransform.GetChild(i).GetComponent<Renderer>().material.color = Color.white;
                        }
                        
                        bookmarkTransform.GetChild(2).GetComponent<Renderer>().material.color = Color.green;
                    }
                }

                /*
                if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    if (sectionFourPageNr <= pages.Count - 1)
                    {
                        pageNr = sectionFourPageNr;
                        
                        for (int i = 0; i < bookmarkTransform.childCount; i++)
                        {
                            bookmarkTransform.GetChild(i).GetComponent<Renderer>().material.color = Color.white;
                        }
                        
                        bookmarkTransform.GetChild(3).GetComponent<Renderer>().material.color = Color.green;
                    }
                }

                if (Input.GetKeyDown(KeyCode.Alpha5))
                {
                    if (sectionFivePageNr <= pages.Count - 1)
                    {
                        pageNr = sectionFivePageNr;
                        
                        for (int i = 0; i < bookmarkTransform.childCount; i++)
                        {
                            bookmarkTransform.GetChild(i).GetComponent<Renderer>().material.color = Color.white;
                        }
                        
                        bookmarkTransform.GetChild(4).GetComponent<Renderer>().material.color = Color.green;
                    }
                }

                if (Input.GetKeyDown(KeyCode.Alpha6))
                {
                    if (sectionSixPageNr <= pages.Count - 1)
                    {
                        pageNr = sectionSixPageNr;
                        
                        for (int i = 0; i < bookmarkTransform.childCount; i++)
                        {
                            bookmarkTransform.GetChild(i).GetComponent<Renderer>().material.color = Color.white;
                        }
                        
                        bookmarkTransform.GetChild(5).GetComponent<Renderer>().material.color = Color.green;
                    }
                }

                if (Input.GetKeyDown(KeyCode.Alpha7))
                {
                    if (sectionSevenPageNr <= pages.Count - 1)
                    {
                        pageNr = sectionSevenPageNr;
                        
                        for (int i = 0; i < bookmarkTransform.childCount; i++)
                        {
                            bookmarkTransform.GetChild(i).GetComponent<Renderer>().material.color = Color.white;
                        }
                        
                        bookmarkTransform.GetChild(6).GetComponent<Renderer>().material.color = Color.green;
                    }
                }

                if (Input.GetKeyDown(KeyCode.Alpha8))
                {
                    if (sectionEightPageNr <= pages.Count - 1)
                    {
                        pageNr = sectionEightPageNr;
                        
                        for (int i = 0; i < bookmarkTransform.childCount; i++)
                        {
                            bookmarkTransform.GetChild(i).GetComponent<Renderer>().material.color = Color.white;
                        }
                        
                        bookmarkTransform.GetChild(7).GetComponent<Renderer>().material.color = Color.green;
                    }
                }

                if (Input.GetKeyDown(KeyCode.Alpha9))
                {
                    if (sectionNinePageNr <= pages.Count - 1)
                    {
                        pageNr = sectionNinePageNr;
                        
                        for (int i = 0; i < bookmarkTransform.childCount; i++)
                        {
                            bookmarkTransform.GetChild(i).GetComponent<Renderer>().material.color = Color.white;
                        }
                        
                        bookmarkTransform.GetChild(8).GetComponent<Renderer>().material.color = Color.green;
                    }
                }
                */
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
                leftPageQ.enabled = true;

                FixScaling(pages[pageNr].leftPage, leftPageQ.transform);

                leftPageQ.sharedMaterial.mainTexture = pages[pageNr].leftPage;
            }
            else
            {
                leftPageQ.enabled = false;
            }


            if (pages[pageNr].rightPage)
            {
                rightPageQ.enabled = true;

                FixScaling(pages[pageNr].rightPage, rightPageQ.transform);
                rightPageQ.sharedMaterial.mainTexture = pages[pageNr].rightPage;
            }
            else
            {
                rightPageQ.enabled = false;
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
                    transform.GetChild(1).gameObject.SetActive(true);
                    bookVisible = true;
                    bookOpened = true;
                    bookangle = 100;

                    if(TutorialSequence.startedTutorial && TutorialSequence.Instance.index == 0)
                    {
                        TutorialSequence.Instance.NextStep(1);
                    }
                }
            }

            if (bookOpened)
            {
                bookangle -= angelPerSecond * Time.deltaTime;
                if (bookangle < 0)
                {
                    bookangle = 0;

                    //Show bookmarks
                    for (int i = 0; i < bookmarkTransform.childCount; i++)
                    {
                        bookmarkTransform.GetChild(i).gameObject.SetActive(true);
                    }
                }
                else
                {
                    leftPageQ.enabled = false;
                    rightPageQ.enabled = false;
                }

            }
            else
            {
                leftPageQ.enabled = false;
                rightPageQ.enabled = false;

                bookangle += angelPerSecond * Time.deltaTime;
                if (bookangle > 100)
                {
                    bookangle = 100;
                    transform.GetChild(1).gameObject.SetActive(false);
                }

                //Hide bookmarks and reset section selection
                for (int i = 0; i < bookmarkTransform.childCount; i++)
                {
                    bookmarkTransform.GetChild(i).gameObject.SetActive(false);
                    bookmarkTransform.GetChild(i).GetComponent<Renderer>().material.color = Color.white;
                }
            }
        }
        else
        {
            leftPageQ.enabled = false;
            rightPageQ.enabled = false;
            bookangle = 100;
        }
    }


    void FixScaling(Texture tex, Transform trans)
    {
        if (maxPageWidth == 0f || maxPageHeight == 0f)
        {
            maxPageWidth = trans.localScale.x;
            maxPageHeight = trans.localScale.y;
        }

        float texWidth = tex.width;
        float texHeight = tex.height;

        float texRatio = texWidth / texHeight;
        float maxRatio = maxPageWidth / maxPageHeight;

        float newWidth;
        float newHeight;

        if (texRatio > maxRatio)
        {
            newWidth = maxPageWidth;
            newHeight = maxPageWidth / texRatio;
        }
        else
        {
            newHeight = maxPageHeight;
            newWidth = maxPageHeight * texRatio;
        }

        trans.localScale = new Vector2(newWidth, newHeight);
    }



    public void AddPage(Page page)
    {
        pages.Add(page);
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

    }

    void Start()
    {
        List<MeshRenderer> images = GetComponentsInChildren<MeshRenderer>().ToList();

        foreach (MeshRenderer image in images)
        {
            if (image.name == "leftPage")
            {
                leftPageQ = image;
            }

            if (image.name == "rightPage")
            {
                rightPageQ = image;
            }
        }

        bookRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        
        leftPageQ.enabled = false;
        rightPageQ.enabled = false;

        bookmarkTransform = transform.GetChild(0).GetChild(0).GetChild(2);
        sectionPageNrs.Add(sectionOnePageNr);
        sectionPageNrs.Add(sectionTwoPageNr);
        sectionPageNrs.Add(sectionThreePageNr);
        sectionPageNrs.Add(sectionFourPageNr);
        sectionPageNrs.Add(sectionFivePageNr);
        sectionPageNrs.Add(sectionSixPageNr);
        sectionPageNrs.Add(sectionSevenPageNr);
        sectionPageNrs.Add(sectionEightPageNr);
        sectionPageNrs.Add(sectionNinePageNr);
    }
}
