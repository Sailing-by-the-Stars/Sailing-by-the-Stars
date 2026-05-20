// Created by Jantina
using UnityEngine;

public enum BoatPartType { Anchor, Rudder, Sail }

public class BoatHighlighter : MonoBehaviour
{
    [SerializeField] public BoatPartType partType;

    [Header("Highlight Settings")]
    [SerializeField] private Color highlightColor = new Color(1f, 0.6f, 0f);
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float minIntensity = 0f;
    [SerializeField] private float maxIntensity = 4f;

    private Renderer[] _renderers;
    private Material[][] _originalMaterials;
    private Material[][] _highlightMaterials;
    private bool _active = false;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>(true);
        _originalMaterials  = new Material[_renderers.Length][];
        _highlightMaterials = new Material[_renderers.Length][];

        for (int i = 0; i < _renderers.Length; i++)
        {
            _originalMaterials[i] = _renderers[i].sharedMaterials;

            var cloned = new Material[_renderers[i].sharedMaterials.Length];
            for (int j = 0; j < cloned.Length; j++)
            {
                cloned[j] = new Material(_renderers[i].sharedMaterials[j]);
                cloned[j].EnableKeyword("_EMISSION");
                cloned[j].SetColor("_EmissiveColor", Color.black);
                cloned[j].globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            _highlightMaterials[i] = cloned;
        }
    }

    private void Update()
    {
        if (!_active) return;

        float pulse     = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, pulse);
        Color emissive  = highlightColor * Mathf.Pow(2f, intensity);

        for (int i = 0; i < _renderers.Length; i++)
            for (int j = 0; j < _highlightMaterials[i].Length; j++)
                _highlightMaterials[i][j].SetColor("_EmissiveColor", emissive);
    }

    public void Highlight()
    {
        _active = true;
        for (int i = 0; i < _renderers.Length; i++)
            _renderers[i].materials = _highlightMaterials[i];
    }

    public void Unhighlight()
    {
        _active = false;
        for (int i = 0; i < _renderers.Length; i++)
            _renderers[i].materials = _originalMaterials[i];
    }

    private void OnDestroy()
    {
        if (_highlightMaterials == null) return;
        foreach (var mats in _highlightMaterials)
            foreach (var m in mats)
                if (m != null) Destroy(m);
    }
}