// Programmer: Arch

using UnityEngine;

/// <summary>
/// Plays the guiding sparkle sequence for the Bargaining puzzle, reacting to the
/// weight of the item picked up or placed.
///
/// Sequence:
///   Step 0 - Sparkle on a non-heaviest item
///   Step 1 - Pick up a non-heaviest item   -> sparkle on scale
///   Step 2 - Item placed on scale          -> sparkle on heaviest item
///   Step 3 - Pick up the heaviest item     -> sparkle on scale
///   Step 4 - Heaviest placed on scale      -> sparkle on heaviest item
///   Step 5 - Pick up the heaviest item     -> arrow on altar
///   Step 6 - Item placed on correct pedestal -> hints stop
/// </summary>
public class BargainingPuzzleHintManager : MonoBehaviour
{
    [Header("Heaviest Item")]
    [Tooltip("The weight value of the heaviest item.")]
    [SerializeField] private int heaviestItemWeight = 5;

    [Header("Non-Heaviest Item Hint")]
    [SerializeField] private GuidingSparklePoint nonHeaviestItemSparkle;

    [Header("Scale Hint")]
    [SerializeField] private GuidingSparklePoint scaleSparkle;

    [Header("Heaviest Item Hint")]
    [SerializeField] private GuidingSparklePoint heaviestItemSparkle;

    [Header("Altar Indicator Hint")]
    [SerializeField] private GuidingSparklePoint altarIndicatorSparkle;
    [Tooltip("Arrow indicator object shown and hidden with the altar hint.")]
    [SerializeField] private GameObject altarArrowObject;

    [Header("Correct Placement Check")]
    [Tooltip("The heaviest item's transform.")]
    [SerializeField] private Transform heaviestItemTransform;
    [Tooltip("The place point of the correct pedestal for the heaviest item.")]
    [SerializeField] private Transform correctPedestalSlot;

    private int hintStep = 0;
    private bool hintsComplete = false;

    private void OnEnable()
    {
        GameEvents.OnPickup += HandlePickup;
        GameEvents.OnScaleItemPlaced += HandleScaleItemPlaced;
        GameEvents.OnAltarItemPlaced += HandleAltarItemPlaced;
    }

    private void OnDisable()
    {
        GameEvents.OnPickup -= HandlePickup;
        GameEvents.OnScaleItemPlaced -= HandleScaleItemPlaced;
        GameEvents.OnAltarItemPlaced -= HandleAltarItemPlaced;
    }

    private void Start()
    {
        nonHeaviestItemSparkle.ShowSparkle();

        if (altarArrowObject != null)
            altarArrowObject.SetActive(false);
    }

    private void HandlePickup(IPickup item)
    {
        if (hintsComplete) return;

        if (hintStep == 0 && IsNonHeaviestBargainItem(item))
        {
            nonHeaviestItemSparkle.HideSparkle();
            scaleSparkle.ShowSparkle();
            hintStep = 1;
        }
        else if (hintStep == 2 && IsHeaviestBargainItem(item))
        {
            heaviestItemSparkle.HideSparkle();
            scaleSparkle.ShowSparkle();
            hintStep = 3;
        }
        else if (hintStep == 4 && IsHeaviestBargainItem(item))
        {
            heaviestItemSparkle.HideSparkle();
            if (altarArrowObject != null)
                altarArrowObject.SetActive(true);
            if (altarIndicatorSparkle != null)
                altarIndicatorSparkle.ShowSparkle();
            hintStep = 5;
        }
    }

    private void HandleScaleItemPlaced(IPickup item)
    {
        if (hintsComplete) return;

        if (hintStep == 1 && IsNonHeaviestBargainItem(item))
        {
            scaleSparkle.HideSparkle();
            ShowHeaviestSparkle();
            hintStep = 2;
        }
        else if (hintStep == 3 && IsHeaviestBargainItem(item))
        {
            scaleSparkle.HideSparkle();
            ShowHeaviestSparkle();
            hintStep = 4;
        }
    }

    private void HandleAltarItemPlaced()
    {
        if (hintsComplete) return;
        if (hintStep != 5) return;
        if (!IsHeaviestOnCorrectPedestal()) return;

        if (altarIndicatorSparkle != null)
            altarIndicatorSparkle.HideSparkle();
        if (altarArrowObject != null)
            altarArrowObject.SetActive(false);

        hintsComplete = true;
    }

    // Places the heaviest sparkle on the item's current position, then shows it.
    private void ShowHeaviestSparkle()
    {
        if (heaviestItemTransform != null)
            heaviestItemSparkle.transform.position = heaviestItemTransform.position;

        heaviestItemSparkle.ShowSparkle();
    }

    private bool IsHeaviestOnCorrectPedestal()
    {
        if (heaviestItemTransform == null || correctPedestalSlot == null) return false;

        return heaviestItemTransform.IsChildOf(correctPedestalSlot);
    }

    private bool IsHeaviestBargainItem(IPickup pickup)
    {
        return pickup is BargainItem item && item.weight == heaviestItemWeight;
    }

    private bool IsNonHeaviestBargainItem(IPickup pickup)
    {
        return pickup is BargainItem item && item.weight != heaviestItemWeight;
    }
}
