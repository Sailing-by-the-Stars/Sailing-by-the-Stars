using Assets.Team_Island_1.Scripts.User_Interface;
using System.Collections;
using UnityEngine;

public class TutorialPopupManager : MonoBehaviour
{
    private static WaitForSecondsRealtime _waitForSeconds;
    [SerializeField] float timeBetweenPopups = 2f;

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

        Debug.Log("PopusSctips: " + popupScripts.Length);
        Debug.Log("TimeScale: " + Time.timeScale);

        StartCoroutine(DisplayNextPopup());
    }

    void PopupIsDone()
    {
        Debug.Log("Popup is done");
        StartCoroutine(DisplayNextPopup());
    }

    IEnumerator DisplayNextPopup()
    {
        Debug.Log("Displaying next component");

        yield return _waitForSeconds;

        Debug.Log("Coroutine resumed");

        if (popupScriptIndex < popupScripts.Length)
        {
            popupScripts[popupScriptIndex].gameObject.SetActive(true);
            Debug.Log("Enabled popup: " + popupScriptIndex);
            popupScriptIndex++;
        }
    }
}
