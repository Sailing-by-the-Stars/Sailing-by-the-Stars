using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Rendering.HighDefinition;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using static UnityEngine.Rendering.DebugUI;

[Serializable]
public class Page
{
    public Texture leftPage;
    public Texture rightPage;
}

public class LegacyJournal : ToolPickup, AstroTutorialStep
{
    PlayerControls playerControls;

    GameState prevState;


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

        playerControls.Journal.BookmarkPage.performed += OpenBookmark;
    }

    private void OnDisable()
    {
        playerControls.Journal.BookmarkPage.performed -= OpenBookmark;
    }

    public void OpenBookmark(InputAction.CallbackContext ctx)
    {
        int numKeyValue; 

        int.TryParse(ctx.control.name, out numKeyValue);

        if(numKeyValue - 1 < 0)
        {
            return;
        }

        if (bookOpened)
        {
                if (sectionPageNrs[numKeyValue - 1] <= pages.Count - 1)
                {
                    pageNr = sectionPageNrs[numKeyValue - 1];

                    for (int i = 0; i < bookmarkTransform.childCount; i++)
                    {
                        bookmarkTransform.GetChild(i).GetComponent<Renderer>().material.color = Color.white;
                    }

                    bookmarkTransform.GetChild(numKeyValue - 1).GetComponent<Renderer>().material.color = Color.green;
                }
        }

    }
    
    //Callback from the bookmark being clicked
    public void OpenBookmark(int sectionPageNr)
    {
        if(sectionPageNr < 0)
        {
            return;
        }

        if (bookOpened)
        {
            if (sectionPageNrs[sectionPageNr] <= pages.Count - 1)
            {
                pageNr = sectionPageNrs[sectionPageNr];
                for (int i = 0; i < bookmarkTransform.childCount; i++)
                {
                    bookmarkTransform.GetChild(i).GetComponent<Renderer>().material.color = Color.white;
                }
            }
        }

    }

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
            if (playerControls.Journal.PreviousPage.triggered && bookVisible && bookOpened)
            {
                pageNr -= 1;
            }

            if (playerControls.Journal.NextPage.triggered && bookVisible && bookOpened)
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

            if (playerControls.Journal.Open.triggered)
            {
                if (bookVisible)
                { 
                    bookVisible = false;
                    bookOpened = false;

                    TempStateMachine.Instance.SetState(prevState);
                }
                else
                {
                    if (TempStateMachine.Instance.gameState == GameState.Astrolabe)
                    {
                        return;
                    }

                    transform.GetChild(1).gameObject.SetActive(true);
                    bookVisible = true;
                    bookOpened = true;
                    bookangle = 100;

                    if(TutorialSequence.startedTutorial && TutorialSequence.Instance.index == 0)
                    {
                        TutorialSequence.Instance.NextStep(1);
                    }

                    prevState = TempStateMachine.Instance.gameState;
                    TempStateMachine.Instance.SetState(GameState.Journal);
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


    private void SetItemByKeyValue(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {

        int numKeyValue; // the number key value we want from this keypress

        int.TryParse(ctx.control.name, out numKeyValue);
        // Warning! If ctx.control.name can't parse as an int, numKeyValue will be 0

        Debug.Log("int value of keypress is: " + numKeyValue);

        // Now do something with the key value ...

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

        Transform bookmarkPrefabs = transform.GetChild(0).GetChild(0).GetChild(2);
        for (int i = 0; i < sectionPageNrs.Count; i++)
        {
            //bookmarkPrefabs.GetChild(i).GetComponent<Bookmark>().pageNr =  sectionPageNrs[i];
        }
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

            float T =  timer / animationTime;
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

            if(zoomedIn == true)
            {
                zoomCoroutine = StartCoroutine(ZoomIn(timer));
                yield break;
            }
            
            float T =  timer / animationTime;
            float curveOutput = animationCurve.Evaluate(T);
            
            cam.fieldOfView = zoomFOV + curveOutput * (initialFOV - zoomFOV);
            
            yield return new WaitForEndOfFrame();
        }

        zoomCoroutine = null;
    }

    private void OnPointerDown(PointerEventData eventData)
    {
        
    }
}
