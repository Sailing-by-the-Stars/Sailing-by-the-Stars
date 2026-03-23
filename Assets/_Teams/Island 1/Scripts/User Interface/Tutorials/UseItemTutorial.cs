using UnityEngine.InputSystem;

namespace Assets._Teams.Island_1.Scripts.User_Interface.Tutorials
{
    public class UseItemTutorial : TutorialPopup
    {
        void Update()
        {
            // TODO: replace with use item event trigger instead of input check in tutorial
            bool pressedLeftMouse = Mouse.current.leftButton.wasPressedThisFrame;

            if (pressedLeftMouse)
            {
                TutorialManager.Instance.CompleteStep(Step);
            }
        }
    }
}
