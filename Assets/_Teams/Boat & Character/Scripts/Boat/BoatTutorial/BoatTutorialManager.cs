// Created by Jantina
using UnityEngine;
using Assets._Teams.Island_1.Scripts.User_Interface;
using System.Collections;

public class BoatTutorialManager : MonoBehaviour
{
    public enum SubStep { Approach, Action, Leave }
    private TutorialStep _currentMajorStep = TutorialStep.None;
    private SubStep _currentSubStep;
    private bool _tutorialActive = false;
    private BoatHighlighter anchorHighlighter;
    private BoatHighlighter rudderHighlighter;
    private BoatHighlighter sailHighlighter;
    private BoatController _boatController;
    private DockInteractionUI _dockUI;
    private int _simpleTutorialStep = 0;
    bool tutorialComplete = false;

    private void Awake()
    {
        _boatController = GetComponent<BoatController>();
        foreach (var h in transform.root.GetComponentsInChildren<BoatHighlighter>())
        {
            switch (h.partType)
            {
                case BoatPartType.Anchor: anchorHighlighter = h; break;
                case BoatPartType.Rudder: rudderHighlighter = h; break;
                case BoatPartType.Sail:   sailHighlighter   = h; break;
            }
        }

        if (anchorHighlighter == null) Debug.LogWarning("[BoatTutorialManager] No Anchor highlighter found.");
        if (rudderHighlighter == null) Debug.LogWarning("[BoatTutorialManager] No Rudder highlighter found.");
        if (sailHighlighter   == null) Debug.LogWarning("[BoatTutorialManager] No Sail highlighter found.");

        _dockUI = FindFirstObjectByType<DockInteractionUI>(FindObjectsInactive.Include);
    }

    public void OnPlayerBoarded()
    {
        if (_tutorialActive) return;
        _tutorialActive = true;
        TutorialManager.Instance?.gameObject.SetActive(false);
        _dockUI?.Suppress(true);
        if (!tutorialComplete)
        {
            if (_boatController.IsSimpleModeEnabled)
                StartSimpleTutorial();
            else
                StartAnchorApproach();
        }
    }


    private void StartAnchorApproach()
    {
        _currentMajorStep = TutorialStep.HaulAnchor;
        _currentSubStep   = SubStep.Approach;
        anchorHighlighter?.Highlight();
        BoatTutorialUI.Instance?.ShowInteractPrompt("crank on your right");
    }

    public void NotifyEnteredAnchor()
    {
        if (_currentMajorStep != TutorialStep.HaulAnchor || _currentSubStep != SubStep.Approach) return;
        _currentSubStep = SubStep.Action;
        anchorHighlighter?.Unhighlight();
        BoatTutorialUI.Instance?.ShowHaulAnchor();
    }

    public void NotifyAnchorHauled()
    {
        if (_currentMajorStep != TutorialStep.HaulAnchor || _currentSubStep != SubStep.Action) return;
        _currentSubStep = SubStep.Leave;
        BoatTutorialUI.Instance?.ShowLeavePrompt();
    }

    public void NotifyLeftAnchor()
    {
        if (_currentMajorStep != TutorialStep.HaulAnchor || _currentSubStep != SubStep.Leave) return;
        StartRudderApproach();
    }


    private void StartRudderApproach()
    {
        _currentMajorStep = TutorialStep.SteerRudder;
        _currentSubStep   = SubStep.Approach;
        rudderHighlighter?.Highlight();
        BoatTutorialUI.Instance?.ShowInteractPrompt("rudder behind you");
    }

    public void NotifyEnteredRudder()
    {
        if (_currentMajorStep != TutorialStep.SteerRudder || _currentSubStep != SubStep.Approach) return;
        _currentSubStep = SubStep.Action;
        rudderHighlighter?.Unhighlight();
        BoatTutorialUI.Instance?.ShowSteerRudder();
    }

    public void NotifyLeftRudder()
    {
        if (_currentMajorStep != TutorialStep.SteerRudder || _currentSubStep != SubStep.Leave) return;
        StartSailApproach();
    }


    private void StartSailApproach()
    {
        _currentMajorStep = TutorialStep.AdjustSail;
        _currentSubStep   = SubStep.Approach;
        sailHighlighter?.Highlight();
        BoatTutorialUI.Instance?.ShowInteractPrompt("crank on your left");
    }

    public void NotifyEnteredSail()
    {
        if (_currentMajorStep != TutorialStep.AdjustSail || _currentSubStep != SubStep.Approach) return;
        _currentSubStep = SubStep.Action;
        sailHighlighter?.Unhighlight();
        BoatTutorialUI.Instance?.ShowAdjustSail();
    }

    public void NotifySailAdjusted()
    {
        if (_currentMajorStep != TutorialStep.AdjustSail || _currentSubStep != SubStep.Action) return;
        _currentSubStep = SubStep.Leave;
        BoatTutorialUI.Instance?.ShowLeavePrompt();
    }

    public void NotifyLeftSail()
    {
        if (_currentMajorStep != TutorialStep.AdjustSail || _currentSubStep != SubStep.Leave) return;
        CompleteTutorial();
    }

    public void OnPlayerDisembarked()
    {
        if (!_tutorialActive) return;
        _tutorialActive = false;
        _currentMajorStep = TutorialStep.None;
        anchorHighlighter?.Unhighlight();
        rudderHighlighter?.Unhighlight();
        sailHighlighter?.Unhighlight();
        BoatTutorialUI.Instance?.Hide();
        _dockUI?.Suppress(false);
        TutorialManager.Instance?.gameObject.SetActive(true);
    }
    private void CompleteTutorial()
    {
        _currentMajorStep = TutorialStep.None;
        _tutorialActive   = false;
        BoatTutorialUI.Instance?.Hide();
        _dockUI?.Suppress(false);  
        tutorialComplete = true;
        TutorialManager.Instance?.gameObject.SetActive(true);
    }

    public bool IsTutorialActive => _tutorialActive;
    private void StartSimpleTutorial()
    {

        _simpleTutorialStep = 0;

        _dockUI?.Suppress(true);

        TutorialManager.Instance?.gameObject.SetActive(false);

        BoatTutorialUI.Instance?.ShowSimpleThrottle();
    }

    public void NotifyRudderSteered()
    {
        if (!_boatController.IsSimpleModeEnabled)
            return;

        if (_simpleTutorialStep != 1)
            return;

        Debug.Log("Simple tutorial completed");

        CompleteTutorial();
    }
    public void NotifyRudderSteeredComplex()
    {
        if (_boatController.IsSimpleModeEnabled)
            return;

        if (_currentMajorStep != TutorialStep.SteerRudder)
            return;

        if (_currentSubStep != SubStep.Action)
            return;

        _currentSubStep = SubStep.Leave;

        BoatTutorialUI.Instance?.ShowLeavePrompt();
    }
    public void NotifyThrottleUsed()
    {
        if (!_boatController.IsSimpleModeEnabled)
            return;

        if (_simpleTutorialStep != 0)
            return;

        _simpleTutorialStep = 1;

        BoatTutorialUI.Instance?.ShowSimpleTurn();
    }
}