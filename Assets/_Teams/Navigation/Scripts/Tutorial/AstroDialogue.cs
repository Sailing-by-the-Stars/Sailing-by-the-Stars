using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class astroDialogue
{
    public string text = "";
    public List<GameObject> manualUI = new();
    public bool waitAtLineEnd = false;
    public bool waitForTutorial = false;
    public float timeTillEndOfline = 2.5f;
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

    private bool inTyping;

    public UnityEvent enterDialogue;
    public UnityEvent exitDialogue;

    public bool waitFlag = false;

    public List<astroDialogue> dialogue = new();

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
                if (index < dialogue.Count)
                {
                    if (inTyping && !dialogue[index].waitForTutorial)
                    {
                        inTyping = false;
                        return;
                    }
            
            
                
                    if (dialogue[index].waitForTutorial == true)
                    {
                        WaitFlag();
                        return;
                    }
            
                    if (dialogue[index].waitAtLineEnd == true)
                    {
                        if (NextLine())
                        {
                            return;
                        }
                    }
                }
                else
                {
                    endDialogue();
                }
            }
        }
    }

    public virtual void WaitFlag()
    {
        waitFlag = true;
    }

    protected virtual void SetText(string text)
    {
        foreach (DialogueTextBox textBox in DialogueTextBox.dialogueTextBoxes)
        {
            if (!string.IsNullOrEmpty(text))
            {
                textBox.TurnOn();
                textBox.textBox.text = text;
            }
            else
            {
                textBox.TurnOff();
            }
        }
    }

    protected virtual void TurnOffTextBoxes()
    {
        foreach (DialogueTextBox textBox in DialogueTextBox.dialogueTextBoxes)
        {
            textBox.TurnOff();
        }

        foreach (astroDialogue aDialogue in dialogue)
        {
            foreach (GameObject obj in aDialogue.manualUI)
            {
                obj.SetActive(false);
            }
        }

        currentDialogue = null;
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
        showLine(Dindex);
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

    public virtual void showLine(int indexOfLine)
    {
        if (indexOfLine >= 0)
        {
            dialogue[indexOfLine].startOfLine.Invoke();
            inTyping = false;
            StopAllCoroutines();
            _coroutineGeneration++; // invalidate any still-running coroutine tails
            waitFlag = false;
            index = indexOfLine;
            StartCoroutine(typeOutLine(dialogue[indexOfLine], indexOfLine));
        }
        else
        {
            endDialogue();
        }
    }

    protected virtual bool NextLine(int Dindex = -1)
    {
        waitFlag = false;

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
            showLine(Dindex);
            return true;
        }

        //endDialogue();
        return false;
    }

    protected virtual void endOfLine(int indexOfLine = -1)
    {
        if (indexOfLine >= 0)
        {
            dialogue[indexOfLine].endOfLine.Invoke();
        }
        if (dialogue[indexOfLine].waitForTutorial )
        {
            WaitFlag();
        }
        else
        {
            foreach (GameObject obj in dialogue[indexOfLine].manualUI)
            {
                obj.SetActive(false);
            }
        }
    }


    // In AstroDialogue:
    private int _coroutineGeneration = 0;

    protected virtual IEnumerator typeOutLine(astroDialogue lineToShow, int indexOfLine = -1)
    {
        int myGeneration = ++_coroutineGeneration;
        float T = 0;

        inTyping = true;
        foreach (GameObject obj in lineToShow.manualUI)
            obj.SetActive(true);

        SetText(lineToShow.text);

        while (inTyping)
        {
            T += Time.deltaTime;
            if (lineToShow.timeTillEndOfline < T)
            {
                if (!dialogue[indexOfLine].waitAtLineEnd)
                    inTyping = false;
            }
            yield return null;
        }

        endOfLine(indexOfLine);

        // If endOfLine triggered a new coroutine (via WaitFlag ? TryGoNextline ? showLine),
        // that call already incremented _coroutineGeneration. Bail out.
        if (myGeneration != _coroutineGeneration)
            yield break;

        inTyping = false;
        if (!dialogue[indexOfLine].waitAtLineEnd && !dialogue[indexOfLine].waitForTutorial)
        {
            NextLine(dialogue[indexOfLine].lineToGoToNext);
        }

        yield return null;
    }
}
