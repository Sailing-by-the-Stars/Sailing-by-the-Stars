using UnityEngine;

namespace Assets._Teams.Island_1.Scripts.User_Interface.Tutorials
{
    public class DropItemTutorial : TutorialPopup
    {
        void Update()
        {
            if (Input.GetKey(KeyCode.Q)) // TODO: remove this as there will be no dropable items in the game
            {
                Complete();
            }
        }
    }
}
