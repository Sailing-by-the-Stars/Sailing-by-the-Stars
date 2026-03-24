using UnityEngine;
using UnityEngine.UI;

namespace Assets._Teams.Island_1.Scripts.User_Interface
{
    // Author: Sander Kleine
    public class MoveTutorial : TutorialPopup
    {
        [Header("Requirements")]
        [SerializeField] private float requiredTimeMovementKeysPressedMs = 1000f;

        [Header("References")]
        [SerializeField] private Image progressbarMask;

        private float timeMovementKeyPressed = 0f;

        void Update()
        {
            if (Input.GetButton("Horizontal") || Input.GetButton("Vertical"))
            {
                timeMovementKeyPressed += Time.deltaTime * 1000;
            }

            SetMaskFill();

            if (timeMovementKeyPressed > requiredTimeMovementKeysPressedMs)
            {
                Complete();
            }
        }

        void SetMaskFill()
        {
            float fillAmount = (float)timeMovementKeyPressed / (float)requiredTimeMovementKeysPressedMs;
            progressbarMask.fillAmount = fillAmount;
        }
    }
}
