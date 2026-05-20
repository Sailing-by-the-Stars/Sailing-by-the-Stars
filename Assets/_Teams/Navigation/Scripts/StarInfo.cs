using System;
using System.Drawing;
using Unity.Mathematics;
using UnityEngine;
using static StarDataLoader;


public class StarInfo : MonoBehaviour
{
    public Transform thisTransform;

    public float emissionMult;
    public UnityEngine.Color matColor;

    [ColorUsage(true, true)]
    public UnityEngine.Color emissionColor;
    public Vector3 initpos;

/*#if UNITY_EDITOR
    private void OnEnable()
    {
        Initialize();
    }
#endif*/

    public void Initialize()
    {
        initpos = transform.position;
        thisTransform = transform;

        if (GetComponent<MeshRenderer>() == null)
        {
            return;
        }
        Initialize(GetComponent<MeshRenderer>());
    }



    private static readonly int ColorID = Shader.PropertyToID("_Color");
    private static readonly int EmissiveColorID = Shader.PropertyToID("_EmissiveColor");
    private MaterialPropertyBlock _mpb;

    public void Initialize(MeshRenderer renderer)
    {
        //Debug.Log(Shader.Find("Shader Graphs/Stars"));
        initpos = transform.position;

        if (_mpb == null)
            _mpb = new MaterialPropertyBlock();

        renderer.GetPropertyBlock(_mpb);

        var material = renderer.sharedMaterial;

        if (material.shader.name == "Shader Graphs/Stars")
        {
            _mpb.SetColor(Shader.PropertyToID("_Color"), matColor);
            _mpb.SetColor(Shader.PropertyToID("_EmissiveColor"), emissionColor);
        }
        else
        {
            Debug.LogError($"wrong shader: '{material.shader.name}'");

        }

        renderer.SetPropertyBlock(_mpb);
    }
}
