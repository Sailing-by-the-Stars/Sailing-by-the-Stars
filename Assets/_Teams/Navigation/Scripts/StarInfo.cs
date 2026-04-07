using System;
using Unity.Mathematics;
using UnityEngine;


public class StarInfo : MonoBehaviour
{
    public float emissionMult;
    public Vector3 initpos;

    private void Start()
    {
        
        initpos = transform.position;

        /*
        if(GetComponent<MeshRenderer>() == null)
        {
            return;
        }
        Material material = GetComponent<MeshRenderer>().material;
        material.shader = Shader.Find("HDRP/Unlit");

        float starSize = transform.localScale.x;

        material.color = Color.white * 2;
        material.EnableKeyword("_EMISSION");

        // base color (no intensity baked in)
        material.SetColor("_EmissiveColor", Color.white);

        half intensityMul = (half)MathF.Pow(2.0f, emissionMult * starSize);
        material.color *= intensityMul;
        */
    }
}
