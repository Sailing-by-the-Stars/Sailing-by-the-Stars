using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class FlatStarField : MonoBehaviour
{
    [SerializeField]
    Material starMat;
    [SerializeField]
    int starFieldScale = 400;
    [SerializeField] private AnimationCurve brightnessCurve;
    [Range(0, 100)]
    [SerializeField] private float starSizeMax = 5f;
    [SerializeField] private float emissionMult = 2;

    private List<StarDataLoader.Star> stars;
    public List<GameObject> starObjects;

    [SerializeField] List<manualStar> manualStars = new();


    [Space(15)]
    [SerializeField]
    float yCutoff = 0;

    [SerializeField]
    [Tooltip("regenerate the stars")]
    bool regenerateStars;

    void RegenerateStars()
    {
        foreach(Transform t in transform)
        {
            DestroyImmediate(t.gameObject);
        }

        // Read in the star data.
        StarDataLoader sdl = new();
        stars = sdl.LoadData();
        starObjects = new();
        foreach (StarDataLoader.Star star in stars)
        {
            if (star.position.y < yCutoff)
            {
                continue;
            }

            // Create star game objects.
            GameObject stargo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            stargo.transform.parent = transform;
            stargo.name = $"HR {star.catalog_number}";
            stargo.transform.position = star.position * starFieldScale + transform.localPosition;

            stargo.GetComponent<MeshRenderer>().material = starMat;

            Material material = stargo.GetComponent<MeshRenderer>().material;

            float sizeM = Mathf.Lerp(0, 1, star.size);
            sizeM = brightnessCurve.Evaluate(sizeM);

            float starSize = sizeM * starSizeMax;


            Vector3 size = new Vector3(starSize, starSize, starSize);

            stargo.transform.localScale = size;

            material.color = star.colour * 2;
            material.EnableKeyword("_EMISSION");

            material.SetColor("_EmissiveColor", star.colour);

            half intensityMul = (half)MathF.Pow(2.0f, emissionMult * starSize);
            material.color *= intensityMul;

            starObjects.Add(stargo);
        }

        foreach (manualStar manualStar in manualStars)
        {
            GameObject oldStar = starObjects[manualStar.starID - 1];

            GameObject newStar = Instantiate(manualStar.starPrefab);
            newStar.transform.parent = transform;
            newStar.name = oldStar.name;
            newStar.transform.position = oldStar.transform.position;
            //stargo.transform.localScale = Vector3.one * Mathf.Lerp(starSizeMin, starSizeMax, star.size);
            newStar.transform.LookAt(transform.position);
            newStar.transform.Rotate(0, 180, 0);

            starObjects[manualStar.starID - 1] = newStar;

            Destroy(oldStar);
        }
    }

    private void OnValidate()
    {
        if (regenerateStars)
        {
            RegenerateStars();
        }

        /*
        if (starObjects != null)
        {
            for (int i = 0; i < starObjects.Count; i++)
            {
                if (starObjects[i].GetComponent<MeshRenderer>() == null)
                {
                    continue;
                }

                // Update the size set in the shader.
                Material material = starObjects[i].GetComponent<MeshRenderer>().material;

                float sizeM = Mathf.Lerp(0, 1, stars[i].size);
                sizeM = brightnessCurve.Evaluate(sizeM);

                float starSize = sizeM * starSizeMax;


                Vector3 size = new Vector3(starSize, starSize, starSize);

                starObjects[i].transform.localScale = size;

                material.color = stars[i].colour * 2;
                material.EnableKeyword("_EMISSION");

                // base color (no intensity baked in)
                material.SetColor("_EmissiveColor", stars[i].colour);

                half intensityMul = (half)MathF.Pow(2.0f, emissionMult * starSize);
                material.color *= intensityMul;
            }
        }
        */
    }
}
