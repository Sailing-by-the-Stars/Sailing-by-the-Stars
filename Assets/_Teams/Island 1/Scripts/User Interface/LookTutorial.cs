using Assets.Team_Island_1.Scripts.User_Interface;
using UnityEngine;
using UnityEngine.UI;

public class LookTutorial: TutorialPopup
{
    [Header("Requirements")]
    [SerializeField] private float requiredMouseTravelDistance = 1000f;
    
    [Header("References")]
    [SerializeField] private Image progressbarMask;

    private float mouseTravelDistance = 0f;
    private Vector3 referenceVector = new(0,0,0);

    void Update()
    {
        mouseTravelDistance += Vector3.Distance(Input.mousePositionDelta, referenceVector);

        SetMaskFill();

        if (mouseTravelDistance > requiredMouseTravelDistance)
        {
            DoneEvent.Invoke();
            transform.gameObject.SetActive(false);
        }
    }

    void SetMaskFill()
    {
        float fillAmount = (float)mouseTravelDistance / (float)requiredMouseTravelDistance;
        progressbarMask.fillAmount = fillAmount;
    }
}
