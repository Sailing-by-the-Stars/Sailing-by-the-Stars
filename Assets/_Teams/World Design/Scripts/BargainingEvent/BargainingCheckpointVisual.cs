/*
 * Created by Christina Pence
 * Contributed to by:
 */
using UnityEngine;
[RequireComponent (typeof(SphereCollider))]

/// <summary>
/// Optional visual effect for BargainingCheckpoint.
/// Draws stacked rings around the checkpoint that spread outward and pulse.
/// Shows when checkpoint is active, hides when reached.
///
/// SETUP:
///   Add alongside BargainingCheckpoint on the same GameObject with sphere collider.
///   Create an HDRP/Unlit material with Surface Type Transparent or Additive.
///   Assign it to ringMaterial in the inspector.
///   Set ringHeight just above water level.
/// </summary>
public class BargainingCheckpointVisual : MonoBehaviour
{
    [Header("Ring Settings")]
    [Tooltip("Number of points used to draw each ring (higher = smoother circle).")]
    [SerializeField] private int segments = 48;
    [Tooltip("Number of stacked rings (more rings = taller spread effect)")]
    [SerializeField] private int ringCount = 4;
    [Tooltip("Vertical spacing between stacked rings in world units.")]
    [SerializeField] private float ringSpacing = 0.4f;
    [Tooltip("Fixed world Y height of the lowest ring. Set just above water level.")]
    [SerializeField] private float ringHeight = 0.2f;
    [Tooltip("Width of each ring line in world units.")]
    [SerializeField] private float lineWidth = 0.1f;
    [Tooltip("Extra radius added to the base collider radius for all rings. " +
             "Makes the visual wider than the trigger so player feels inside it.")]
    [SerializeField] private float radiusOffset = 2f;
    [Tooltip("How much each successive ring expands beyond the one below it. " +
             "0.1 = each ring is 10% wider than the previous.")]
    [SerializeField] private float radiusSpreadPerRing = 0.1f;

    [Header("Appearance")]
    [Tooltip("HDRP/Unlit material with Surface Type set to Transparent." +
             "Set color to desired ring color.")]
    [SerializeField] private Material ringMaterialTemplate;
    [Tooltip("Base emission intensity.")]
    [SerializeField] private float emissionIntensity = 1.3f;

    [Header("Pulse")]
    [Tooltip("Speed of the pulse animation.")]
    [SerializeField] private float pulseSpeed = 1.5f;
    [Tooltip("How much emission varies during pulse. 0 = no pulse.")]
    [SerializeField] private float pulseIntensity = 0.6f;

    private LineRenderer[] rings;
    private Color baseColor;
    private bool isVisible = false;

    private void Awake()
    {
        float colliderRadius = GetComponent<SphereCollider>().radius * transform.lossyScale.x;
        CreateRings(colliderRadius + radiusOffset);
        SetVisible(false);
    }
    private void Update()
    {
        if (!isVisible)
        {
            return;
        }

        float pulse = emissionIntensity + Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;

        ringMaterialTemplate.SetColor("_UnlitColor", baseColor * pulse);
    }
    /// <summary>
    /// Shows or hides the rings.
    /// </summary>
    public void SetVisible(bool visible)
    {
        isVisible = visible;

        if (rings == null)
        {
            return;
        }
        foreach (LineRenderer ring in rings)
        {
            if (ring != null)
            {
                ring.enabled = visible;
            }
        }
    }
    private void CreateRings(float baseRadius)
    {
        rings = new LineRenderer[ringCount];

        baseColor = ringMaterialTemplate.GetColor("_UnlitColor");

        for (int i = 0; i < ringCount; i++)
        {
            GameObject ringObj = new GameObject($"Ring_{i}");
            ringObj.transform.SetParent(transform);
            ringObj.transform.localPosition = new Vector3(0f, ringHeight + i * ringSpacing, 0f);

            LineRenderer lr = ringObj.AddComponent<LineRenderer>();
            lr.loop = true;
            lr.positionCount = segments;
            lr.material = ringMaterialTemplate;
            lr.useWorldSpace = false;
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;

            float ringRadius = baseRadius * (1f + i * radiusSpreadPerRing);

            for (int j = 0; j < segments; j++)
            {
                float angle = (float)j / segments * 2f * Mathf.PI;
                lr.SetPosition(j, new Vector3(Mathf.Cos(angle) * ringRadius,
                                              0f,
                                              Mathf.Sin(angle) * ringRadius));
            }

            rings[i] = lr;
        }
    }
    private void OnDestroy()
    {
        if (ringMaterialTemplate != null)
        {
            ringMaterialTemplate.SetColor("_UnlitColor", baseColor);
        }
    }
    private void OnDisable()
    {
        if (ringMaterialTemplate != null)
        {
            ringMaterialTemplate.SetColor("_UnlitColor", baseColor);
        }
    }
}