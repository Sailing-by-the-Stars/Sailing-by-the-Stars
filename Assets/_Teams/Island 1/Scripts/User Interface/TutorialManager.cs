using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Author: Sander Kleine
namespace Assets._Teams.Island_1.Scripts.User_Interface
{
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance;

        [Header("Display Settings")]
        [SerializeField] float delay = 2f;

        private Dictionary<TutorialStep, TutorialPopup> popups;
        private TutorialPopup currentPopup;
        private TutorialStep currentStep;
        private readonly Queue<TutorialStep> stepQueue = new();
        private bool isBusy = false;

        private HashSet<TutorialStep> completedSteps = new();

        public bool IsStepActive => currentPopup != null && currentPopup.gameObject.activeSelf;

        private static WaitForSecondsRealtime _waitForSecondsRealtime;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            popups = new Dictionary<TutorialStep, TutorialPopup>();

            _waitForSecondsRealtime = new WaitForSecondsRealtime(delay);

            foreach (var popup in GetComponentsInChildren<TutorialPopup>(true))
            {
                popups.Add(popup.Step, popup);
                popup.gameObject.SetActive(false);
            }

            StartCoroutine(StartWithDelay());
        }

        private IEnumerator StartWithDelay()
        {
            yield return _waitForSecondsRealtime;
            EnqueueStep(TutorialStep.LookAround);
            EnqueueStep(TutorialStep.Move);
        }

        private void ShowStep(TutorialStep step)
        {
            isBusy = true;

            currentStep = step;
            currentPopup = popups[step];
            currentPopup.gameObject.SetActive(true);
        }

        public void EnqueueStep(TutorialStep step)
        {
            if (completedSteps.Contains(step))
                return;

            if (stepQueue.Contains(step) || step == currentStep)
                return;

            stepQueue.Enqueue(step);

            TryProcessQueue();
        }

        public void EnqueueSteps(TutorialStep[] steps)
        {
            foreach (var step in steps)
            {
                EnqueueStep(step);
            }
        }

        private void TryProcessQueue()
        {
            if (isBusy) return;
            if (stepQueue.Count == 0) return;

            TutorialStep nextStep = stepQueue.Dequeue();
            ShowStep(nextStep);
        }

        public void CompleteStep(TutorialStep step)
        {
            if (step != currentStep) return;

            completedSteps.Add(step);

            currentPopup.gameObject.SetActive(false);
            currentPopup = null;

            StartCoroutine(DelayThenContinue());
        }

        private IEnumerator DelayThenContinue()
        {
            yield return _waitForSecondsRealtime;

            isBusy = false;
            TryProcessQueue();
        }
    }
}