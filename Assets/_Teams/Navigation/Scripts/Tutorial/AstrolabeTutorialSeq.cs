using UnityEngine;

public class AstrolabeTutorialSeq : TutorialSequence
{
    public static AstrolabeTutorialSeq AstroInstance;

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

}
