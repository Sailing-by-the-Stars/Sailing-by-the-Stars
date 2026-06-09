using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


[Serializable]
public class Constelation
{
    public string name = "";
    public List<int> starIDs = new();
    public GameObject starPrefab;
    public Color debugColor = Color.violet;
    public Color finalColor = Color.violet;
    public float sizeMult = 1;
}

[Serializable]
public class ManualTwinkler
{
    public int starID;
    public int targetAngle;
    public bool tutorialStar = false;
    public ConfirmStarFoundBox confirmBox;
}

public class FlatStarField : MonoBehaviour
{
    [Header("color things")]
    [SerializeField] int colorSeed = 0;
    [SerializeField] List<Color> availableColors = new();
    [SerializeField] private float emissionMult = 2;
    [SerializeField] private float chromaBoost = 3;
    [SerializeField] bool manualColor = true;

    [Header("size things")]

    [Header("general size")]
    [Range(0, 100)]
    [SerializeField] private float starSizeMax = 5f;

    [Header("manual stars")]
    [SerializeField] float minManualSize = 8;
    [SerializeField] float manualSizeMult = 1.5f;
    [SerializeField] float flatManualSizeAdd = 20;

    [Header("twinkler sizes")]
    [SerializeField] float twinklerSizeMult = 3f;
    [SerializeField] float twinklerEmissionMult = 0.8f;


    [Header("star field things")]
    [SerializeField]
    StarPositionType starPositionType;

    [Space(15)]
    [SerializeField]
    Material starMat;
    [SerializeField]
    Material twinklerMat;
    [SerializeField]
    int starFieldScale = 400;
    [SerializeField] private AnimationCurve brightnessCurve;


    private List<StarDataLoader.Star> stars;
    public List<GameObject> starObjects;

    [SerializeField] List<Constelation> manualStars = new();
    [SerializeField] List<ManualTwinkler> twinklingStars = new();
    [SerializeField] private float starColliderMult = 1.5f;

    [SerializeField] Mesh manualStarMesh;


    


    [SerializeField]
    bool debugColors = false;

    //[ContextMenu("fix my shit plz")]
    void fixiiiiiiit()
    {
        manualStars.Clear();

        
        //int I = 0;
        foreach ((string, int[]) ints in constellations)
        {
            Constelation New = new();

            New.name = ints.Item1;
            foreach (int i in ints.Item2)
            {
                New.starIDs.Add(i);
                //I++;
                //Debug.Log($"count: {I}");
            }

            manualStars.Add(New);
        }

        FixColors();
    }


    [ContextMenu("randomizeColors")]
    void FixColors()
    {
        if (availableColors.Count <= 0)
        {
            Debug.LogError("no available colors set!!!!");
            return;
        }

        UnityEngine.Random.InitState(colorSeed);

        foreach (var constelation in manualStars)
        {
            constelation.finalColor = availableColors[
                UnityEngine.Random.Range(0, availableColors.Count)
            ];
        }

        if(starObjects.Count > 0)
        {
            RegenerateStars();
        }
    }

