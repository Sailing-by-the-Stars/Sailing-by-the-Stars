using UnityEngine;

namespace Assets._Teams.Island_1.Scripts.User_Interface
{
    // Author: Sander Kleine
    public class TutorialPopup: MonoBehaviour
    {
        public TutorialStep Step;

        public virtual void Complete()
        {
            TutorialManager.Instance.CompleteStep(Step);
        }
    }
}
