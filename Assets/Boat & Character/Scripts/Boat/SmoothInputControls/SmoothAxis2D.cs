/*
*   Created by Johan Beimers
*   Contributed to by: 
*/

using UnityEngine;
using UnityEngine.InputSystem;

public class SmoothAxis2D : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference positiveButton;
    [SerializeField] private InputActionReference negativeButton;

    [Header("Movement Settings")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float minValue = -1f;
    [SerializeField] private float maxValue = 1f;

    [Header("Current State")]
    [SerializeField] private float currentValue = 0f;
    private float targetValue = 0f;
    private bool PositivePressed = false;
    private bool NegativePressed = false;

    void OnEnable()
    {
        positiveButton.action.Enable();
        negativeButton.action.Enable();

        positiveButton.action.performed += OnPositivePressed;
        positiveButton.action.canceled += OnPositiveReleased;
        negativeButton.action.performed += OnNegativePressed;
        negativeButton.action.canceled += OnNegativeReleased;
    }

    void OnDisable()
    {
        positiveButton.action.performed -= OnPositivePressed;
        positiveButton.action.canceled -= OnPositiveReleased;
        negativeButton.action.performed -= OnNegativePressed;
        negativeButton.action.canceled -= OnNegativeReleased;

        positiveButton.action.Disable();
        negativeButton.action.Disable();
    }

    private void OnPositivePressed(InputAction.CallbackContext ctx)
    {
        PositivePressed = true;
        UpdateTargetValue();
    }

    private void OnPositiveReleased(InputAction.CallbackContext ctx)
    {
        PositivePressed = false;
        UpdateTargetValue();
    }

    private void OnNegativePressed(InputAction.CallbackContext ctx)
    {
        NegativePressed = true;
        UpdateTargetValue();
    }

    private void OnNegativeReleased(InputAction.CallbackContext ctx)
    {
        NegativePressed = false;
        UpdateTargetValue();
    }

    private void UpdateTargetValue()
    {
        if (PositivePressed && !NegativePressed)
            targetValue = maxValue;
        else if (NegativePressed && !PositivePressed)
            targetValue = minValue;
        else if (PositivePressed && NegativePressed)
            targetValue = (minValue + maxValue) / 2f;
        else
            targetValue = currentValue;
    }

    void Update()
    {
        currentValue = Mathf.MoveTowards(value, targetValue, speed * Time.deltaTime);
    }

    public float value  => currentValue;
}