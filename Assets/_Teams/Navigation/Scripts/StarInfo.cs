using System;
using Unity.Mathematics;
using UnityEngine;
using static StarDataLoader;


public class StarInfo : MonoBehaviour
{
    public float emissionMult;
    public Color matColor;

    [ColorUsage(true, true)]
    public Color emissionColor;
    public Vector3 initpos;

    private void Start()
    {
        
        initpos = transform.position;

        
        if(GetComponent<MeshRenderer>() == null)
        {
            return;
        }
        Material material = GetComponent<MeshRenderer>().material;


        if (material.shader == Shader.Find("Shader Graphs/Stars"))
        {
            material.SetColor("_Color", matColor);

            material.SetFloat("Brightness", emissionMult);

            material.SetColor("_EmissiveColor", emissionColor);
        }
        else
        {
            material.color = matColor;
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissiveColor", emissionColor);

        }
    }
}
