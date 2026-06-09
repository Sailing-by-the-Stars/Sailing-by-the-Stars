using UnityEngine;

public class AstrolabeTutorialSeq : TutorialSequence 
{
    public static AstrolabeTutorialSeq AstroInstance;

    public static bool finished;

    private void Awake()
    {
        if (AstroInstance != null && AstroInstance != this)
        {
            Destroy(this);
            Debug.LogWarning("tried initializing another tutorialSequence");
            return;
        }

        AstroInstance = this;
    }

    public void ManualFinish()
    {
        finished = true;
    }


    public override void NextStep(int stepIndex = -1)
    {


        if (stepIndex == 0)
        {
            enterTutorial.Invoke();
        }

        startedTutorial = true;
        if (stepIndex != -1)
        {
            index = stepIndex;
        }
        else
        {
            index++;
            stepIndex = index;
        }
        //Debug.LogError(stepIndex);

        if (index + 1 > EventOrder.Count)
        {
            Debug.LogWarning("got to the end of the tutorial!");
            exitTutorial
                .Invoke();
            finishedTutorial = true;
            finished = true;
            return;
        }

        currentStepFinishes = 0;
        EventOrder[index].sequenceEnter.Invoke(this);
    }

}
