using UnityEngine;
using UnityEngine.UI;

namespace Assets._Teams.Island_1.Scripts.User_Interface
{
    // Author: Sander Kleine
    public class RotateAstrolabe : TutorialPopup
    {
        [Header("Requirements")]
        [SerializeField] private float requiredScrollDistance = 100f;

        [Header("References")]
        [SerializeField] private Image progressbarMask;

        private float scrollDistance = 0f;
        private Vector3 referenceVector = new(0, 0, 0);

        void Update()
        {
            scrollDistance += Mathf.Abs(Input.mouseScrollDelta.y);
            SetMaskFill();

            if (scrollDistance > requiredScrollDistance)
            {
                Complete();
            }
        }

        void SetMaskFill()
        {
            float fillAmount = (float)scrollDistance / (float)requiredScrollDistance;
            progressbarMask.fillAmount = fillAmount;
        }
    }
}
