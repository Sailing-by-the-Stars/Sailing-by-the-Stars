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
        bookVisible = false;
        bookangle = 101;
        bookOpened = false;

        transform.GetChild(0).gameObject.SetActive(false);

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
                leftPageQ.enabled = true;

                Texture tex = pages[pageNr].leftPage;
                Transform rt = leftPageQ.transform;

                float width = tex.width;
                float height = tex.height;

                if (width > height)
                {
                    float ratio = height / width;
                    rt.localScale = new Vector2(1f, ratio);
                }
                else
                {
                    float ratio = width / height;
                    rt.localScale = new Vector2(ratio, 1f);
                }

                leftPageQ.sharedMaterial.mainTexture = tex;
            }
            else
            {
                leftPageQ.enabled = false;
            }


            if (pages[pageNr].rightPage)
            {
                rightPageQ.enabled = true;

                Texture tex = pages[pageNr].rightPage;
                Transform rt = rightPageQ.transform;

                float width = tex.width;
                float height = tex.height;

                if (width > height)
                {
                    float ratio = height / width;
                    rt.localScale = new Vector2(1f, ratio);
                }
                else
                {
                    float ratio = width / height;
                    rt.localScale = new Vector2(ratio, 1f);
                }

                rightPageQ.sharedMaterial.mainTexture = tex;
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
                    transform.GetChild(0).gameObject.SetActive(true);
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
                    transform.GetChild(0).gameObject.SetActive(false);
                }
            }

                bookRenderer.SetBlendShapeWeight(0, bookangle);
        }
        else
        {
            leftPageQ.enabled = false;
            rightPageQ.enabled = false;
            bookangle = 100;
            bookRenderer.SetBlendShapeWeight(0, bookangle);
        }
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
    }
}
