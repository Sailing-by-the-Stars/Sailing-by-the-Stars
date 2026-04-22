using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class astroDialogue
{
    public string text = "";
    public bool waitAtLineEnd = false;
    public bool waitBeforeLineEnd = false;
    public int lineToGoToNext = -1;
    public UnityEvent startOfLine;
    public UnityEvent endOfLine;
}


public class AstroDialogue : MonoBehaviour
{
    public static AstroDialogue currentDialogue;

    public bool inDialogue;
    public float textSpeed;

    [SerializeField] 
    protected int index = 0;

    private string line;
    private bool inTyping;

    public UnityEvent enterDialogue;
    public UnityEvent exitDialogue;

    [SerializeField] protected List<astroDialogue> dialogue = new();

    protected virtual void Start()
    {
        TurnOffTextBoxes();
    }


    protected virtual void Update()
    {
        if (inDialogue)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!inTyping)
                {
                    if (dialogue[index].waitAtLineEnd == false)
                    {
                        if (NextLine())
                        {
                            return;
                        }
                    }

                    if (!dialogue[index].waitBeforeLineEnd)
                    {
                        TurnOffTextBoxes();
                    }
                    else
                    {
                        WaitFlag();
                    }
                }
                else
                {
                    StopAllCoroutines();
                    endOfLine(index);
                    inTyping = false;
                    SetText(dialogue[index].text);
                }
            }
        }
    }

    public virtual void WaitFlag()
    {

    }

    protected virtual void SetText(string text)
    {
        foreach (DialogueTextBox textBox in DialogueTextBox.dialogueTextBoxes)
        {
            textBox.TurnOn();
            textBox.textBox.text = text;
        }
    }

    protected virtual void TurnOffTextBoxes()
    {
        foreach (DialogueTextBox textBox in DialogueTextBox.dialogueTextBoxes)
        {
            textBox.TurnOff();
        }
    }


    public virtual void StartDialogue(int Dindex = 0)
    {
        if(Dindex < 0)
        {
            Dindex = 0;
        }

        if (currentDialogue == null)
        {
            currentDialogue = this;


        }
        else if (currentDialogue == this)
        {
            Debug.LogError("this dialogue had already started!");
        }
        else
        {
            Debug.LogError("tried starting another dialogue instance");
            return;
        }

        enterDialogue.Invoke();
        inDialogue = true;
        index = Dindex;
        typeLine(Dindex);
    }

    public virtual void endDialogue()
    {
        if(currentDialogue == this)
        {
            currentDialogue = null;
        }
        else
        {
            Debug.LogError("this wasn't the main dialogue for some reason!");
        }

        TurnOffTextBoxes();
        StopAllCoroutines();
        exitDialogue.Invoke();
        inDialogue = false;
    }

    public virtual void typeLine(int indexOfLine)
    {
        if (indexOfLine >= 0)
        {
            dialogue[indexOfLine].startOfLine.Invoke();
            StopAllCoroutines();
            index = indexOfLine;
            StartCoroutine(typeOutLine(dialogue[indexOfLine].text, indexOfLine));
        }
        else
        {
            endDialogue();
        }
    }

    protected virtual bool NextLine(int Dindex = -1)
    {
        if (Dindex >= 0)
        {
            index = Dindex;
        }
        else
        {
            index++;
            Dindex = index;
        }

        if(Dindex < dialogue.Count)
        {
            typeLine(Dindex);
            return true;
        }

        return false;
    }

    protected virtual void endOfLine(int indexOfLine = -1)
    {
        if (indexOfLine >= 0)
        {
            dialogue[indexOfLine].endOfLine.Invoke();
        }
    }


    protected virtual IEnumerator typeOutLine(string lineToType, int indexOfLine = -1)
    {
        inTyping = true;
        SetText(string.Empty);
        line = string.Empty;
        foreach (char character in lineToType.ToCharArray())
        {
            line += character;
            SetText(line);
            yield return new WaitForSeconds(textSpeed);
        }
        inTyping = false;
        endOfLine(indexOfLine);
    }
}
