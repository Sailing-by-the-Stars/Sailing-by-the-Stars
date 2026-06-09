using UnityEngine;
using UnityEngine.UI;

namespace Assets._Teams.Island_1.Scripts.User_Interface
{
    // Author: Sander Kleine
    public class RotateAstrolabe : TutorialPopup
    {
        [Header("Requirements")]
        [SerializeField] private float requiredScrollDistance = 100f;

        private float scrollDistance = 0f;

        void Update()
        {
            scrollDistance += Mathf.Abs(Input.mouseScrollDelta.y);

            if (scrollDistance > requiredScrollDistance)
            {
                Complete();
            }
        }
    }
}
