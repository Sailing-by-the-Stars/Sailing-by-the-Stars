using Assets._Teams.Island_1.Scripts.User_Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public enum SectionName
{
    AstrolabeInstructions,
    Navigation,
    Lore,
    Quests,
}


[Serializable]
public class Page
{
    public Texture leftPage;
    public Texture rightPage;
    [NonSerialized]
    public int pageID = -1;
}


public class Journal : ToolPickup, AstroTutorialStep
{
    [SerializeField]
    bool skipTutorial = false;


    public PlayerControls playerControls;
    GameState prevState;

    //[NonSerialized]
    public TutorialSequence currentSequence = null;
    [NonSerialized]
    public int currentTutorialStep = -1;

    private bool isEquipped;
    private bool bookOpened;

    private GameObject bookModel;
    
    public float maxPageWidth = 0f;
    public float maxPageHeight = 0f;

    [SerializeField]
    List<JournalSection> Sections = new();

    public int curPageNr = 0;
    [NonSerialized]
    public int prevPageNumber = -1;
    int curSectionIndex = 0;

    List<Bookmark> bookmarks;
    public Action bookmarkClicked;

    private Camera cam;
    private float initialFOV;
    private float zoomFOV = 30;
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


    [SerializeField]
    int tutorialStep = 0;

    JournalOpenSound openSound;
    JournalCloseSound closeSound;


    Canvas popupui;
    private void Awake()
    {
        openSound = FindFirstObjectByType<JournalOpenSound>();
        closeSound = FindFirstObjectByType<JournalCloseSound>();
    }

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

        RewardItem.collectedReward?.Invoke(0);

        if (!skipTutorial)
        {
            TutorialSequence.Instance.NextStep(0);
        }
        else
        {
            TutorialInputs();
        }

            isEquipped = true;

        //Disable collider to hide interact text
        GetComponent<BoxCollider>().enabled = false;
        bookOpened = false;

