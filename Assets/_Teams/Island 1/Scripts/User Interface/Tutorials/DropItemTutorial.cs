using UnityEngine.InputSystem;

namespace Assets._Teams.Island_1.Scripts.User_Interface.Tutorials
{
    public class DropItemTutorial : TutorialPopup
    {
        void Update()
        {
            // TODO: replace with drop item event trigger instead of input check in tutorial
            bool pressedQKey = Keyboard.current.qKey.wasPressedThisFrame;

            if (pressedQKey)
            {
                TutorialManager.Instance.CompleteStep(Step);
            }
        }
    }
}
