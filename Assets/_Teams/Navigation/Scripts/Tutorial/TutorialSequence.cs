using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[Serializable]
public class SequenceEntry
{ 
    public UnityEvent<TutorialSequence> sequenceEnter = new();
    public bool continueAfterEntry = true;
    public int nextEntry = -1;
    public int dialogueEntry = -1;

#if UNITY_EDITOR
    public string dialogueEntryText;
#endif

    public bool clickForDialogue = false;
}

public class TutorialSequence : MonoBehaviour
{
    public UnityEvent enterTutorial;
    public UnityEvent exitTutorial;

    public bool finishedTutorial = false;
    public bool startedTutorial = false;
    public static TutorialSequence Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            Debug.LogWarning("tried initializing another tutorialSequence");
            return;
        }

        Instance = this;
    }



    [SerializeField]
    bool startOnStart = false;
    public List<SequenceEntry> EventOrder = new();
    public int index = 0;

    private int currentStepFinishes = 0;

    public Action finishStep;

    private void Start()
    {
        if (startOnStart)
        {
            NextStep(0);
        }
    }


    public void FinishStep(int stepIndex, bool continueStep = true)
    {
        currentStepFinishes += 1;

        if (index != stepIndex)
        {
            Debug.LogWarning("tried finishing the wrong step!!");
            return;
        }
        
        if (EventOrder[index].sequenceEnter.GetPersistentEventCount() > currentStepFinishes)
        {
            //Debug.Log($"waiting for {EventOrder[index].sequenceEnter.GetPersistentEventCount() - currentStepFinishes} more events to finish before the next step");
            return;
        }
        else
        {
            finishStep?.Invoke();

            if (EventOrder[index].continueAfterEntry)
            {
                if (EventOrder[index].nextEntry >= 0)
                {
                    NextStep(EventOrder[index].nextEntry);
                    return;
                }
                NextStep();
            }
        }
    }

    public void NextStep(int stepIndex = -1)
    {


        if(stepIndex == 0)
        {
            enterTutorial.Invoke();
        }

        startedTutorial = true;
        if(stepIndex != -1)
        {
            index = stepIndex;
        }
        else
        {
            index++;
            stepIndex = index;
        }
        //Debug.LogError(stepIndex);

        if(index + 1 > EventOrder.Count)
        {
            Debug.LogWarning("got to the end of the tutorial!");
            exitTutorial
                .Invoke();
            finishedTutorial = true;
            return;
        }

        currentStepFinishes = 0;
        EventOrder[index].sequenceEnter.Invoke(this);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        TutorialDialogue tutorialDialogue = GetComponent<TutorialDialogue>();

        foreach (var step in EventOrder)
        {
            if(step.dialogueEntry >= 0 && tutorialDialogue && tutorialDialogue.dialogue.Count > step.dialogueEntry)
            {
                step.dialogueEntryText = tutorialDialogue.dialogue[step.dialogueEntry].text;
            }
        }
    }
#endif
}
