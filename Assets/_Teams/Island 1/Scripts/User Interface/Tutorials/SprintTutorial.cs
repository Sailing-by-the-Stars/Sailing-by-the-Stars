using UnityEngine;
using UnityEngine.UI;

namespace Assets._Teams.Island_1.Scripts.User_Interface
{
    // Author: Sander Kleine
    public class SprintTutorial : TutorialPopup
    {
        [Header("Requirements")]
        [SerializeField] private float requiredTimeSprintKeyPressedMs = 1000f;

        [Header("References")]
        [SerializeField] private Image progressbarMask;

        private float timeSprintKeyPressed = 0f;

        void Update()
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                timeSprintKeyPressed += Time.deltaTime * 1000;
            }

            SetMaskFill();

            if (timeSprintKeyPressed > requiredTimeSprintKeyPressedMs)
            {
                Complete();
            }
        }

        void SetMaskFill()
        {
            float fillAmount = (float)timeSprintKeyPressed / (float)requiredTimeSprintKeyPressedMs;
            progressbarMask.fillAmount = fillAmount;
        }
    }
}
