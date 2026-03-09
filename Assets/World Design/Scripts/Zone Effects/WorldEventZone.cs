using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class WorldEventZone : MonoBehaviour
{
    [Header("Trigger settings")]
    [Tooltip("Time before exit effects trigger to prevent flickering at borders.")]
    [SerializeField] private float exitDelay = 0.25f;
    [Tooltip("Tag of the object that triggers this zone.")]
    [SerializeField] private string instigatorTag = "boat";

    private bool isInside = false; // logical inside state of instigator
    private Coroutine exitCoroutine; // Track effect coroutines for cancellation
    private IZoneEffect[] effects;
    // and what about exit events?
    // but what about our start effects tracking?
    //private List<Coroutine> runningCoroutines = new();

    private void Awake()
    {
        effects = GetComponentsInChildren<IZoneEffect>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(instigatorTag) || isInside)
        {
            return;
        }
        // Cancel any pending exit action
        if (exitCoroutine != null)
        {
            StopCoroutine(exitCoroutine);
            exitCoroutine = null;
        }

        isInside = true;

        foreach (IZoneEffect effect in effects)
        {
            effect.OnEnter(other.gameObject);
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(instigatorTag) || !isInside)
        {
            return;
        }
        isInside = false;
        // trigger exit on a delay to avoid rapid enter/exit at edges
        exitCoroutine = StartCoroutine(DelayedExit(other.gameObject));
    }
    IEnumerator DelayedExit(GameObject instigator)
    {
        yield return new WaitForSeconds(exitDelay);

        // If the player re-entered during delay, cancel exit
        if (isInside)
        {
            yield break;
        }
        foreach (IZoneEffect effect in effects)
        {
            effect.OnExit(instigator);
        }
        exitCoroutine = null;
    }
}