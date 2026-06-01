using System;
using System.Collections;
using UnityEngine;

public class PShadowPillarButton : MonoBehaviour, IInteractable
{
    private PShadowGridManager gridManager;

    [Header("Grid Settings")]
    [SerializeField] private bool isRowButton;
    [Tooltip("Rows: -1 = left, 1 = right | Columns: -1 = down, 1 = up")]
    [SerializeField] private int direction;
    [Tooltip("Which row/column this button affects.")]
    [SerializeField] private int index;
    
    [Tooltip("If this is true, all other settings are irrelevant!")]
    [SerializeField] private bool isResetButton;
    
    [Header("Button Sliding")]
    [SerializeField] private Transform buttonMesh;
    [SerializeField] private Vector3 localPressDirection = Vector3.up;
    [SerializeField] private float pressDistance = 0.05f;
    [SerializeField] private float pressTime = 0.05f;
    [SerializeField] private float releaseTime = 0.08f;

    private Vector3 restLocalPos;
    private Vector3 pressedLocalPos;
    private Coroutine animRoutine;
    private bool isPressed;
    
    [SerializeField] private string objectInteractMessage = "Press E to Push Pillar(s)";
    public string InteractMessage => objectInteractMessage;

    private void Awake()
    {
        restLocalPos = buttonMesh.localPosition;
        pressedLocalPos = restLocalPos + localPressDirection.normalized * pressDistance;
    }

    private void Start()
    {
        if (gridManager == null)
        {
            gridManager = FindFirstObjectByType<PShadowGridManager>();
        }
        if (gridManager == null)
        {
            Debug.LogError($"{name} could not find the PShadowGridManager!");
        }
    }
    
    public void Interact(InteractionController interactionController)
    {
        if (isResetButton)
        {
            gridManager.ResetPillars();
            return;
        }

        gridManager.TryPushLine(isRowButton, index, direction);
        PressButton();
    }
    
    private void PressButton()
    {
        if (isPressed) return;

        isPressed = true;

        if (animRoutine != null)
            StopCoroutine(animRoutine);

        animRoutine = StartCoroutine(MoveButton(restLocalPos, pressedLocalPos, pressTime, () =>
        {
            // Optional tiny hold so it feels tactile
            StartCoroutine(ReturnAfterDelay(0.03f));
        }));
    }

    private IEnumerator ReturnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (animRoutine != null)
            StopCoroutine(animRoutine);

        animRoutine = StartCoroutine(MoveButton(pressedLocalPos, restLocalPos, releaseTime, () =>
        {
            isPressed = false;
        }));
    }

    private IEnumerator MoveButton(Vector3 from, Vector3 to, float duration, Action OnComplete = null)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, duration);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            buttonMesh.localPosition = Vector3.LerpUnclamped(from, to, eased);
            yield return null;
        }

        buttonMesh.localPosition = to;
        OnComplete?.Invoke();
    }
}