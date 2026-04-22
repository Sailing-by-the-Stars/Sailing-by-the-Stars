using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

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
    [SerializeField] private float chromaBoost = 3;


    private List<StarDataLoader.Star> stars;
    public List<GameObject> starObjects;

    [SerializeField] List<Constelation> manualStars = new();
    [SerializeField] List<ManualTwinkler> twinklingStars = new();
    [SerializeField] float minManualSize = 8;
    [SerializeField] float manualSizeMult = 1.5f;
    [SerializeField] float flatManualSizeAdd = 20;

    [ContextMenu("fix my shit plz")]
    void fixiiiiiiit()
    {
        manualStars.Clear();
        
        
        foreach ((string, int[]) ints in constellations)
        {
            Constelation New = new();
            New.name = ints.Item1;
            foreach (int i in ints.Item2)
            {
                New.starIDs.Add(i);
            }

            manualStars.Add(New);
        }
    }

    private readonly List<(string, int[])> constellations = new() {

    ("Ursa Major",
     new int[] { 4295, 4301, 4554, 4660, 4905, 5054, 5191 }),

    ("Orion",
     new int[] { 1948, 1903, 1852, 2004, 1713, 2061, 1790, 1907, 2124,
                 2199, 2135, 2047, 2159, 1543, 1544, 1570, 1552, 1567 }),

    ("Ursa Minor",
     new int[] { 424, 6789, 6322, 5903, 6116, 5735, 5563 }),

    ("Cepheus",
     new int[] { 8162, 8238, 8974, 8465, 8694 }),

    ("Lazerta",
     new int[] { 8585, 8498, 8572, 8538, 8579, 8541 }),

    ("Cassiopeia",
     new int[] { 21, 168, 264, 403, 542 }),

    ("Pegasus",// im here
     new int[] { 8781, 8775, 39, 15, 8634, 8450, 8308, 337, 603, 165, 226, 269, 8430, 8454, 8650, 8665, 8667, 8684 }),

    ("Monoceros",
     new int[] { 2970, 3188, 2714, 2356, 2227, 2506, 2298, 2385, 2456, 2479 }),

    ("Gemini",
     new int[] { 2890, 2891, 2990, 2421, 2777, 2473, 2650, 2216,
                 2343, 2484, 2286, 2134, 2763, 2697, 2540, 2821, 2905, 2985 }),

    ("Cancer",
     new int[] { 3475, 3449, 3461, 3572, 3249 }),

    ("Leo",
     new int[] { 3982, 4534, 4057, 4357, 3873, 4031, 4359, 3975, 4399, 4386, 3905, 3773, 3731 }),

    ("Leo Minor",
     new int[] { 3800, 3974, 4100, 4247, 4090 }),

    ("Lynx",
     new int[] { 3705, 3690, 3612, 3579, 3275, 2818, 2560, 2238 }),

    ("Canis Major",
     new int[] { 2491, 2361, 2538, 2291, 2282, 2618, 2693, 2451 }),

    ("Canis Minor",
     new int[] { 2943, 2845 }),
    
    ("Taurus",
     new int[] { 1457, 1409, 1412, 1373, 1346, 1140, 1910, 1030, 1239, 1389, 1497, 1030, 1239, 1389, 1497}),

    //("Aries",
    // new int[] { 617, 548, 553 }),

    ("Andromeda",
     new int[] { 15, 337, 402, 603, 915, 681, 472 }),

    ("Cygnus",
     new int[] { 7924, 7796, 7528, 7417, 7531, 7141, 7106, 7615 }),

    ("Lyra",
     new int[] { 7001, 7178, 7298, 7144, 7102 }),

    ("Aquila",
     new int[] { 7525, 7557, 7602, 7377, 7235, 7236, 7264, 7405 }),

    ("Scorpius",
     new int[] { 6134, 6084, 5953, 5984, 6165, 6241, 6508, 6527, 6553, 6630 }),

    ("Sagittarius",
     new int[] { 7121, 7116, 7194, 6913, 7234, 7337, 7348, 6859 }),

    ("Bootes",
     new int[] { 5200, 5235, 5340, 5429, 5435, 5477, 5506, 5602, 5681 }),

    ("Virgo",
     new int[] { 5107, 4910, 4825, 4689, 4540, 5056, 5196, 5315, 5338 }),

    ("Libra",
     new int[] { 5531, 5685, 5787, 5820 }),

    ("Capricornus",
     new int[] { 7776, 7754, 7822, 8278, 8322, 7950, 8080 }),

    ("Aquarius",
     new int[] { 8414, 8232, 8518, 8610, 8709, 8834, 8932, 8968, 8728, 8264 }),
    /*
    ("Pisces",//really bad, redo pls
     new int[] { 9062, 8916, 8773, 434, 510, 618, 21, 38, 9089, 8852 }),
    */
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

        stars = sdl.LoadData(starPositionType);
        starObjects = new();
        foreach (StarDataLoader.Star star in stars)
        {
            // Create star game objects
            //>

            GameObject stargo = null;

            bool isManualStar = false;

            foreach (Constelation manualStar in manualStars)
            {
                foreach (int id in manualStar.starIDs)
                {
                    if (id != star.catalog_number)
                    {
                        continue;
                    }
                    

                    isManualStar = true;

                    if (manualStar.starPrefab)
                    {
                        stargo = Instantiate(manualStar.starPrefab);
                    }
                    else
                    {

                        stargo = GameObject.CreatePrimitive(PrimitiveType.Sphere);


                        TwinklingStar template = transform.parent.GetComponent<TwinklingStar>();


                        foreach (var obj in twinklingStars)
                        {
                            if (id == obj.starID)
                            {
                                TwinklingStar twinkler = stargo.AddComponent<TwinklingStar>();

                                twinkler.targetAngle = obj.targetAngle;

                                twinkler.twinkleCurve = template.twinkleCurve;
                                twinkler.dimCurve = template.dimCurve;
                                twinkler.intensity = template.intensity;
                                twinkler.twinkleTime = template.twinkleTime;
                                stargo.layer = LayerMask.NameToLayer("Constelation");
                                stargo.GetComponent<SphereCollider>().radius = 1.5f;
                            }
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

            stargo.transform.Rotate(90, 0, 0);
            //<

            //get necisairy values
            //>
            MeshRenderer meshRenderer = stargo.GetComponent<MeshRenderer>();

            meshRenderer.material = starMat;
            Material material = meshRenderer.sharedMaterial;

            float sizeM = Mathf.Lerp(0, 1, star.size);
            sizeM = brightnessCurve.Evaluate(sizeM);
            float starSize = sizeM * starSizeMax;
            //<

            //set the stars size
            //>
            Vector3 size = new Vector3(starSize, starSize, starSize);
            stargo.transform.localScale = size;

            if (isManualStar)
            {
                if (starSize < minManualSize)
                {
                    Vector3 newsize = new Vector3(minManualSize, minManualSize, minManualSize);
                    stargo.transform.localScale = newsize * manualSizeMult;
                }
                else
                {

                    stargo.transform.localScale *= manualSizeMult;

                    Vector3 addedScale = new Vector3(stargo.transform.localScale.x + flatManualSizeAdd, stargo.transform.localScale.y + flatManualSizeAdd, stargo.transform.localScale.z + flatManualSizeAdd);
                    stargo.transform.localScale = addedScale;
                }

            }
            //<

            //set the references on the star
            //>
            half intensityMul;
            intensityMul = (half)MathF.Pow(2.0f, emissionMult);
            StarInfo starInfo = stargo.AddComponent<StarInfo>();
            starInfo.matColor = star.colour;
            Color tempColor = star.colour * intensityMul * starSize;

            tempColor = BoostChroma(tempColor, chromaBoost);

            starInfo.emissionColor = tempColor;
            starInfo.emissionMult = intensityMul * starSize;

            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.Camera;

            starInfo.initpos = starInfo.transform.position;

            starInfo.Initialize();
            //<


            starObjects.Add(stargo);
        }

        GetComponentInParent<GlobeShape>().enabled = true;
    }

    Color BoostChroma(Color color, float amount)
    {
        float gray = color.grayscale;
        Color grayColor = new Color(gray, gray, gray);

        return grayColor + (color - grayColor) * amount;
    }

}
