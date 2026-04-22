using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueTextBox : MonoBehaviour
{
    public static List<DialogueTextBox> dialogueTextBoxes = new();

    public TMP_Text textBox;

    [SerializeField]
    GameObject visualsRoot;

    private void Awake()
    {
        textBox = GetComponent<TMP_Text>();

        if (!visualsRoot)
        {
            visualsRoot = transform.parent.gameObject;
        }

        visualsRoot.SetActive(false);

        dialogueTextBoxes.Add(this);
    }


    private void OnDestroy()
    {
        dialogueTextBoxes.Remove(this);
    }


    public void TurnOn()
    {
        visualsRoot?.SetActive(true);
    }

    public void TurnOff()
    {
        visualsRoot?.SetActive(false);
    }
}
