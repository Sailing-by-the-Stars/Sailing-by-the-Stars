using System.Collections;
using UnityEngine;

public class ShowJournalUpdateNotif : MonoBehaviour
{
    public void Show()
    {
        gameObject.SetActive(true);
        StartCoroutine(ShowCoroutine());
    }

    IEnumerator ShowCoroutine()
    {
        //Wait for the animation to finish
        yield return new WaitForSeconds(6);
        gameObject.SetActive(false);
    }
}
