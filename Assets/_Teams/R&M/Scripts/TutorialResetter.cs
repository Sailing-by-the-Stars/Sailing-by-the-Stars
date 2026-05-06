using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UI;
using Assets._Teams.Island_1.Scripts.User_Interface;

public class TutorialResetter : MonoBehaviour
{
    public static TutorialResetter Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void ResetAll()
    {
        StartCoroutine(ResetRoutine());
    }

    private IEnumerator ResetRoutine()
    {
        Debug.Log("FULL tutorial reset starting...");
        // Team Navigation Reset 
        AstroDialogue.currentDialogue = null;

        if (TutorialSequence.Instance != null)
        {
            TutorialSequence.Instance.gameObject.SetActive(false);
        }
        // Team Island 1 Reset
        var manager = TutorialManager.Instance;

        if (manager != null)
        {
            manager.StopAllCoroutines();

            var type = manager.GetType();

            var queueField = type.GetField("stepQueue", BindingFlags.NonPublic | BindingFlags.Instance);
            if (queueField != null)
            {
                var queue = queueField.GetValue(manager);
                queue?.GetType().GetMethod("Clear")?.Invoke(queue, null);
            }

            var completedField = type.GetField("completedSteps", BindingFlags.NonPublic | BindingFlags.Instance);
            if (completedField != null)
            {
                var set = completedField.GetValue(manager);
                set?.GetType().GetMethod("Clear")?.Invoke(set, null);
            }

            type.GetField("isBusy", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(manager, false);

            type.GetField("currentPopup", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(manager, null);

            type.GetField("currentStep", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(manager, TutorialStep.None);

            var popupsField = type.GetField("popups", BindingFlags.NonPublic | BindingFlags.Instance);
            if (popupsField != null)
            {
                var dict = popupsField.GetValue(manager) as System.Collections.IDictionary;

                if (dict != null)
                {
                    foreach (var value in dict.Values)
                    {
                        var go = (value as Component)?.gameObject;
                        if (go != null)
                            go.SetActive(false);
                    }
                }
            }

            // Reset LookAround progress
            var lookAround = manager.GetComponentInChildren<LookAroundTutorial>(true);
            if (lookAround != null)
            {
                var dist = lookAround.GetType().GetField("mouseTravelDistance", BindingFlags.NonPublic | BindingFlags.Instance);
                dist?.SetValue(lookAround, 0f);

                var bar = lookAround.GetType().GetField("progressbarMask", BindingFlags.NonPublic | BindingFlags.Instance);
                var img = bar?.GetValue(lookAround) as Image;
                if (img != null) img.fillAmount = 0f;
            }

            // Reset Move progress
            var moveTutorial = manager.GetComponentInChildren<MoveTutorial>(true);
            if (moveTutorial != null)
            {
                var time = moveTutorial.GetType().GetField("timeMovementKeyPressed", BindingFlags.NonPublic | BindingFlags.Instance);
                time?.SetValue(moveTutorial, 0f);

                var bar = moveTutorial.GetType().GetField("progressbarMask", BindingFlags.NonPublic | BindingFlags.Instance);
                var img = bar?.GetValue(moveTutorial) as Image;
                if (img != null) img.fillAmount = 0f;
            }

            foreach (var trigger in FindObjectsByType<TutorialTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var triggeredField = trigger.GetType().GetField("triggered", BindingFlags.NonPublic | BindingFlags.Instance);
                triggeredField?.SetValue(trigger, false);
            }

            manager.gameObject.SetActive(false);
        }

        yield return null;

        if (TutorialSequence.Instance != null)
        {
            TutorialSequence.Instance.gameObject.SetActive(true);
        }

        if (manager != null)
        {
            manager.gameObject.SetActive(true);
            manager.EnqueueStep(TutorialStep.LookAround);
            manager.EnqueueStep(TutorialStep.Move);
        }

        TutorialSequence.finishedTutorial = false;
        TutorialSequence.startedTutorial = false;

        yield return null;

        Debug.Log("Tutorials fully reset.");

        if (TutorialSequence.Instance != null)
        {
            TutorialSequence.Instance.NextStep(0);
        }
    }
}