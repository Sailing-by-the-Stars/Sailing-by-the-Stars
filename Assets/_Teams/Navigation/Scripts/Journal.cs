using Assets._Teams.Island_1.Scripts.User_Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;

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

    public TutorialSequence currentSequence = null;
    [NonSerialized]
    public int currentTutorialStep = -1;

    private bool isEquipped;
    public bool bookOpened { get; private set; }

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

    private CinemachineCamera cam;
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

        playerControls.Journal.Open.performed += TryToggleJournal;
    }


    private void OnDisable()
    {

    }

    void Start()
    {
        bookModel = transform.GetChild(1).gameObject;

        bookmarks = new();
        foreach (Bookmark bookmark in GetComponentsInChildren<Bookmark>())
            bookmarks.Add(bookmark);

        foreach (JournalSection section in Sections)
            section.parentJournal = this;

        List<MeshRenderer> images = GetComponentsInChildren<MeshRenderer>().ToList();
        foreach (MeshRenderer image in images)
        {
            if (image.name == "leftPage") leftPageQ = image;
            if (image.name == "rightPage") rightPageQ = image;
        }

        leftPageQ.enabled = false;
        rightPageQ.enabled = false;

        popupui = gameObject.GetComponentInChildren<Canvas>();
        if (popupui)
            popupui.enabled = false;
        else
            Debug.LogError("journal has no canvas?");

        if (skipTutorial)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            Grab(player.GetComponent<PickupController>());
        }
    }

    void Update()
    {
        HandleInput();
        RenderCurrentPage();
    }



    public override void Grab(PickupController pickupController)
    {
        PuzzleProgress.MarkComplete("denial");
        base.Grab(pickupController);
        RewardItem.collectedReward?.Invoke(0);
        PuzzleProgress.MarkComplete("denial");

#if !UNITY_EDITOR
        skipTutorial = false;
#endif

        if (!skipTutorial)
            TutorialSequence.Instance.NextStep(0);
        else
        {
            TutorialSequence.Instance.NextStep(1000);
            TutorialInputs();
        }

        isEquipped = true;

        GetComponent<BoxCollider>().enabled = false;
        bookOpened = false;

        foreach (Bookmark bookmark in bookmarks)
            bookmark.gameObject.SetActive(false);

        bookModel.SetActive(false);

        initalRot = Quaternion.Euler(90, 90, 90);
        transform.localRotation = initalRot;

        cam = Camera.main.GetComponent<CinemachineBrain>().ActiveVirtualCamera as CinemachineCamera;
        initialFOV = cam.Lens.FieldOfView;
        animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        zoomedIn = false;
        if (zoomCoroutine == null)
            zoomCoroutine = StartCoroutine(ZoomOut(animationTime - Time.deltaTime));
    }



    void HandleInput()
    {
        if (!bookOpened) return;

        if (playerControls.Journal.PreviousPage.triggered)
            GoToPage(curPageNr - 1);

        if (playerControls.Journal.NextPage.triggered)
            GoToPage(curPageNr + 1);

        ToggleZoom();
    }




    public void ForcePage(int pageNr)
    {
        if(pageNr >= 0)
        {
            if (!bookOpened)
            {
                TryToggleJournal();
            }


            GoToPage(pageNr);

            playerControls.Journal.Disable();
        }
        else
        {
            if (bookOpened)
            {
                playerControls.Journal.Enable();
            }
        }
    }


    /// <summary>
    /// Navigates to an absolute page number, switching sections as needed.
    /// Safe to call whether the journal is open or closed.
    /// </summary>
    public void GoToPage(int targetPageNr)
    {
        int totalPages = 0;
        int runningTotal = 0;
        JournalSection targetSection = null;
        int targetSectionIndex = 0;

        for (int i = 0; i < Sections.Count; i++)
        {
            JournalSection section = Sections[i];
            section.firstPageNr = runningTotal;
            runningTotal += section.nrOfPages;
            section.lastPageNr = runningTotal; 
            totalPages = runningTotal;

            if (targetPageNr >= section.firstPageNr && targetPageNr < section.lastPageNr)
            {
                targetSection = section;
                targetSectionIndex = i;
            }
        }


        targetPageNr = Mathf.Clamp(targetPageNr, 0, totalPages - 1);


        if (targetSection == null)
        {
            for (int i = 0; i < Sections.Count; i++)
            {
                if (targetPageNr >= Sections[i].firstPageNr && targetPageNr < Sections[i].lastPageNr)
                {
                    targetSection = Sections[i];
                    targetSectionIndex = i;
                    break;
                }
            }
        }

        if (targetSection == null)
        {
            Debug.LogError($"GoToPage: could not find a section for page {targetPageNr}");
            return;
        }

        
        if (targetSectionIndex != curSectionIndex)
        {
            Sections[curSectionIndex].CloseSection();
            targetSection.OpenSection();
            curSectionIndex = targetSectionIndex;
            OpenBookmark(targetSection.sectionName);
        }

        curPageNr = targetPageNr;
        prevPageNumber = curPageNr - 1;
    }



    void RenderCurrentPage()
    {
        if (!bookOpened) return;
        if (prevPageNumber == curPageNr) return;

        
        int running = 0;
        for (int i = 0; i < Sections.Count; i++)
        {
            Sections[i].firstPageNr = running;
            running += Sections[i].nrOfPages;
            Sections[i].lastPageNr = running;
        }

        JournalSection curSection = Sections[curSectionIndex];
        int localPageNr = curPageNr - curSection.firstPageNr;

        Sections[curSectionIndex].OpenPage(localPageNr);
        prevPageNumber = curPageNr;
    }



    

    private void TryToggleJournal(InputAction.CallbackContext context)
    {
        TryToggleJournal();
    }

    
    public void TryToggleJournal()
    {
        if (!isEquipped) return;

        prevPageNumber = -1;

        if (bookOpened)
        {
            CloseJournal();
            TempStateMachine.Instance.SetState(prevState);
        }
        else
        {
            if (TempStateMachine.Instance.gameState == GameState.Astrolabe) return;
            OpenJournal();
            TempStateMachine.Instance.SetState(GameState.Journal);
        }
    }

    public void OpenJournal()
    {
        if (openSound != null)
            openSound.PlaySound();
        else
            Debug.LogWarning("couldn't find journal open sound!");

        bookOpened = true;
        bookModel.SetActive(true);

        foreach (Bookmark bookmark in bookmarks)
            bookmark.gameObject.SetActive(true);

        prevState = TempStateMachine.Instance.gameState;

        
        GoToPage(curPageNr);
    }

    public void CloseJournal()
    {
        if (closeSound != null)
            closeSound.PlaySound();
        else
            Debug.LogWarning("couldn't find journal close sound!");

        bookOpened = false;
        leftPageQ.enabled = false;
        rightPageQ.enabled = false;
        bookModel.SetActive(false);

        foreach (Bookmark bookmark in bookmarks)
            bookmark.gameObject.SetActive(false);

        Sections[curSectionIndex].CloseSection();

        if (zoomedIn)
        {
            zoomedIn = false;
            if (zoomCoroutine == null)
                zoomCoroutine = StartCoroutine(ZoomOut());
        }
    }

    

    public void OpenBookmark(SectionName sectionName, bool maxPage = false)
    {
        foreach (Bookmark bookmark in bookmarks)
        {
            if (bookmark.sectionName == sectionName)
                bookmark.Highlight();
            else
                bookmark.UnHighlight();
        }
    }

    /// <summary>
    /// Opens a section by name, landing on its first (or last) page.
    /// </summary>
    public void OpenSection(SectionName sectionName, bool maxPageNr = false)
    {
        for (int i = 0; i < Sections.Count; i++)
        {
            if (Sections[i].sectionName == sectionName)
            {
                // Rebuild boundaries first
                int running = 0;
                foreach (var s in Sections)
                {
                    s.firstPageNr = running;
                    running += s.nrOfPages;
                    s.lastPageNr = running;
                }

                int target = maxPageNr
                    ? Sections[i].lastPageNr - 1
                    : Sections[i].firstPageNr;

                GoToPage(target);
                return;
            }
        }

        Debug.LogError($"OpenSection: couldn't find section {sectionName}!");
    }

    public JournalSection GetSection(SectionName sectionName)
    {
        foreach (var section in Sections)
        {
            if (section.sectionName == sectionName)
                return section;
        }

        Debug.LogError($"couldn't find {sectionName} in the journal!");
        return null;
    }

    /// <summary>
    /// Returns the absolute page number for a Page object.
    /// </summary>
    public int getFullPageNR(Page page)
    {
        int running = 0;
        foreach (JournalSection section in Sections)
        {
            foreach (Page sPage in section.pages)
            {
                if (sPage == page)
                    return running + sPage.pageID;
            }
            running += section.nrOfPages;
        }

        Debug.LogError("getFullPageNR: couldn't find the page in the journal!");
        return -1;
    }


    private void ToggleZoom()
    {
        if (!playerControls.Journal.Zoom.triggered) return;

        if (zoomedIn)
        {
            zoomedIn = false;
            if (zoomCoroutine == null)
                zoomCoroutine = StartCoroutine(ZoomOut());
        }
        else
        {
            zoomedIn = true;
            if (zoomCoroutine == null)
                zoomCoroutine = StartCoroutine(ZoomIn());
        }
    }

    private IEnumerator ZoomIn(float timer = 0)
    {
        if (timer == 0) timer = animationTime;

        while (timer > 0)
        {
            timer -= Time.deltaTime;

            if (!zoomedIn)
            {
                zoomCoroutine = StartCoroutine(ZoomOut(timer));
                yield break;
            }

            cam.Lens.FieldOfView = zoomFOV + animationCurve.Evaluate(timer / animationTime) * (initialFOV - zoomFOV);
            yield return new WaitForEndOfFrame();
        }

        zoomCoroutine = null;
    }

    private IEnumerator ZoomOut(float timer = 0)
    {
        while (timer < animationTime)
        {
            timer += Time.deltaTime;

            if (zoomedIn)
            {
                zoomCoroutine = StartCoroutine(ZoomIn(timer));
                yield break;
            }

            cam.Lens.FieldOfView = zoomFOV + animationCurve.Evaluate(timer / animationTime) * (initialFOV - zoomFOV);
            yield return new WaitForEndOfFrame();
        }

        zoomCoroutine = null;
    }



    public void EnterStep(TutorialSequence sequence)
    {
        if (sequence == null)
        {
            currentTutorialStep = 100000;
        }
        else if (currentSequence != null && currentSequence != sequence)
        {
            Debug.LogError("this tutorialDialogue object is already in a different sequence!!");
            return;
        }

        currentSequence = sequence;
        currentTutorialStep += 1;

        switch (currentTutorialStep)
        {
            case 0:
                TutorialInputs(-1);
                playerControls.Journal.Open.performed += FinishInputPrompt;
                break;
            case 1:
                TutorialInputs(-1);
                playerControls.Journal.PreviousPage.performed += FinishInputPrompt;
                playerControls.Journal.NextPage.performed += FinishInputPrompt;
                break;
            case 2:
            case 3:
                TutorialInputs(2);
                playerControls.Journal.Zoom.performed += FinishInputPrompt;
                break;
            case 4:
                TutorialInputs(3);
                bookmarkClicked += FinishInputPrompt;
                break;
            case 5:
                TutorialInputs(4);
                playerControls.Journal.Open.performed += FinishInputPrompt;
                break;
            default:
                if (popupui)
                    popupui.enabled = true;
                else
                    Debug.LogWarning("no popup ui assigned?");
                break;
        }
    }

    private void FinishInputPrompt(InputAction.CallbackContext context) => FinishInputPrompt();

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
            case -1: break;
            case 0: playerControls.Journal.Open.Enable(); break;
            case 1:
                playerControls.Journal.PreviousPage.Enable();
                playerControls.Journal.NextPage.Enable(); break;
            case 2: playerControls.Journal.Zoom.Enable(); break;
            case 3: playerControls.Journal.Click.Enable(); break;
            case 4:
                playerControls.Journal.Enable();
                playerControls.Journal.Open.Enable(); break;
            default: playerControls.Journal.Enable(); break;
        }
    }

    public void ExitStep()
    {
        if (!currentSequence)
        {
            Debug.LogError("tried continuing a tutorial while none was assigned!");
            return;
        }

        TutorialSequence tempSeq = currentSequence;
        currentSequence = null;
        tempSeq.FinishStep(currentTutorialStep);
    }
}