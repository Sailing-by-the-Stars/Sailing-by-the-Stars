using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System;
using Unity.VisualScripting;

public class TutorialDialogue : AstroDialogue, AstroTutorialStep
{
    public TutorialSequence currentSequence;

    public int currentTutorialStep = -1;

    protected override void Start()
    {
        
    }

    public void EnterStep(TutorialSequence sequence)
    {
        waitFlag = false;

        if (currentSequence != null && currentSequence != sequence)
        {
            Debug.LogWarning("this tutorialDialogue object is already in a different sequence!!");
            return;
        }
        currentSequence = sequence;
        currentTutorialStep = sequence.index;

        if(currentDialogue == null || currentDialogue != this)
        {
            StartDialogue(currentSequence.EventOrder[sequence.index].dialogueEntry);
        }
        else
        {
            TryGoNextline(currentSequence.EventOrder[sequence.index].dialogueEntry);
        }
    }

    public override void WaitFlag()
    {
        if (waitFlag)
        {
            return;
        }
        base.WaitFlag();

        if (!currentSequence)
        {
            Debug.LogWarning("tried waiting on a tutorial while none was assigned!");
            return;
        }

        //currentSequence.FinishStep(currentTutorialStep);

        currentSequence.finishStep += Finish;

        ExitStep();
    }

    public void Finish()
    {
        currentSequence.finishStep -= Finish;

        TurnOffTextBoxes(true);
    }

    public void ExitStep()
    {
        if (!currentSequence)
        {
            Debug.LogWarning("tried continueing a tutorial while none was assigned!");
            return;
        }

        currentSequence.FinishStep(currentTutorialStep);
        //Debug.LogError("gaaaaaaaaah");
    }

    void TryGoNextline(int Dindex)
    {
        if(Dindex < 0)
        {
            Dindex = index + 1;
        }

        if (NextLine(Dindex) == false)
        {
            Debug.LogWarning("couldn't go to the next line!!!");
        }
    }

    protected override void TurnOffTextBoxes()
    {
        base.TurnOffTextBoxes();
        ExitStep();

        currentSequence = null;
    }

    protected void TurnOffTextBoxes(bool bleeeh)
    {
        base.TurnOffTextBoxes();

        currentSequence = null;
    }

}
