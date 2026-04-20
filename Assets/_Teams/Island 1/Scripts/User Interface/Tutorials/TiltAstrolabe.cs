using UnityEngine;

namespace Assets._Teams.Island_1.Scripts.User_Interface.Tutorials
{
    public class TiltAstrolabe : TutorialPopup
    {
        void Update()
        {
            if (Input.GetMouseButton(0))
            {
                Complete();
            }
        }
    }
}
