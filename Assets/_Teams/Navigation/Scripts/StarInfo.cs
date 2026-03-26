using System;
using Unity.Mathematics;
using UnityEngine;
using static StarDataLoader;

[ExecuteAlways]
public class StarInfo : MonoBehaviour
{
    [SerializeField]
    public StarDataLoader.Star starData;

    public AnimationCurve brightnessCurve;
    public float emissionMult;
    public float starSizeMax;



    private void Start()
    {
        /*Material material = GetComponent<MeshRenderer>().material;
        material.shader = Shader.Find("HDRP/Unlit");

        //material.SetFloat("_Size", Mathf.Lerp(starSizeMin, starSizeMax, star.size));
        float sizeM = Mathf.Lerp(0, 1, starData.size);
        sizeM = brightnessCurve.Evaluate(sizeM);

        float starSize = sizeM * starSizeMax;


        Vector3 size = new Vector3(starSize, starSize, starSize);

        transform.localScale = size;

        material.color = starData.colour * 2;
        material.EnableKeyword("_EMISSION");

        // base color (no intensity baked in)
        material.SetColor("_EmissiveColor", starData.colour);

        half intensityMul = (half)MathF.Pow(2.0f, emissionMult * starSize);
        material.color *= intensityMul;*/
    }
}
