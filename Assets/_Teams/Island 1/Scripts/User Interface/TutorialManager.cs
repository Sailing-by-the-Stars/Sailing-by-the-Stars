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

        [Header("References")]
        [SerializeField] private TutorialPopup pickupPopup;
        [SerializeField] private TutorialPopup usePopup;
        [SerializeField] private TutorialPopup dropPopup;

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
                if (popup != null && popup.Step != TutorialStep.None && !popups.ContainsKey(popup.Step))
                {
                    popups.Add(popup.Step, popup);
                }
                popup.gameObject.SetActive(false);
            }

            StartCoroutine(StartWithDelay());
        }

        private IEnumerator StartWithDelay()
        {
            yield return new WaitForSecondsRealtime(10f);
            EnqueueStep(TutorialStep.LookAround);
            EnqueueStep(TutorialStep.Move);
        }

        private void ShowStep(TutorialStep step)
        {
            isBusy = true;

            if (!popups.TryGetValue(step, out var popup) || popup == null)
            {
                // Auto-complete steps that have no associated popup
                completedSteps.Add(step);
                isBusy = false;
                TryProcessQueue();
                return;
            }

            currentStep = step;
            currentPopup = popup;
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

            if (completedSteps.Contains(step))
            {
                Debug.LogWarning($"this tutorialstep has already been completed: {step}");
            }
            else
            {
                completedSteps.Add(step);

                currentPopup.gameObject.SetActive(false);
                currentPopup = null;
            }


            StartCoroutine(DelayThenContinue());
        }

        private IEnumerator DelayThenContinue()
        {
            yield return _waitForSecondsRealtime;

            isBusy = false;
            TryProcessQueue();
        }

        void OnEnable()
        {
            GameEvents.OnPickup += OnTutorialItemGrab;
            GameEvents.OnUse += OnTutorialItemUse;
            GameEvents.OnDrop += OnTutorialItemDrop;
        }

        void OnDisable()
        {
            GameEvents.OnPickup -= OnTutorialItemGrab;
            GameEvents.OnUse -= OnTutorialItemUse;
            GameEvents.OnDrop -= OnTutorialItemDrop;
        }

        private void OnTutorialItemGrab(IPickup pickup)
        {
            pickupPopup.Complete();
        }

        private void OnTutorialItemUse(IPickup pickup)
        {
            usePopup.Complete();
        }

        private void OnTutorialItemDrop(IPickup pickup)
        {
            dropPopup.Complete();
        }
    }
}