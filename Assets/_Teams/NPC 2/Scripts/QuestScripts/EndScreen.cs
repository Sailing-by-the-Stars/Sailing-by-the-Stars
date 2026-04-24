using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Programmer: Arch

public class EndScreen : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private GameObject endScreenPanel;
    [SerializeField] private CanvasGroup endScreenCanvasGroup;
    [SerializeField] private GameObject staticEndingText;
    [SerializeField] private float fadeDuration = 2f;

    [Header("Credits Settings")]
    [SerializeField] private RectTransform creditsTextTransform;
    [SerializeField] private float scrollSpeed = 50f;
    [SerializeField] private float startDelay = 1f;

    [Header("Blinking Stars Settings")]
    [SerializeField] private bool showBlinkingStars = true;
    [SerializeField] private int amountOfStars = 150; 

    private bool questsStarted = false;
    private PlayerState playerState;

    private void Awake()
    {
        // hide end screen at start
        if (endScreenPanel != null)
        {
            endScreenPanel.SetActive(false);
        }
        
        playerState = FindFirstObjectByType<PlayerState>();

        // set alpha to zero for fade in
        if (endScreenCanvasGroup != null)
        {
            endScreenCanvasGroup.alpha = 0f;
        }
    }

    private void Update()
    {
        // wait until quest manager is ready
        if (QuestManager.Instance == null) 
        {
            return;
        }

        int currentQuestCount = QuestManager.Instance.ActiveQuests.Count;

        // register when quests are added to the list
        if (currentQuestCount > 0 && !questsStarted)
        {
            questsStarted = true;
        }

        // edited by Boas.
        // needs to be changed to something more dynamic.

        // if quests were started and now the list is empty, turn on end screen and fade
        if (questsStarted && playerState.completedQuests.Contains("312a0283-06ac-4260-9759-999f2225e226"))
        {
            TriggerEndScreen();
            
            // stop update from calling trigger multiple times
            questsStarted = false; 
        }
    }

    public void TriggerEndScreen()
    {
        if (endScreenPanel != null && !endScreenPanel.activeSelf)
        {   
            Debug.Log("End is working");
            endScreenPanel.SetActive(true);
            
            // spawn the ui stars right away
            if (showBlinkingStars)
            {
                GenerateUIStars();
            }
            
            StartCoroutine(FadeInScreen());
        }
    }

    // spawns stars directly on the ui so they do not hide behind the black screen
    private void GenerateUIStars()
    {
        for (int i = 0; i < amountOfStars; i++)
        {
            GameObject starObj = new GameObject("UI_Star", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            starObj.transform.SetParent(endScreenPanel.transform, false);

            // make sure stars are drawn behind the text
            starObj.transform.SetAsFirstSibling();

            RectTransform rect = starObj.GetComponent<RectTransform>();
            
            // randomly spread them across a large area
            rect.anchoredPosition = new Vector2(Random.Range(-1200f, 1200f), Random.Range(-700f, 700f));
            
            // random size slightly larger than before
            float size = Random.Range(3f, 7f);
            rect.sizeDelta = new Vector2(size, size);

            Image img = starObj.GetComponent<Image>();
            
            StartCoroutine(TwinkleStar(img));
        }
    }

    private IEnumerator TwinkleStar(Image img)
    {
        // slightly faster blink rate
        float blinkSpeed = Random.Range(0.8f, 3.0f);
        float minAlpha = Random.Range(0.0f, 0.2f);
        float maxAlpha = Random.Range(0.6f, 1f);
        
        // random start time so they do not all blink together
        float timeOffset = Random.Range(0f, 10f);

        while (true)
        {
            if (img != null)
            {
                float alpha = Mathf.Lerp(minAlpha, maxAlpha, Mathf.PingPong((Time.time + timeOffset) * blinkSpeed, 1f));
                Color c = img.color;
                c.a = alpha;
                img.color = c;
            }
            yield return null;
        }
    }

    private IEnumerator FadeInScreen()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            
            if (endScreenCanvasGroup != null)
            {
                endScreenCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            }
            
            yield return null;
        }

        // wait a small moment before rolling credits
        yield return new WaitForSeconds(startDelay);

        // hide the static text so it does not overlap
        if (staticEndingText != null)
        {
            staticEndingText.SetActive(false);
        }

        // scroll the credits if assigned
        if (creditsTextTransform != null)
        {
            while (true)
            {
                creditsTextTransform.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
                yield return null;
            }
        }
    }
}