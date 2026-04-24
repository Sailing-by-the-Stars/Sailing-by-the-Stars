using UnityEngine;
using System.Collections;
using TMPro;

public class NPCTextBubble : MonoBehaviour
{
    [Header("Text Bubble")]
    [SerializeField] private GameObject bubbleObject;
    [SerializeField] private TextMeshPro bubbleText;
    
    [Header("Display Settings")]
    [SerializeField] private float displayDuration = 3f;

    private Coroutine hideCoroutine;

    private void Start()
    {
        bubbleObject.SetActive(false);
    }

    public void ShowText(string message)
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }
        
        bubbleText.text = message;
        bubbleObject.SetActive(true);

        hideCoroutine = StartCoroutine(HideAfterDuration());
    }

    public void HideText()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }
        
        bubbleObject.SetActive(false);
    }

    private IEnumerator HideAfterDuration()
    {
        yield return new WaitForSeconds(displayDuration);
        bubbleObject.SetActive(false);
    }
}
