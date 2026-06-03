using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Bookmark : MonoBehaviour
{
    Journal journal;
    public TutorialSequence currentSequence = null;
    [NonSerialized]
    public int currentTutorialStep = -1;
    public SectionName sectionName;
    Vector3 startPos;
    bool selected = false;

    private void Start()
    {
        startPos = transform.localPosition;
        journal = GetComponentInParent<Journal>();
    }

    void Update()
    {
        if (journal.playerControls.Journal.Click.triggered)
        {
            if (IsPointerOverUIElement())
            {
                journal.OpenSection(sectionName);
                journal.bookmarkClicked?.Invoke();
            }
        }
    }

    public void Highlight()
    {
        if(currentSequence != null && currentTutorialStep == 5)
        {
            ExitStep();
        }
        
        if (!selected)
        {
            transform.Translate(0, 0.025f, 0);
            selected = true;
        }
    }

    public void UnHighlight()
    {
        transform.localPosition = startPos;
        selected = false;
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
        
        if(currentTutorialStep == 8)
        {
            // Unimplemented
            // EnableControl(1);
        }
    }
    
    public void ExitStep()
    {
        if (!currentSequence)
        {
            Debug.LogError("tried continueing a tutorial while none was assigned!");
            return;
        }

        currentSequence.FinishStep(currentTutorialStep);
    }


    public bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }
    public bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];

            if (curRaysastResult.gameObject == gameObject)
                return true;
        }

        return false;
    }
    static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);

        return raysastResults;
    }
}
