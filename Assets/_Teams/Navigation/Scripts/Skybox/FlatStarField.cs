using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static StarDataLoader;

public class FlatStarField : MonoBehaviour
{
    [SerializeField]
    StarPositionType starPositionType;

    [Space(15)]
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

    [SerializeField] List<ManualStar> manualStars = new();
    [SerializeField] float minManualSize = 8;
    [SerializeField] float manualSizeMult = 1.5f;

    [ContextMenu("fix my shit plz")]
    void fixiiiiiiit()
    {
        manualStars.Clear();
        int[] ints = new int[] {15, 21, 39, 74, 99, 168, 188, 219, 321, 334, 337, 343, 390, 403, 424, 472, 510, 539, 542, 544, 546, 553, 591, 596, 603, 617, 681, 804, 834, 850, 874, 879, 896, 897, 911, 921, 936, 947, 951, 963, 984, 1017, 1131, 1136, 1140, 1142, 1145, 1149, 1151, 1152, 1156, 1165, 1178, 1180, 1228, 1231, 1298, 1325, 1346, 1373, 1409, 1457, 1464, 1481, 1543, 1544, 1552, 1570, 1577, 1605, 1612, 1641, 1666, 1708, 1713, 1790, 1791, 1829, 1852, 1855, 1865, 1879, 1899, 1903, 1948, 1956, 2004, 2040, 2061, 2088, 2134, 2216, 2282, 2286, 2294, 2326, 2421, 2473, 2484, 2491, 2618, 2650, 2657, 2693, 2777, 2821, 2827, 2845, 2891, 2943, 2990, 3045, 3165, 3207, 3208, 3249, 3275, 3307, 3323, 3391, 3403, 3418, 3449, 3461, 3569, 3572, 3634, 3685, 3699, 3731, 3748, 3773, 3852, 3873, 3905, 3982, 4031, 4033, 4057, 4069, 4247, 4287, 4295, 4301, 4357, 4359, 4375, 4377, 4434, 4534, 4540, 4554, 4623, 4630, 4660, 4662, 4689, 4730, 4757, 4763, 4785, 4786, 4825, 4853, 4905, 4910, 4915, 4932, 4968, 5054, 5056, 5062, 5107, 5191, 5235, 5267, 5288, 5291, 5329, 5338, 5340, 5350, 5404, 5435, 5459, 5487, 5506, 5531, 5533, 5563, 5586, 5602, 5603, 5685, 5714, 5733, 5735, 5744, 5747, 5787, 5793, 5854, 5953, 5978, 5984, 6008, 6027, 6056, 6075, 6084, 6117, 6134, 6148, 6149, 6165, 6370, 6378, 6396, 6406, 6410, 6508, 6526, 6527, 6536, 6553, 6555, 6556, 6603, 6636, 6688, 6705, 6746, 6789, 6859, 6879, 6903, 6913, 7001, 7051, 7052, 7053, 7054, 7106, 7116, 7121, 7141, 7176, 7178, 7194, 7228, 7235, 7254, 7264, 7298, 7310, 7337, 7343, 7348, 7405, 7417, 7462, 7479, 7525, 7557, 7582, 7597, 7602, 7604, 7618, 7650, 7747, 7754, 7773, 7776, 7790, 7796, 7851, 7852, 7882, 7906, 7924, 7949, 7950, 8131, 8162, 8232, 8238, 8278, 8301, 8308, 8316, 8322, 8414, 8417, 8425, 8450, 8499, 8518, 8591, 8610, 8634, 8650, 8709, 8728, 8773, 8775, 8781, 8880, 8974};
        foreach (int i in ints)
        {
            ManualStar New = new();
            New.starID = i;
            manualStars.Add(New);
        }
    }


    [ContextMenu("Regenerate The Stars")]
    void RegenerateStars()
    {
        foreach(GameObject o in starObjects)
        {
            DestroyImmediate(o);
        }

        // Read in the star data.
        StarDataLoader sdl = new();

        if (!starPositionType)
        {
            starPositionType = new FlatStarPosition();
        }

        stars = sdl.LoadData(starPositionType);
        starObjects = new();
        foreach (StarDataLoader.Star star in stars)
        {
            // Create star game objects
            //>
            GameObject stargo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            stargo.transform.parent = transform;
            stargo.name = $"HR {star.catalog_number}";
            //<

            //deterimine position
            //>
            star.position.y = 0;
            stargo.transform.localPosition = star.position * starFieldScale;
            stargo.transform.Rotate(90, 0, 0);
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
            material.color = star.colour;
            material.EnableKeyword("_EMISSION");
            half intensityMul = (half)MathF.Pow(2.0f, emissionMult);
            material.SetColor("_EmissiveColor", material.color * intensityMul * starSize);
            //<


            starObjects.Add(stargo);
        }

        foreach (ManualStar manualStar in manualStars)
        {
            GameObject oldStar = starObjects[manualStar.starID - 1];


            GameObject newStar;
            if (manualStar.starPrefab)
            {
                newStar = Instantiate(manualStar.starPrefab);
            }
            else
            {
                newStar = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            }


            newStar.transform.parent = transform;
            newStar.name = oldStar.name;
            newStar.transform.position = oldStar.transform.position;

            Vector3 newsize = oldStar.transform.localScale;

            if(newsize.x < minManualSize)
            {
                newsize = new Vector3(minManualSize, minManualSize, minManualSize);
            }

            newStar.transform.localScale = newsize * manualSizeMult;

            Material oldMaterial = oldStar.GetComponent<MeshRenderer>().material;
            newStar.GetComponent<MeshRenderer>().material = oldMaterial;

            starObjects[manualStar.starID - 1] = newStar;

#if UNITY_EDITOR
            DestroyImmediate(oldStar);
#else
            Destroy(oldStar);
#endif
        }
    }

    private void OnValidate()
    {
        /*if (regenerateStars)
        {
            regenerateStars = false;
            foreach (GameObject o in starObjects)
            {
                DestroyImmediate(o);
            }
            RegenerateStars();
        }*/

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
