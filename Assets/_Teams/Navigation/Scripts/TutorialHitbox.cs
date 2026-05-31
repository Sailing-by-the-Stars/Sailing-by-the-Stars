using UnityEngine;

public class TutorialHitbox : MonoBehaviour, AstroTutorialStep
{
    bool waitingForPlayer = false;

    [SerializeField] bool playerIsIn = false;


    public TutorialSequence currentSequence;

    public int currentTutorialStep = -1;

    public void EnterStep(TutorialSequence sequence)
    {
        if (currentSequence != null && currentSequence != sequence)
        {
            Debug.LogWarning("this tutorialDialogue object is already in a different sequence!!");
            return;
        }
        currentSequence = sequence;
        currentTutorialStep = sequence.index;

        waitingForPlayer = true;
        if (waitingForPlayer && playerIsIn)
        {
            ExitStep();
        }
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsIn = true;
        }


        if (waitingForPlayer && playerIsIn)
        {
            ExitStep();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsIn = false;
        }
    }
}
