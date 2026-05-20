// Created by Jantina
using UnityEngine;
using Assets._Teams.Island_1.Scripts.User_Interface;
using System.Collections;

public class BoatTutorialManager : MonoBehaviour
{
    public enum SubStep { Approach, Action, Leave }

    private TutorialStep _currentMajorStep = TutorialStep.None;
    private SubStep      _currentSubStep;
    private bool         _tutorialActive = false;

    private BoatHighlighter anchorHighlighter;
    private BoatHighlighter rudderHighlighter;
    private BoatHighlighter sailHighlighter;

    // Cached so we can suppress/restore the dock UI while tutorial runs
    private DockInteractionUI _dockUI;

    private void Awake()
    {
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
        StartAnchorApproach();
    }


    private void StartAnchorApproach()
    {
        _currentMajorStep = TutorialStep.HaulAnchor;
        _currentSubStep   = SubStep.Approach;
        anchorHighlighter?.Highlight();
        BoatTutorialUI.Instance?.ShowInteractPrompt("anchor");
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
        BoatTutorialUI.Instance?.ShowInteractPrompt("rudder");
    }

    public void NotifyEnteredRudder()
    {
        if (_currentMajorStep != TutorialStep.SteerRudder || _currentSubStep != SubStep.Approach) return;
        _currentSubStep = SubStep.Action;
        rudderHighlighter?.Unhighlight();
        BoatTutorialUI.Instance?.ShowSteerRudder();
    }

    public void NotifyRudderSteered()
    {
        if (_currentMajorStep != TutorialStep.SteerRudder || _currentSubStep != SubStep.Action) return;
        _currentSubStep = SubStep.Leave;
        BoatTutorialUI.Instance?.ShowLeavePrompt();
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
        BoatTutorialUI.Instance?.ShowInteractPrompt("knot on your left");
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
        TutorialManager.Instance?.gameObject.SetActive(true);
    }

    public bool IsTutorialActive => _tutorialActive;
}