using UnityEngine;

namespace Assets._Teams.Island_1.Scripts.User_Interface.Tutorials
{
    public class ToggleAstrolabe : TutorialPopup
    {
        void Update()
        {
            if (Input.GetKey(KeyCode.Tab))
            {
                Complete();
            }
        }
    }
}
