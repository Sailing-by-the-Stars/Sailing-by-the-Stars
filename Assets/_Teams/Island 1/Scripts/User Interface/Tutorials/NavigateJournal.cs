using UnityEngine;
using UnityEngine.UI;

namespace Assets._Teams.Island_1.Scripts.User_Interface.Tutorials
{
    public class NavigateJournal : TutorialPopup
    {
        bool EPressed = false;
        bool QPressed = false;

        void Update()
        {
            if (Input.GetKey(KeyCode.E))
            {
                EPressed = true;
            }
            if (Input.GetKey(KeyCode.Q))
            {
                QPressed = true;
            }

            if (EPressed && QPressed)
            {
                Complete();
            }
        }
    }
}