    private readonly List<(string, int[])> constellations = new() {

    ("Ursa Major", //D
     new int[] { 4295, 4301, 4554, 4660, 4905, 5054, 5191, //normal
         3757, 3323, 3888, 3894, 3775, 3569, 3594, 4518, 4335, 4033, 4069 }), // extended

    ("Draco",
     new int[] { 4434, 4787, 5291, 5744, 5986, 6132, 6396, 6927, 7352, 7582, 7310, 6688, 6555, 6536, 6705 }),

    ("Eridanus",
     new int[] { 1481, 1679, 1666, 1560, 1520, 1463, 1136, 1084, 984, 874, 818, 919, 1003, 1088, 1173, 1464 }),

    ("Orion", //D
     new int[] { 1948, 1903, 1852, 2004, 1713, 2061, 1790, 1907, 2124,
                 2199, 2135, 2047, 2159, 1543, 1544, 1570, 1552, 1567, 1601 }),

    ("Ursa Minor", //D
     new int[] { 424, 6789, 6322, 5903, 6116, 5735, 5563 }),

    ("Cepheus", //D
     new int[] { 8162, 8238, 8974, 8465, 8694 }),

    ("Camelopardus",
     new int[] { 1686, 1148, 1542, 1035 }),

    ("Lacerta", //D
     new int[] { 8585, 8498, 8572, 8538, 8579, 8541 }),

    ("Cassiopeia", //D
     new int[] { 21, 168, 264, 403, 542 }),

    ("Pegasus", //D
     new int[] { 8781, 8775, 39, 15, 8634, 8450, 8308, 337, 603, 165, 226, 269, 8430, 8454, 8650, 8665, 8667, 8315 }),

    ("Monoceros", //D
     new int[] { 2970, 3188, 2714, 2356, 2227, 2506, 2298, 2174 }),

    ("Gemini", //D
     new int[] { 2890, 2891, 2990, 2421, 2777, 2473, 2650, 2216,
                 2343, 2484, 2286, 2134, 2763, 2697, 2540, 2821, 2905, 2985 }),

    ("Cancer", //D
     new int[] { 3475, 3449, 3461, 3572, 3249, 3262 }),

    ("Leo", //D
     new int[] { 3982, 4534, 4057, 4357, 3873, 4031, 4359, 3975, 3905 }),
    
    ("Leo Minor", //D
     new int[] { 3800, 3974, 4100, 4247 }),

    ("Lynx", //D
     new int[] { 3705, 3690, 3612, 3579, 3275, 2818, 2560, 2238 }),

    /*("Canis Major",
     new int[] { 2491, 2361, 2538, 2291, 2282, 2618, 2693, 2451 }),
    */
    ("Canes Venatici", //d
     new int[] { 4785, 4915 }),

    ("Taurus", //D
     new int[] { 1457, 1409, 1412, 1373, 1346, 1140, 1910, 1030, 1239, 1389, 1497, 1030, 1239, 1389, 1497}),

    ("Aires", //D
     new int[] { 617, 545, 553, 838 }),

    ("Triangulum", //D
     new int[] { 664, 622, 544 }),

    ("Perseus",
     new int[] { 834, 915, 1017, 1122, 936, 1220, 921, 840, 1228, 1203, 1131 }),

    ("Cygnus", //D
     new int[] { 7924, 7796, 7528, 7417, 7615, 7420, 7949, 8115, 8309, 7328 }),

    ("Lyra", //D
     new int[] { 7001, 7178, 7106, 7139, 7056 }),

    ("Vulpecula", //D
     new int[] { 7653, 7406 }),

    ("Delphinus", //d
     new int[] { 7948, 7906, 7928, 7882, 7852 }),

    ("Equuleus", //d
     new int[] { 8097, 8123, 8178, 8131 }),

    ("Sagitta", //D
     new int[] { 7679, 7635, 7536, 7479, 7488 }),

    ("Aquila", //D
     new int[] { 7525, 7557, 7602, 7377, 7235, 7236, 7176, 7570, 7710 }),

    ("Serpens", //D
     new int[] { 5879, 5933, 5867, 5789, 5854, 5892, 5888, 5881, 6056 }),

    ("Ophiuchus", //D
     new int[] { 6556, 6299, 6603, 6075, 6175, 6378, 6519 }),

    ("Serpens Cauda", //d
     new int[] { 7141, 6918, 6869, 6755, 6869, 7040, 7141 }),

    ("Scutum", //d
     new int[] { 7063, 7066, 6973, 6930, 7119 }),
    
    /*("Scorpius",
     new int[] { 6134, 6084, 5953, 5984, 6165, 6241, 6508, 6527, 6553, 6630 }),
    */
    ("Sagittarius",
     new int[] { 7121, 7116, 7194, 6913, 7234, 7337, 7348, 6859 }),

    ("Bootes", //D
     new int[] { 5200, 5235, 5340, 5429, 5435, 5477, 5506, 5602, 5681 }),

    ("Corona Borealis",
     new int[] { 5778, 5747, 5793, 5849, 5889, 5947, 5971 }),

    ("Virgo",
     new int[] { 5107, 4910, 4825, 4689, 4540, 5056, 5196, 5315, 5338 }),

    ("Libra",
     new int[] { 5531, 5685, 5787, 5820 }),

    /*
    ("Capricornus",
     new int[] { 7776, 7754, 7822, 8278, 8322, 7950, 8080 }),
    */

    ("Aquarius",
     new int[] { 8414, 8232, 8518, 8610, 8709, 8834, 8932, 8968, 8728, 8264 }),
    
    ("Picies", //D
     new int[] { 291, 383, 360, 437, 510, 595, 549, 489, 434, 294, 224, 80, 9072, 8969, 8916, 8878, 8852, 8911, 8984, 9004 }),

    ("Cetus",
     new int[] { 813, 896, 718, 649, 754, 911, 804, 779, 681, 781, 708, 539, 402, 334, 188, 74, 811, 740, 710, 509 }),

    ("Hercules",
     new int[] { 6148, 6212, 6092, 6168, 6241, 6095, 6220, 6324, 6410, 6418, 6436, 6485, 6526, 6588, 6695, 6703, 6779 }),

    ("Crux",
     new int[] { 4853, 4730, 4700, 4656 }),

    ("Auriga",
     new int[] { 1708, 2088, 2095, 1791, 1577, 1612 }),


};