        foreach (Bookmark bookmark in bookmarks)
        {
            bookmark.gameObject.SetActive(false);
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

    public int getFullPageNR(Page page)
    {
        int extraPages = 0;
        int fullPageID = 0;
        foreach (JournalSection section in Sections)
        {
            foreach (Page sPage in section.pages)
            {
                if (sPage == page)
                {
                    fullPageID = extraPages + sPage.pageID;
                    return fullPageID;
                }
            }

            extraPages += section.nrOfPages;
        }
        Debug.LogError("couldn't find the page in the journal!");
        return -1;
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

        //Throws object not found error but works...

        popupui = gameObject.GetComponentInChildren<Canvas>();
        if (popupui)
        {
            popupui.enabled = false;

        }
        else
        {
            Debug.LogError("journal has no canvas?");
        }

        if (skipTutorial)
        {

            var player = GameObject.FindGameObjectWithTag("Player");
            var pickupController = player.GetComponent<PickupController>();

            Grab(pickupController);
        }
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

                if (maxSectionNr >= curPageNr && section.firstPageNr <= curPageNr)
                {
                    curSection = section;
                    curSectionIndex = i;
                }
                
                i++;
                totalNrOfPages += section.nrOfPages;
            }
            
            if (curSection == null)
            {
                Debug.LogError($"the pageNr {curPageNr} is somehow out of range!");
                return;
            }
            
            // Only run OpenSection() when the pageNr goes in or our out of a section
            // Doing this while checking if the page number is in the bounds of a section
            // in the sections loop above would case OpenSection() to be run every frame
            if (playerControls.Journal.PreviousPage.triggered)
            {
                curPageNr -= 1;
                
                if (curPageNr < 0)
                {
                    curPageNr = 0;
                }
                
                if (curPageNr < curSection.firstPageNr)
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
                curPageNr += 1;
                
                if (curPageNr > totalNrOfPages - 1)
                {
                    curPageNr = totalNrOfPages - 1;
                    Debug.Log("hit the end of the journal!");
                }
                
                if (curPageNr > curSection.lastPageNr - 1)
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


            if(prevPageNumber != curPageNr)
            {
                Sections[curSectionIndex].OpenPage(curPageNr - curSection.firstPageNr);
                prevPageNumber = curPageNr;
            }
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

                prevPageNumber = -1;
                if (bookOpened)
                {
                    
                    if (openSound != null)
                    {
                        closeSound.PlaySound();
                    }
                    else
                    {
                        Debug.LogWarning("couldn't find journal open sound! make sure it's added to the AudioManager");
                    }
                    bookOpened = false;
                    leftPageQ.enabled = false;
                    rightPageQ.enabled = false;
                    bookModel.SetActive(false);
                    foreach (Bookmark bookmark in bookmarks)
                    {
                        bookmark.gameObject.SetActive(false);
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
                        bookmark.gameObject.SetActive(true);
                    }
                    
                    //Update section for if a new page is picked up in a different section
                    int maxSectionNr = 0;
                    curSectionIndex = 0;
            
                    int i = 0;
                    foreach (JournalSection section in Sections)
                    {
                        maxSectionNr += section.nrOfPages;
                        section.lastPageNr = maxSectionNr;
                        section.firstPageNr = section.lastPageNr - section.nrOfPages;

                        if (maxSectionNr >= curPageNr && section.firstPageNr <= curPageNr)
                        {
                            curSectionIndex = i;
                        }
                
                        i++;
                    }
                    
                    Sections[curSectionIndex].OpenSection();
                    
                    prevState = TempStateMachine.Instance.gameState;
                    TempStateMachine.Instance.SetState(GameState.Journal);

                    if (closeSound != null)
                    {
                        openSound.PlaySound();
                    }
                    else
                    {
                        Debug.LogWarning("couldn't find journal close sound! make sure it's added to the AudioManager");
                    }
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
                    curPageNr = section.lastPageNr - 1;
                }
                else
                {
                    curPageNr = section.firstPageNr;
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


    public void OpenPage(Page page, int extraPages)
    {
        int newPageNr = getFullPageNR(page);

        if (curPageNr > newPageNr - extraPages)
        {
            curPageNr += extraPages;
            prevPageNumber = curPageNr;
        }

        if (curPageNr >= newPageNr)
        {
            //Jump back to the new page
            curPageNr -= curPageNr - newPageNr;
            prevPageNumber = curPageNr - 1;
        }
        else if (curPageNr < newPageNr)
        {
            //Jump forward to the new page
            curPageNr += curPageNr + newPageNr;
            prevPageNumber = curPageNr - 1;
        }
    }



    public void EnterStep(TutorialSequence sequence)
    {
        if (sequence == null)
        {
            currentTutorialStep = int.MaxValue;
        } else if (currentSequence != null && currentSequence != sequence)
        {
            Debug.LogError("this tutorialDialogue object is already in a different sequence!!");
            return;
        }
        
        currentSequence = sequence;
        currentTutorialStep += 1;

        //Debug.Log($"going to the next tutorial step: {currentTutorialStep}");


        switch (currentTutorialStep)
        {
            case 0:
                ///step 0:
                ///turn off all inputs
                TutorialInputs(-1);
                //finish when user presses J
                playerControls.Journal.Open.performed += FinishInputPrompt;
                break;
                //wait 1 text prompt,
                //then turn on journal.open
                //TutorialInputs(0);
            case 1:
                ///step 1:
                ///turn off inputs
                TutorialInputs(-1);
                //finish when either is pressed
                playerControls.Journal.PreviousPage.performed += FinishInputPrompt;
                playerControls.Journal.NextPage.performed += FinishInputPrompt;
                break;
                //wait a prompt
                //then turn on the page flips
                //TutorialInputs(1);
            case 2:
                ///step 2:
                ///turn on zoom
                TutorialInputs(2);
                //finish on press
                playerControls.Journal.Zoom.performed += FinishInputPrompt;

                break;
            case 3:
                ///step 3:
                ///turn on zoom
                TutorialInputs(2);
                //finish on press
                playerControls.Journal.Zoom.performed += FinishInputPrompt;
                break;
            case 4:
                ///step 4:
                ///turn on bookmark clicks
                TutorialInputs(3);
                //finish when any of them are pressed
                bookmarkClicked += FinishInputPrompt;
                break;
            case 5:
                ///step 5: 
                ///turn off all input
                TutorialInputs(4);
                //wait 2 prompts
                //TutorialInputs();
                //prompt for j press 
                playerControls.Journal.Open.performed += FinishInputPrompt;
                break;
                //turn on controls hud
                //finish
            default:
                if (popupui)
                {
                    popupui.enabled = true;
                }
                else
                {
                    Debug.LogWarning("no popup ui assigned?");
                }

                    TutorialInputs();

                break;
        }
    }

    private void FinishInputPrompt(InputAction.CallbackContext context)
    {
        FinishInputPrompt();
    }

    private void FinishInputPrompt()
    {
        playerControls.Journal.Open.performed -= FinishInputPrompt;
        playerControls.Journal.PreviousPage.performed -= FinishInputPrompt;
        playerControls.Journal.NextPage.performed -= FinishInputPrompt;
        playerControls.Journal.Zoom.performed -= FinishInputPrompt;
        bookmarkClicked -= FinishInputPrompt;



        ExitStep();
    }


    public void TutorialInputs(int tutorialIndex = -2)
    {
        playerControls.Journal.Disable();

        switch (tutorialIndex)
        {
            case -1:
                break;
            case 0:
                playerControls.Journal.Open.Enable();
                break;
            case 1:
                playerControls.Journal.PreviousPage.Enable();
                playerControls.Journal.NextPage.Enable();
                break;
            case 2:
                playerControls.Journal.Zoom.Enable();
                break;
            case 3:
                playerControls.Journal.Click.Enable();
                break;

            case 4:
                playerControls.Journal.Enable();
                playerControls.Journal.Open.Enable();
                break;
            default:
                playerControls.Journal.Enable();
                break;
        }
    }

    public void ExitStep()
    {
        if (!currentSequence)
        {
            Debug.LogError("tried continueing a tutorial while none was assigned!");
            return;
        }
        TutorialSequence tempSeq = currentSequence;
        currentSequence = null;
        tempSeq.FinishStep(currentTutorialStep);
    }

}
