using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum SectionName
{
    AstrolabeInstructions,
    Navigation,
    Lore,
    Quests,
}


public class Journal : ToolPickup, AstroTutorialStep
{
    PlayerControls playerControls;


    [NonSerialized]
    public TutorialSequence currentSequence;
    [NonSerialized]
    public int currentTutorialStep = -1;

    private bool isEquipped;
    private bool bookVisible;
    private bool bookOpened;


    public float maxPageWidth = 0f;
    public float maxPageHeight = 0f;

    [SerializeField]
    List<JournalSection> Sections = new();

    int pageNr = 1;
    int curSectionIndex = 0;

    List<Bookmark> bookmarks;


    private Camera cam;
    private float initialFOV;
    private float zoomFOV = 35;
    private float animationTime = 0.75f;

    private AnimationCurve animationCurve;
    public static bool zoomedIn;

    private Coroutine zoomCoroutine;


    private void OnEnable()
    {
        playerControls = TempStateMachine.Instance.PlayerControls;
    }

    private void OnDisable()
    {

    }


    public override void Grab(PickupController pickupController)
    {
        base.Grab(pickupController);

        isEquipped = true;

        //Disable collider to hide interact text
        GetComponent<BoxCollider>().enabled = false;
        bookVisible = false;
        bookOpened = false;

        transform.GetChild(1).gameObject.SetActive(false);


        ///animation stuff:
        //bookangle = 101;
        //initalRot = Quaternion.Euler(90, 90, 90);
        //transform.localRotation = initalRot;

        //Set up values for zoom
        cam = GetComponentInParent<Camera>();

        initialFOV = cam.fieldOfView;

        animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        zoomedIn = false;
        if (zoomCoroutine == null)
        {
            zoomCoroutine = StartCoroutine(ZoomOut(animationTime - Time.deltaTime));
        }
    }


    public void OpenBookmark(SectionName sectionName, bool maxPage = false)
    {
        if (bookOpened)
        {
            foreach (Bookmark bookmark in bookmarks)
            {
                if (bookmark.sectionName == sectionName)
                {
                    bookmark.Highlight();
                }
                else
                {
                    bookmark.UnHighlight();
                }
            }

        }

    }


    void Start()
    {
        bookmarks = new();
        foreach (Bookmark bookmark in GetComponentsInChildren<Bookmark>())
        {
            bookmarks.Add(bookmark);
        }

        foreach (JournalSection section in Sections)
        {
            section.parentJournal = this;
        }
    }


    void Update()
    {
        HandlePageTurn();


        if (playerControls.Journal.Zoom.triggered)
        {
            if (zoomedIn)
            {
                zoomedIn = false;
                if (zoomCoroutine == null)
                {

                    zoomCoroutine = StartCoroutine(ZoomOut());
                }
            }
            else
            {
                zoomedIn = true;
                if (zoomCoroutine == null)
                {
                    zoomCoroutine = StartCoroutine(ZoomIn());
                }
            }
        }
    }

    void HandlePageTurn()
    {
        int minSectionNr = 0;
        int maxSectionNr = 0;
        JournalSection curSection = null;
        curSectionIndex = 0;
        int nrOfPages = 0;

        int i = 0;
        foreach (JournalSection section in Sections)
        {
            maxSectionNr += section.nrOfPages;

            if (maxSectionNr > pageNr && maxSectionNr - section.nrOfPages < pageNr)
            {
                curSection = section;
                minSectionNr = maxSectionNr - section.nrOfPages;
                curSectionIndex = i;
            }

            i++;
            nrOfPages += section.nrOfPages;
        }


        if (curSection == null)
        {
            Debug.LogError("the pageNr is somehow out of range!");
            return;
        }


        if (playerControls.Journal.PreviousPage.triggered && bookVisible && bookOpened)
        {
            pageNr -= 1;
        }

        if (playerControls.Journal.NextPage.triggered && bookVisible && bookOpened)
        {
            pageNr += 1;
        }



        if (pageNr < 1)
        {
            pageNr = 1;
        }
        if (pageNr > nrOfPages)
        {
            pageNr = nrOfPages;
            Debug.Log("hit the end of the journal!");
        }

        Sections[curSectionIndex].OpenPage(pageNr - minSectionNr);

        if (pageNr < minSectionNr)
        {
            if (curSectionIndex > 1)
            {
                OpenSection(Sections[curSectionIndex - 1].sectionName);
                return;
            }

            Debug.LogError("tried going to a section below 0!");
            return;
        }
        if (pageNr > maxSectionNr)
        {
            if (curSectionIndex < Sections.Count - 2)
            {
                OpenSection(Sections[curSectionIndex + 1].sectionName, true);
                return;
            }

            Debug.LogError($"tried going to a section above {Sections.Count}!");
            return;
        }

    }


    public void OpenSection(SectionName sectionName, bool maxPageNr = false)
    {
        if (sectionName == Sections[curSectionIndex].sectionName)
        {
            Debug.LogWarning("that section was already opened!");
            return;
        }

        Sections[curSectionIndex].CloseSection();

        int i = 0;
        foreach (var section in Sections)
        {
            if (section.sectionName == sectionName)
            {
                section.OpenSection();
                curSectionIndex = i;

                OpenBookmark(sectionName);
                return;
            }
            i++;
        }

        Debug.LogError("couldn't find the section to open!");
    }


    private IEnumerator ZoomIn(float timer = 0)
    {
        if (timer == 0)
        {
            timer = animationTime;
        }

        while (timer > 0)
        {
            timer -= Time.deltaTime;

            if (zoomedIn == false)
            {
                zoomCoroutine = StartCoroutine(ZoomOut(timer));
                yield break;
            }

            float T = timer / animationTime;
            float curveOutput = animationCurve.Evaluate(T);

            cam.fieldOfView = zoomFOV + curveOutput * (initialFOV - zoomFOV);

            yield return new WaitForEndOfFrame();
        }

        zoomCoroutine = null;
    }

    private IEnumerator ZoomOut(float timer = 0)
    {
        if (timer == 0)
        {
            timer = 0;
        }

        while (timer < animationTime)
        {
            timer += Time.deltaTime;

            if (zoomedIn == true)
            {
                zoomCoroutine = StartCoroutine(ZoomIn(timer));
                yield break;
            }

            float T = timer / animationTime;
            float curveOutput = animationCurve.Evaluate(T);

            cam.fieldOfView = zoomFOV + curveOutput * (initialFOV - zoomFOV);

            yield return new WaitForEndOfFrame();
        }

        zoomCoroutine = null;
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

}
