using UnityEngine;

[ExecuteAlways]
public class AcceptanceStarfall : MonoBehaviour
{
    [Header("Starfall Settings")]
    [Tooltip("Size of the area particles fall from (X/Z). Match to your island size.")]
    public Vector2 emissionAreaSize = new Vector2(80f, 80f);

    [Tooltip("Height above this object's origin that particles spawn from.")]
    public float emissionHeight = 40f;

    [Range(1f, 60f)]
    [Tooltip("How many star motes spawn per second.")]
    public float emissionRate = 18f;

    [Tooltip("Colour of falling motes. Alpha is ignored — controlled by fade curve.")]
    public Color moteColour = new Color(1.0f, 0.97f, 0.75f);

    [Tooltip("Colour of the landing pulse.")]
    public Color landColour = Color.white;

    [Header("Trail Settings")]
    [Range(0f, 1f)]
    [Tooltip("Length of each star's trail relative to its lifetime (0 = no trail).")]
    public float trailLifetimeRatio = 0.25f;

    [Range(0f, 0.15f)]
    [Tooltip("Width of the trail at its widest point.")]
    public float trailWidth = 0.04f;

    private ParticleSystem _drift;
    private ParticleSystem _land;
    private Texture2D _starTexture;

    private void OnEnable()
    {
        _starTexture = GenerateStarTexture(64, 4, 0.42f, 0.18f);
        BuildSystems();
    }

    private void OnValidate()
    {
        _starTexture = GenerateStarTexture(64, 4, 0.42f, 0.18f);
        BuildSystems();
    }

    private void OnDisable()
    {
        DestroyChildren();
        if (_starTexture != null)
        {
#if UNITY_EDITOR
            DestroyImmediate(_starTexture);
#else
            Destroy(_starTexture);
#endif
            _starTexture = null;
        }
    }


    private void BuildSystems()
    {
        DestroyChildren();
        _land  = CreateLandSystem();
        _drift = CreateDriftSystem(_land);
    }

