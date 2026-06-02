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
        thisTransform = transform;
        initpos = thisTransform.position;

        if (GetComponent<MeshRenderer>() == null)
        {
            return;
        }
        Initialize(GetComponent<MeshRenderer>());
    }

    public void Initialize(StarInfo baseStar)
    {
        emissionMult = baseStar.emissionMult;
        matColor = baseStar.matColor;
        emissionColor = baseStar.emissionColor;
        Initialize();
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
        else if (material.shader.name == "Shader Graphs/animated stars")
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

    private void OnDestroy()
    {
        FlatStarField flatStarField = thisTransform.GetComponentInParent<FlatStarField>();
        if (flatStarField != null)
        {
            flatStarField.starObjects.Remove(gameObject);
        }
        GlobeShape globeShape = thisTransform.GetComponentInParent<GlobeShape>();
        if (globeShape != null)
        {
            globeShape.relatedMiniStars.Remove(this);
            globeShape.InitializeMiniStars();
        }
    }
}
