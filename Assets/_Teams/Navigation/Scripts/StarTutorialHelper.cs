using UnityEngine;

public class StarTutorialHelper : MonoBehaviour, AstroTutorialStep
{
    int currentTutorialStep = -1;
    TutorialSequence currentSequence;
    public void EnterStep(TutorialSequence sequence)
    {
        if (currentSequence != null && currentSequence != sequence)
        {
            Debug.LogError("this tutorialDialogue object is already in a different sequence!!");
            return;
        }

        currentSequence = sequence;
        currentTutorialStep = currentSequence.index;


        foreach (TwinklingStar twinkler in TwinklingStar.tutorialStars)
        {
            twinkler.EnterStep(this);
        }
    }

    public void ExitStep()
    {
        if (!currentSequence)
        {
            Debug.LogError("tried continueing a tutorial while none was assigned!");
            return;
        }

        if (currentTutorialStep == -1)
        {
            Debug.LogWarning("the current tutorial step isn't assigned correctly!!");
            return;
        }

        TutorialSequence tempSeq = currentSequence;
        currentSequence = null;
        tempSeq.FinishStep(currentTutorialStep);
    }

    public void enableStars()
    {
        foreach (TwinklingStar twinkler in TwinklingStar.tutorialStars)
        {
            twinkler.starState = StarState.selected;
        }
    }
}
