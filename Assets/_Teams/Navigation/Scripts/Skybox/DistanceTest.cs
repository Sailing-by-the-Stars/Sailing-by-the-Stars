using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class DistanceTest : MonoBehaviour
{
    [SerializeField]
    StarPositionType starPositionType;

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

    [SerializeField] List<Constelation> manualStars = new();
    [SerializeField] List<int> getDeclinationStars = new();


    [Space(15)]
    [SerializeField]
    float yCutoff = 0;


    [ContextMenu("Regenerate The Stars")]
    void RegenerateStars()
    {
        foreach (GameObject o in starObjects)
        {
            //Debug.LogError("destroying a star");
            DestroyImmediate(o);
        }

        // Read in the star data.
        StarDataLoader sdl = new();

        if (starPositionType == null)
        {
            starPositionType = new FlatStarPosition();
        }
        stars = sdl.LoadData(starPositionType);
        starObjects = new();
        foreach (StarDataLoader.Star star in stars)
        {

            //only certain stars
            //>
            bool foundStar = false;
            foreach(int starNumber in getDeclinationStars)
            {
                if (star.catalog_number == starNumber)
                {
                    Debug.Log($"the star HR {star.catalog_number} has the declination: {MathF.Round((float) star.declination, 5)}");
                    foundStar = true;
                }
            }

            if (foundStar == false)
            {
                continue;
            }
            //<

            // Create star game objects
            //>
            GameObject stargo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            stargo.transform.parent = transform;
            stargo.name = $"HR {star.catalog_number}";
            //<
            
            //deterimine position
            //>
            stargo.transform.position = star.position * starFieldScale + transform.localPosition;
            //<

            //get necisairy values
            //>
            stargo.AddComponent<StarInfo>();
            stargo.GetComponent<MeshRenderer>().material = starMat;
            Material material = stargo.GetComponent<MeshRenderer>().material;

            float sizeM = Mathf.Lerp(0, 1, star.size);
            sizeM = brightnessCurve.Evaluate(sizeM);
            float starSize = sizeM * starSizeMax;
            //<

            //set the stars size
            //>
            Vector3 size = new Vector3(starSize, starSize, starSize);
            stargo.transform.localScale = size;
            //<

            //set material properties
            //>
            material.color = star.colour * 2;
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissiveColor", star.colour);
            half intensityMul = (half)MathF.Pow(2.0f, emissionMult * starSize);
            material.color *= intensityMul;
            //<
            

            starObjects.Add(stargo);
        }

        foreach (Constelation manualStar in manualStars)
        {
            foreach (int id in manualStar.starIDs)
            {
                GameObject oldStar = starObjects[id - 1];


                GameObject newStar;
                if (manualStar.starPrefab)
                {
                    newStar = Instantiate(manualStar.starPrefab);
                }
                else
                {
                    newStar = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                    StarInfo oldInfo = oldStar.GetComponent<StarInfo>();

                    StarInfo starInfo = newStar.AddComponent<StarInfo>();
                    starInfo.matColor = oldInfo.matColor;
                    starInfo.emissionColor = oldInfo.emissionColor;
                    starInfo.emissionMult = oldInfo.emissionMult;
                }


                newStar.transform.parent = transform;
                newStar.name = oldStar.name;
                newStar.transform.position = oldStar.transform.position;

                Vector3 newsize = oldStar.transform.localScale;


                Material oldMaterial = oldStar.GetComponent<MeshRenderer>().sharedMaterial;
                newStar.GetComponent<MeshRenderer>().sharedMaterial = oldMaterial;

                starObjects[id - 1] = newStar;

#if UNITY_EDITOR
                DestroyImmediate(oldStar);
#else
                Destroy(oldStar);
#endif
            }
        }
    }

    private void OnValidate()
    {
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
