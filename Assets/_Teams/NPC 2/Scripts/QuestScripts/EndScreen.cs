using System.Collections;
using UnityEngine;

// Programmer: Arch

public class EndScreen : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private GameObject endScreenPanel;
    [SerializeField] private CanvasGroup endScreenCanvasGroup;
    [SerializeField] private float fadeDuration = 2f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem starParticles;

    private bool questsStarted = false;

    private void Awake()
    {
        // hide end screen at start
        if (endScreenPanel != null)
        {
            endScreenPanel.SetActive(false);
        }
        
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

        // if quests were started and now the list is empty, turn on end screen and fade
        if (questsStarted && currentQuestCount == 0)
        {
            TriggerEndScreen();
            
            // stop update from calling trigger multiple times
            questsStarted = false; 
        }
    }

    /// <summary>
    /// Activates the end screen panel and starts visual effects.
    /// </summary>
    public void TriggerEndScreen()
    {
        if (endScreenPanel != null && !endScreenPanel.activeSelf)
        {
            endScreenPanel.SetActive(true);
            
            if (starParticles != null)
            {
                starParticles.Play();
            }
            
            StartCoroutine(FadeInScreen());
        }
    }

    /// <summary>
    /// Fades the canvas group over time.
    /// </summary>
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
    }
}