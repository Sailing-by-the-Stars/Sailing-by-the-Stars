using Assets.Team_Island_1.Scripts.User_Interface;
using System.Collections;
using UnityEngine;

public class TutorialPopupManager : MonoBehaviour
{
    [Header("Display Settings")]
    [SerializeField] float timeBetweenPopups = 2f;

    private static WaitForSecondsRealtime _waitForSeconds;
    private TutorialPopup[] popupScripts;
    private int popupScriptIndex = 0;


    void Start()
    {
        _waitForSeconds = new WaitForSecondsRealtime(timeBetweenPopups);

        popupScripts = GetComponentsInChildren<TutorialPopup>(true);

        foreach (TutorialPopup popupScript in popupScripts)
        {
            popupScript.DoneEvent.AddListener(PopupIsDone);
            popupScript.gameObject.SetActive(false);
        }

        StartCoroutine(DisplayNextPopup());
    }

    void PopupIsDone()
    {
        StartCoroutine(DisplayNextPopup());
    }

    IEnumerator DisplayNextPopup()
    {
        yield return _waitForSeconds;

        if (popupScriptIndex < popupScripts.Length)
        {
            popupScripts[popupScriptIndex].gameObject.SetActive(true);
            popupScriptIndex++;
        }
    }
}
