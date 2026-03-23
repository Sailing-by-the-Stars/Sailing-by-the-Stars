using UnityEngine.InputSystem;

namespace Assets._Teams.Island_1.Scripts.User_Interface.Tutorials
{
    public class PickupItemTutorial : TutorialPopup
    {
        void Update()
        {
            // TODO: replace with pickup item event trigger instead of input check in tutorial
            bool pressedEKey = Keyboard.current.eKey.wasPressedThisFrame;

            if (pressedEKey)
            {
                TutorialManager.Instance.CompleteStep(Step);
            }
        }
    }
}