    private void Awake()
    {
        if (starObjects == null)
        {
            starObjects = new();
        }

        RegenerateStars();
    }

    [ContextMenu("Regenerate The Stars")]
    void RegenerateStars()
    {
        foreach(GameObject o in starObjects)
        {
            #if UNITY_EDITOR
                DestroyImmediate(o);
#else
                Destroy(o);
#endif
        }

        // Read in the star data.
        StarDataLoader sdl = new();

        if (starPositionType == null)
        {
            starPositionType = new FlatStarPosition();
        }

        TwinklingStar.tutorialStars = new();

        stars = sdl.LoadData(starPositionType);
        starObjects = new();
        foreach (StarDataLoader.Star star in stars)
        {
            // Create star game objects
            //>

            GameObject stargo = null;
            Constelation starConstelation = null;

            bool isManualStar = false;
            TwinklingStar twinkler = null;
            bool fullyManual = false;

            TwinklingStar template = transform.parent.GetComponent<TwinklingStar>();

            foreach (Constelation manualStar in manualStars)
            {
                foreach (int id in manualStar.starIDs)
                {
                    if (id != star.catalog_number)
                    {
                        continue;
                    }
                    

                    isManualStar = true;

                    starConstelation = manualStar;

                    if (manualStar.starPrefab)
                    {
                        stargo = Instantiate(manualStar.starPrefab);
                        fullyManual = true;
                    }
                    else
                    {
                        bool twinkling = false;

                        int i = 0;
                        foreach (var obj in twinklingStars)
                        {
                            i++;

                            if (id == obj.starID)
                            {
                                if(stargo == null)
                                {
                                    stargo = GameObject.CreatePrimitive(PrimitiveType.Quad);
#if UNITY_EDITOR
                                    DestroyImmediate(stargo.GetComponent<MeshCollider>());
#else
                Destroy(stargo.GetComponent<MeshCollider>());
#endif
                                    stargo.AddComponent<SphereCollider>();
                                }

                                if (twinkler == null)
                                {
                                    twinkler = stargo.AddComponent<TwinklingStar>();
                                }


                                twinkler.volume = template.volume;
                                twinkler.twinkleCurve = template.twinkleCurve;
                                twinkler.selectedCurve = template.selectedCurve;
                                twinkler.dimCurve = template.dimCurve;
                                twinkler.intensity = template.intensity;
                                twinkler.twinkleTime = template.twinkleTime;

                                twinkler.tutorialStar = obj.tutorialStar;


                                if (!twinkler.tutorialStar)
                                {
                                    twinkler.selectedTime = template.selectedTime;
                                }

                                twinkler.fmodEvent = template.fmodEvent;

                                EntryCheck entryCheck = new();
                                entryCheck.entryNumber = i;
                                entryCheck.targetAngle = obj.targetAngle;
                                
                                twinkler.entryNumbers.Add(entryCheck);

                                if (obj.confirmBox)
                                {
                                    obj.confirmBox.starToConfirm = twinkler;

                                }
                                else if(!obj.tutorialStar)
                                {
                                    Debug.LogWarning($"the twinkling star {obj.starID} doesn't have a confirmBox assigned to it");
                                }


                                    stargo.layer = LayerMask.NameToLayer("Constelation");
                                stargo.GetComponent<SphereCollider>().radius = starColliderMult;
                                twinkling = true;
                                continue;
                            }
                        }

                        if (stargo == null)
                        {
                            stargo = GameObject.CreatePrimitive(PrimitiveType.Sphere);

#if !UNITY_EDITOR
                        stargo.GetComponent<MeshFilter>().mesh = manualStarMesh;
#endif
                        }

                        Collider collider = stargo.GetComponent<Collider>();
                        if (collider != null && twinkling == false)
                        {
#if UNITY_EDITOR
                            DestroyImmediate(collider);
#else
                Destroy(collider);
#endif
                        }
                    }

                    stargo.name = $"HR {star.catalog_number} {manualStar.name}";

                    if (isManualStar)
                    {
                        break;
                    }
                }

                if (isManualStar)
                {
                    break;
                }
                //Debug.Log("this doesn't");
            }


            if (isManualStar == false)
            {
                stargo = GameObject.CreatePrimitive(PrimitiveType.Quad);
                
                stargo.name = $"HR {star.catalog_number}";

                Collider collider = stargo.GetComponent<Collider>();
                if (collider != null)
                {
#if UNITY_EDITOR
                    DestroyImmediate(collider);
#else
                Destroy(collider);
#endif
                }
            }
            
            stargo.transform.parent = transform;

            
            //<

            //deterimine position
            //>
            //star.position.y = 0;
            stargo.transform.localPosition = star.position * starFieldScale;
            stargo.transform.rotation = transform.rotation;


            if (!fullyManual)
            {
                stargo.transform.Rotate(90, 0, 0);
            }
            //<

            //get necisairy values
            //>
            MeshRenderer meshRenderer = stargo.GetComponent<MeshRenderer>();

            if (twinkler != null)
            {
                meshRenderer.material = twinklerMat;
            }
            else
            {
                meshRenderer.material = starMat;
            }
            Material material = meshRenderer.sharedMaterial;

            float sizeM = Mathf.Lerp(0, 1, star.size);
            sizeM = brightnessCurve.Evaluate(sizeM);
            float starSize = sizeM * starSizeMax;
            //<

            //set the stars size
            //>
            Vector3 size = new Vector3(starSize, starSize, starSize);
            

            if (isManualStar)
            {
                starSize *= manualSizeMult;
                starSize *= starConstelation.sizeMult;
                if (twinkler != null)
                {
                    starSize *= twinklerSizeMult;
                }

                starSize += flatManualSizeAdd;

                if (!fullyManual)
                {
                    if (starSize < minManualSize)
                    {
                        Vector3 newsize = new Vector3(minManualSize, minManualSize, minManualSize);
                        stargo.transform.localScale = newsize;
                    }
                    else
                    {
                        Vector3 newsize = new Vector3(starSize, starSize, starSize);
                        stargo.transform.localScale = newsize;
                    }
                }


                if (twinkler != null)
                {
                    starSize /= twinklerSizeMult;
                    starSize *= twinklerEmissionMult;
                }
            }
            else
            {
                stargo.transform.localScale = size;
            }

                //<

                //set the references on the star
                //>
                half intensityMul;
            intensityMul = (half)MathF.Pow(2.0f, emissionMult);

            StarInfo starInfo = GetComponent<StarInfo>();
            if (starInfo == null)
            {
                starInfo = stargo.AddComponent<StarInfo>();
            }


#if !UNITY_EDITOR
            debugColors = false;
#endif
            Color tempColor;

            if (debugColors)
            {
                if (isManualStar && starConstelation != null)
                {
                    starInfo.matColor = starConstelation.debugColor;
                    tempColor = starConstelation.debugColor * intensityMul * starSize;
                    starInfo.emissionColor = tempColor;
                    starInfo.emissionMult = intensityMul * starSize;

                }
                else
                {
                    starInfo.matColor = star.colour;
                    tempColor = star.colour * intensityMul * starSize;

                    tempColor = BoostChroma(tempColor, chromaBoost);

                    starInfo.emissionColor = tempColor;
                    starInfo.emissionMult = intensityMul * starSize;
                }
            }
            else
            {
                if (manualColor && starConstelation != null && starConstelation.finalColor != null)
                {
                    starInfo.matColor = starConstelation.finalColor;
                    tempColor = starConstelation.finalColor * intensityMul * starSize;
                    starInfo.emissionColor = tempColor;
                    starInfo.emissionMult = intensityMul * starSize;
                }
                else
                {
                    if (manualColor && starConstelation != null)
                    {
                        Debug.LogError($"had to fall back to automatic colors for the constilation {starConstelation.name}!!");
                    }

                        starInfo.matColor = star.colour;
                    tempColor = star.colour * intensityMul * starSize;

                    tempColor = BoostChroma(tempColor, chromaBoost);

                    starInfo.emissionColor = tempColor;
                    starInfo.emissionMult = intensityMul * starSize;
                }
            }


            if(twinkler != null)
            {
                twinkler.initialColor = tempColor;
            }

            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.Camera;

            starInfo.initpos = starInfo.transform.position;

            starInfo.Initialize();
            //<


            starObjects.Add(stargo);


            foreach (StarInfo childStar in starInfo.GetComponentsInChildren<StarInfo>())
            {
                float tempStarSize = starSize;

                if(childStar == starInfo)
                {
                    continue;
                }

                twinkler = childStar.GetComponent<TwinklingStar>();
                float mult = 1;
                if (twinkler != null)
                {
                    int i = 0;
                    foreach (var obj in twinklingStars)
                    {
                        i++;
                        if (star.catalog_number == obj.starID)
                        {
                            twinkler.volume = template.volume;
                            twinkler.twinkleCurve = template.twinkleCurve;
                            twinkler.selectedCurve = template.selectedCurve;
                            twinkler.dimCurve = template.dimCurve;
                            //twinkler.intensity = template.intensity;
                            twinkler.initialColor = tempColor;
                            twinkler.twinkleTime = template.twinkleTime;

                            twinkler.tutorialStar = obj.tutorialStar;
                            if (!twinkler.tutorialStar)
                            {
                                twinkler.selectedTime = template.selectedTime;
                            }

                            EntryCheck entryCheck = new();
                            entryCheck.entryNumber = i;
                            entryCheck.targetAngle = obj.targetAngle;

                            twinkler.entryNumbers.Add(entryCheck);

                            if (obj.confirmBox)
                            {
                                obj.confirmBox.starToConfirm = twinkler;

                            }
                            else if (!obj.tutorialStar)
                            {
                                Debug.LogWarning($"the twinkling star {obj.starID} doesn't have a confirmBox assigned to it");
                            }


                            childStar.gameObject.layer = LayerMask.NameToLayer("Constelation");
                            childStar.GetComponent<SphereCollider>().radius = starColliderMult;
                        }
                    }


                    mult = (1 / twinklerSizeMult) * emissionMult/2;

                    tempStarSize *= twinklerSizeMult;
                    //tempStarSize *= twinklerEmissionMult;
                }

                if (tempStarSize < minManualSize)
                {
                    Vector3 newsize = new Vector3(minManualSize, minManualSize, minManualSize);
                    childStar.transform.localScale = newsize;
                }
                else
                {
                    Vector3 newsize = new Vector3(tempStarSize, tempStarSize, tempStarSize);
                    childStar.transform.localScale = newsize;
                }


                if (childStar.GetComponent<TwinklingStar>())
                {
                    childStar.Initialize(starInfo, mult);

                }
                else
                {
                    childStar.Initialize(starInfo);

                }
            }
        }

        GetComponentInParent<GlobeShape>().enabled = true;
    }

    [ContextMenu("Clear The Stars")]
    void ClearStars()
    {
        foreach (GameObject o in starObjects)
        {
#if UNITY_EDITOR
            DestroyImmediate(o);
#else
                Destroy(o);
#endif
        }
        starObjects = new();
    }

    Color BoostChroma(Color color, float amount)
    {
        float gray = color.grayscale;
        Color grayColor = new Color(gray, gray, gray);

        return grayColor + (color - grayColor) * amount;
    }

}