    private void DestroyChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (child.name == "StarDrift" || child.name == "StarLand")
            {
#if UNITY_EDITOR
                DestroyImmediate(child.gameObject);
#else
                Destroy(child.gameObject);
#endif
            }
        }
        _drift = null;
        _land  = null;
    }
    
    private ParticleSystem CreateDriftSystem(ParticleSystem landSystem)
    {
        var go = new GameObject("StarDrift");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, emissionHeight, 0f);
        go.hideFlags = HideFlags.NotEditable;

        var ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.loop            = true;
        main.playOnAwake     = true;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(8f, 15f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.08f, 0.2f);
        main.startRotation   = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
        main.gravityModifier = new ParticleSystem.MinMaxCurve(0.08f);
        main.maxParticles    = 300;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startColor      = new ParticleSystem.MinMaxGradient(
            moteColour,
            new Color(0.85f, 0.90f, 1.0f)
        );

        var emission = ps.emission;
        emission.enabled      = true;
        emission.rateOverTime = emissionRate;

        var shape = ps.shape;
        shape.enabled   = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale     = new Vector3(emissionAreaSize.x, 1f, emissionAreaSize.y);

        // fade in, hold, fade out
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] {
                new GradientAlphaKey(0f,   0f),
                new GradientAlphaKey(1f,   0.08f),
                new GradientAlphaKey(1f,   0.85f),
                new GradientAlphaKey(0f,   1f)
            }
        );
        col.color = new ParticleSystem.MinMaxGradient(g);

        // add a little twinkle: subtle size pulse over lifetime :)
        var sizeOverLife = ps.sizeOverLifetime;
        sizeOverLife.enabled = true;
        sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f,    0.6f),
            new Keyframe(0.15f, 1.0f),
            new Keyframe(0.3f,  0.7f),
            new Keyframe(0.5f,  1.0f),
            new Keyframe(0.7f,  0.75f),
            new Keyframe(0.85f, 1.0f),
            new Keyframe(1f,    0.0f)
        ));

        // gentle sideways sway
        var noise = ps.noise;
        noise.enabled     = true;
        noise.strength    = new ParticleSystem.MinMaxCurve(0.4f);
        noise.frequency   = 0.3f;
        noise.scrollSpeed = new ParticleSystem.MinMaxCurve(0.1f);
        noise.quality     = ParticleSystemNoiseQuality.Medium;

        // trail
        var trails = ps.trails;
        trails.enabled          = true;
        trails.mode             = ParticleSystemTrailMode.PerParticle;
        trails.ratio            = 1f;
        trails.lifetime         = new ParticleSystem.MinMaxCurve(trailLifetimeRatio);
        trails.minVertexDistance = 0.05f;
        trails.dieWithParticles = true;
        trails.textureMode      = ParticleSystemTrailTextureMode.Stretch;

        // trail narrows to a point at the tail
        AnimationCurve trailWidthCurve = new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(1f, 0f)
        );
        trails.widthOverTrail = new ParticleSystem.MinMaxCurve(trailWidth, trailWidthCurve);

        // trail fades out toward the tail
        var trailGradient = new Gradient();
        trailGradient.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        trails.colorOverTrail = new ParticleSystem.MinMaxGradient(trailGradient);

        // sub-emitter: spawn StarLand on death
        if (landSystem != null)
        {
            var sub = ps.subEmitters;
            sub.enabled = true;
            sub.AddSubEmitter(landSystem, ParticleSystemSubEmitterType.Death,
                ParticleSystemSubEmitterProperties.InheritNothing);
        }

        // renderer with star texture
        var r = go.GetComponent<ParticleSystemRenderer>();
        r.renderMode   = ParticleSystemRenderMode.Billboard;
        r.material     = GetAdditiveMaterial(_starTexture);
        r.trailMaterial = GetAdditiveMaterial(null); // plain additive for trail

        ps.Play();
        return ps;
    }


    private ParticleSystem CreateLandSystem()
    {
        var go = new GameObject("StarLand");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Vector3.zero;
        go.hideFlags = HideFlags.NotEditable;

        var ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.loop            = false;
        main.playOnAwake     = false;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.8f, 1.4f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(0f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
        main.startColor      = landColour;
        main.maxParticles    = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.enabled = false;

        var shape = ps.shape;
        shape.enabled   = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius    = 0.01f;

        var sizeOverLife = ps.sizeOverLifetime;
        sizeOverLife.enabled = true;
        sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f,   0.2f),
            new Keyframe(0.3f, 1.0f),
            new Keyframe(1f,   0.0f)
        ));

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = new ParticleSystem.MinMaxGradient(g);

        var r = go.GetComponent<ParticleSystemRenderer>();
        r.renderMode = ParticleSystemRenderMode.Billboard;
        r.material   = GetAdditiveMaterial(_starTexture);

        return ps;
    }


    private static Texture2D GenerateStarTexture(int size, int points, float outerRadius, float innerRadius)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode   = TextureWrapMode.Clamp;

        float half  = size * 0.5f;
        float angleStep = Mathf.PI / points; // angle between outer and inner tip

        var pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = (x - half) / half; // –1 to 1
                float py = (y - half) / half;

                float dist  = Mathf.Sqrt(px * px + py * py);
                float angle = Mathf.Atan2(py, px);

                // Distance to the nearest star-edge at this angle
                float sector     = Mathf.Floor((angle + Mathf.PI) / angleStep);
                float sectorAngle = (sector + 0.5f) * angleStep - Mathf.PI;
                float t          = Mathf.Abs(angle - sectorAngle) / (angleStep * 0.5f); 
                float starEdge   = Mathf.Lerp(outerRadius * 2f, innerRadius * 2f, t);

                // Soft glow core (always present, fades with distance)
                float glow = Mathf.Pow(Mathf.Max(0f, 1f - dist / (outerRadius * 2f)), 2.5f);

                // Star-point mask: 1 inside the star shape, blended at edge
                float starMask = Mathf.Clamp01((starEdge - dist) / (starEdge * 0.15f));

                float alpha = Mathf.Clamp01(glow + starMask * 0.85f);

                byte a = (byte)(alpha * 255f);
                pixels[y * size + x] = new Color32(255, 255, 255, a);
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();
        return tex;
    }
    

    private static Material GetAdditiveMaterial(Texture2D texture)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null) shader = Shader.Find("Particles/Additive");
        if (shader == null) shader = Shader.Find("Legacy Shaders/Particles/Additive");

        var mat = new Material(shader);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;

        if (texture != null)
        {
            mat.mainTexture = texture;
            if (mat.HasProperty("_BaseMap"))
                mat.SetTexture("_BaseMap", texture);
        }

        return mat;
    }
}
