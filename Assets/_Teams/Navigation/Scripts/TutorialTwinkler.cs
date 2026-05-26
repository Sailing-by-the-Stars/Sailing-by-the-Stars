using UnityEngine;

public class TutorialTwinkler : TwinklingStar, AstroTutorialStep
{
    public TutorialSequence currentSequence;

    public int currentTutorialStep = -1;


    private void Start()
    {
    }

    public void EnterStep(TutorialSequence sequence)
    {
        if (currentSequence != null && currentSequence != sequence)
        {
            Debug.LogWarning("this tutorialDialogue object is already in a different sequence!!");
            return;
        }
        currentSequence = sequence;
        currentTutorialStep = sequence.index;

        GetComponent<Renderer>().enabled = true;
        starState = StarState.selected;
    }

    public void ExitStep()
    {
        if (!currentSequence)
        {
            Debug.LogWarning("tried continueing a tutorial while none was assigned!");
            return;
        }

        currentSequence.FinishStep(currentTutorialStep);

        Destroy(gameObject);
    }

    public override void Hit(float hitAngle)
    {
        if (!currentSequence)
        {
            return;
        }

        if (starState == StarState.dimmed)
        {
            return;
        }

        if (hitAngle < 5)
        {
            return;
        }

        ExitStep();
    }
}
