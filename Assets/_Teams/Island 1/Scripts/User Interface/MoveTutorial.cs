using Assets.Team_Island_1.Scripts.User_Interface;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MoveTutorial: TutorialPopup
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
            DoneEvent.Invoke();
            transform.gameObject.SetActive(false);
        }
    }

    void SetMaskFill()
    {
        float fillAmount = (float)timeMovementKeyPressed / (float)requiredTimeMovementKeysPressedMs;
        progressbarMask.fillAmount = fillAmount;
    }
}
