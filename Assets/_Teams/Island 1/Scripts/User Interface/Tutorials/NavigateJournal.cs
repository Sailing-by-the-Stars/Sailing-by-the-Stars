using UnityEngine;
using UnityEngine.UI;

namespace Assets._Teams.Island_1.Scripts.User_Interface.Tutorials
{
    public class NavigateJournal : TutorialPopup
    {
        [Header("References")]
        [SerializeField] private Image progressbarMask;

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

            SetMaskFill();

            if (EPressed && QPressed)
            {
                Complete();
            }
        }

        void SetMaskFill()
        {
            float fillAmount = (EPressed ? 0.5f : 0.0f) + (QPressed ? 0.5f : 0.0f);
            progressbarMask.fillAmount = fillAmount;
        }
    }
}
