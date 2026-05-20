using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Rendering;
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
    GameState prevState;

    [NonSerialized]
    public TutorialSequence currentSequence;
    [NonSerialized]
    public int currentTutorialStep = -1;

    private bool isEquipped;
    private bool bookOpened;

    private GameObject bookModel;
    
    public float maxPageWidth = 0f;
    public float maxPageHeight = 0f;

    [SerializeField]
    List<JournalSection> Sections = new();
    
    int pageNr = 0;
    int curSectionIndex = 0;

    List<Bookmark> bookmarks;
    
    private Camera cam;
    private float initialFOV;
    private float zoomFOV = 35;
    private float animationTime = 0.75f;

    private AnimationCurve animationCurve;
    public static bool zoomedIn;

    private Coroutine zoomCoroutine;
    
    [HideInInspector]  
    public MeshRenderer leftPageQ;

    [HideInInspector]
    public MeshRenderer rightPageQ;
    
    //Animation stuff
    private Quaternion initalRot;
    
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
        bookOpened = false;

        foreach (Bookmark bookmark in bookmarks)
        {
            bookmark.GameObject().SetActive(false);
        }
        
        bookModel.SetActive(false);
        
        
        ///animation stuff:
        // bookangle = 101;
        initalRot = Quaternion.Euler(90, 90, 90);
        transform.localRotation = initalRot;

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
        bookModel = transform.GetChild(1).gameObject;
        bookmarks = new();
        foreach (Bookmark bookmark in GetComponentsInChildren<Bookmark>())
        {
            bookmarks.Add(bookmark);
        }

        foreach (JournalSection section in Sections)
        {
            section.parentJournal = this;
        }
        
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
        
        leftPageQ.enabled = false;
        rightPageQ.enabled = false;
    }


    void Update()
    {
        HandlePageTurn();
        ToggleJournal();
        ToggleZoom();
    }

    void HandlePageTurn()
    {
        if (bookOpened)
        { 
            int maxSectionNr = 0;
            JournalSection curSection = null;
            curSectionIndex = 0;
            int totalNrOfPages = 0;

            int i = 0;
            foreach (JournalSection section in Sections)
            {
                maxSectionNr += section.nrOfPages;
                section.lastPageNr = maxSectionNr;
                section.firstPageNr = section.lastPageNr - section.nrOfPages;

                if (maxSectionNr >= pageNr && section.firstPageNr <= pageNr)
                {
                    curSection = section;
                    curSectionIndex = i;
                }
                
                i++;
                totalNrOfPages += section.nrOfPages;
            }
            
            
            if (curSection == null)
            {
                Debug.LogError($"the pageNr {pageNr} is somehow out of range!");
                return;
            }
            
            // Only run OpenSection() when the pageNr goes in or our out of a section
            // Doing this while checking if the page number is in the bounds of a section
            // in the sections loop above would case OpenSection() to be run every frame
            if (playerControls.Journal.PreviousPage.triggered)
            {
                pageNr -= 1;
                
                if (pageNr < 0)
                {
                    pageNr = 0;
                }
                
                if (pageNr < curSection.firstPageNr)
                {
                    if (curSectionIndex > 0)
                    {
                        OpenSection(Sections[curSectionIndex - 1].sectionName, true);
                        return;
                    }
                
                    Debug.LogError("tried going to a section below 0!");
                    return;
                }
            }

            if (playerControls.Journal.NextPage.triggered)
            {
                pageNr += 1;
                
                if (pageNr > totalNrOfPages - 1)
                {
                    pageNr = totalNrOfPages - 1;
                    Debug.Log("hit the end of the journal!");
                }
                
                if (pageNr > curSection.lastPageNr - 1)
                {
                    if (curSectionIndex < Sections.Count - 1)
                    {
                        OpenSection(Sections[curSectionIndex + 1].sectionName);
                        return;
                    }
                
                    Debug.LogError($"tried going to a section above {Sections.Count}!");
                    return;
                }
            }

            Sections[curSectionIndex].OpenPage(pageNr - curSection.firstPageNr);
        }

    }

    public JournalSection GetSection(SectionName sectionName)
    {
        foreach (var section in Sections)
        {
            if (section.sectionName == sectionName)
            {
                return section;
            }
        }
        
        Debug.LogError($"couldn't find {sectionName} in the journal!");
        return null;
    }

    private void ToggleJournal()
    {
        if (isEquipped)
        {
            if (playerControls.Journal.Open.triggered)
            {
                if (bookOpened)
                {
                    bookOpened = false;
                    leftPageQ.enabled = false;
                    rightPageQ.enabled = false;
                    bookModel.SetActive(false);
                    foreach (Bookmark bookmark in bookmarks)
                    {
                        bookmark.GameObject().SetActive(false);
                    }
                    
                    Sections[curSectionIndex].CloseSection();
                    
                    TempStateMachine.Instance.SetState(prevState);
                    
                    if (zoomedIn)
                    {
                        zoomedIn = false;
                        if (zoomCoroutine == null)
                        {
                            zoomCoroutine = StartCoroutine(ZoomOut());
                        }
                    }
                }
                else
                {
                    if (TempStateMachine.Instance.gameState == GameState.Astrolabe)
                    {
                        return;
                    }
                    
                    bookOpened = true;
                    bookModel.SetActive(true);
                    foreach (Bookmark bookmark in bookmarks)
                    {
                        bookmark.GameObject().SetActive(true);
                    }
                    
                    Sections[curSectionIndex].OpenSection();
                    
                    prevState = TempStateMachine.Instance.gameState;
                    TempStateMachine.Instance.SetState(GameState.Journal);
                }
            }
        }

    }

    private void ToggleZoom()
    {
        if (playerControls.Journal.Zoom.triggered)
        {
            if (bookOpened)
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

                if (maxPageNr)
                {
                    pageNr = section.lastPageNr - 1;
                }
                else
                {
                    pageNr = section.firstPageNr;
                }
                
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
