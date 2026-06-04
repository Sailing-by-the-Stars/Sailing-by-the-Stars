using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public enum StarState
{
    none,
    minigame,
    highlighted,
    selected,
    dimmed,
}

public interface IStarState
{
    void Enter(TwinklingStar star);
    void Tick(TwinklingStar star);
    void Exit(TwinklingStar star);
}


public class EntryCheck
{
    public int entryNumber = -1;
    public int targetAngle = 0;
    public bool beenChecked = false;
}


public class TwinklingStar : MonoBehaviour
{
    StarTutorialHelper currentHelper;

    [SerializeField] StarState debugState;

    StarState prevStarState;
    StarState StarState;
    public StarState starState
    {
        get { return StarState; }
        set
        {
            if (StarState == value) return;
            StarState = value;
            UpdateStar(StarState, prevStarState);
            Debug.Log($"starState changed to: {StarState}");
            prevStarState = StarState;
        }
    }

    private static readonly Dictionary<StarState, IStarState> _states = new()
    {
        { StarState.none, new NoneState() },
        { StarState.minigame, new MinigameState() },
        { StarState.highlighted, new HighlightedState() },
        { StarState.selected, new SelectedState() },
        { StarState.dimmed, new DimmedState() },
    };

    public AnimationCurve twinkleCurve;
    public AnimationCurve selectedCurve;
    public AnimationCurve dimCurve;
    public float intensity = 1;
    public float twinkleTime = 1;
    public float dimTime = 2;
    public float selectedTime = 5;
    public float selectedIntensity = 1;
    public float targetAngle = 1;
    public bool tutorialStar = false;
    public static float currentTarget;

    public static event Action OnStarFound;

    public List<EntryCheck> entryNumbers = new();

    public static List<TwinklingStar> tutorialStars;


    public Color initialColor;
    private bool twinkle;
    public Vector3 initpos;

    public int frameCount = 12;
    public float animationFPS = 12f;


    private void OnEnable()
    {
        NavigationSection.openedNavPage += OpenJournalPage;
    }

    private void OnDisable()
    {
        NavigationSection.openedNavPage -= OpenJournalPage;
    }


    void OpenJournalPage(List<int> pageNrs)
    {
        foreach (EntryCheck entryCheck in entryNumbers)
        {
            if (entryCheck.beenChecked)
            {
                continue;
            }

            if (pageNrs.Contains(entryCheck.entryNumber))
            {
                entryCheck.beenChecked = true;
                targetAngle = entryCheck.targetAngle;
                if (starState != StarState.highlighted && starState != StarState.dimmed)
                {
                    starState = StarState.selected;
                }
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentTarget = -1;
        initpos = transform.position;

        if (tutorialStar)
        {
            tutorialStars.Add(this);
        }
    }

    private void OnDestroy()
    {
        if (tutorialStar)
        {
            if (tutorialStars.Contains(this))
            {
                tutorialStars.Remove(this);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_states.TryGetValue(starState, out var curState))
        {
            curState.Tick(this);
        }
    }

    public virtual void Hit(float hitAngle)
    {
        if(starState == StarState.selected)
        {
            ExitStep();
        }


        if(starState == StarState.dimmed)
        {
            return;
        }

        if (hitAngle < 5)
        {
            return;
        }

        //Debug.Log($"star hit at angle: {hitAngle}");

        if ((hitAngle) >= targetAngle && (starState == StarState.selected || starState == StarState.highlighted))
        {
            currentTarget = -1;
            starState = StarState.dimmed;

            OnStarFound?.Invoke();
        }
        else
        {
            if (starState == StarState.selected)
            {
                currentTarget = targetAngle;
                starState = StarState.highlighted;
            }
        }
    }

    private void OnValidate()
    {
        if (debugState != starState)
        {
            starState = debugState;
        }

    }



    private MaterialPropertyBlock _mpb;
    public void UpdateColor(Material material, Color color, float intensity, float frame = 0)
    {
        if (_mpb == null)
            _mpb = new MaterialPropertyBlock();

        GetComponent<Renderer>().GetPropertyBlock(_mpb);

        if (material.shader.name == "Shader Graphs/Stars")
        {
            half intensityMul = (half)MathF.Pow(2.0f, intensity);
            
            
            _mpb.SetColor(Shader.PropertyToID("_Color"), color);
            _mpb.SetColor(Shader.PropertyToID("_EmissiveColor"), color * intensityMul);
        }else if (material.shader.name == "Shader Graphs/animated stars")
        {
            float intensityMul = Mathf.Pow(2.0f, intensity);

            _mpb.SetColor(Shader.PropertyToID("_Color"), color);
            _mpb.SetColor(Shader.PropertyToID("_EmissiveColor"), color * intensityMul);

            _mpb.SetFloat(Shader.PropertyToID("_FrameIndex"), frame);
        }
        else
        {
            Debug.LogError($"wrong shader: '{material.shader.name}'");

        }

        GetComponent<Renderer>().SetPropertyBlock(_mpb);
    }
    public void UpdateColor(Color color, float intensity, float frame = 0)
    {
        UpdateColor(GetComponent<Renderer>().material, color, intensity, frame);
    }
    public void UpdateColor(float intensity, float frame = 0)
    {
        UpdateColor(initialColor, intensity, frame);
    }
    public void UpdateColor(float frame = 0)
    {
        UpdateColor(intensity, frame);
    }


    //state machine things:
    public void UpdateStar(StarState state, StarState prevState)
    {
        if (_states.TryGetValue(prevState, out var oldState))
        {
            oldState.Exit(this);
        }
        else
        {
            if (oldState != null)
            {
                Debug.LogError($"State {oldState} not registered");
            }
        }

        if (_states.TryGetValue(state, out var newState))
        {
            newState.Enter(this);
        }
        else
        {
            Debug.LogError($"State {newState.ToString()} not registered");
        }
    }

    public class NoneState : IStarState
    {
        public void Enter(TwinklingStar star)
        {
            
        }

        public void Tick(TwinklingStar star)
        {
            
        }

        public void Exit(TwinklingStar star)
        {
            
        }
    }

    public class MinigameState : IStarState
    {
        public void Enter(TwinklingStar star)
        {
            star.UpdateColor(Color.cyan, star.intensity);
        }

        public void Tick(TwinklingStar star)
        {

        }

        public void Exit(TwinklingStar star)
        {
            star.UpdateColor();
        }
    }

    public class HighlightedState : IStarState
    {
        float timer = 0;
        float animationLength = 1;

        public void Enter(TwinklingStar star)
        {
            timer = 0;
            animationLength = star.twinkleTime;
            star.twinkle = true;
            //star.StartCoroutine(Twinkle(star));
        }

        public void Tick(TwinklingStar star)
        {
            //Loop animation if current state is selected
            if (star.twinkle)
            {
                timer += Time.deltaTime;

                if (timer > animationLength)
                {
                    timer -= animationLength;
                }

                //Find current position on the animation curve
                float T = timer / animationLength;
                float curveOutput = star.twinkleCurve.Evaluate(T);

                star.UpdateColor(star.initialColor, star.intensity * (curveOutput));
            }
        }

        public void Exit(TwinklingStar star)
        {
            star.twinkle = false;
            //star.StopCoroutine(Twinkle(star));
            star.UpdateColor();
        }
    }

    public class SelectedState : IStarState
    {
        float timer = 1;
        float animationLength = 1;

        public void Enter(TwinklingStar star)
        {
            timer = 1f;
            animationLength = star.selectedTime;
            star.twinkle = true;
        }

        public void Tick(TwinklingStar star)
        {
            if (!star.twinkle)
                return;

            timer += Time.deltaTime;

            if (timer > animationLength)
                timer -= animationLength;

            float curveOutput = 0;

            int frame = 0;

            if (timer >= 0 && timer < 1f)
            {
                curveOutput = star.selectedCurve.Evaluate(timer);
                frame = Mathf.FloorToInt((timer / animationLength) * star.frameCount);
                frame = Mathf.Clamp(frame, 0, star.frameCount - 1);
            }

            star.UpdateColor(star.intensity * (curveOutput * star.selectedIntensity), frame);
        }

        public void Exit(TwinklingStar star)
        {
            star.twinkle = false;
            star.UpdateColor(0);
        }

        private IEnumerator Brighten(TwinklingStar star)
        {
            float timer = 0;
            int animationLength = star.dimCurve.length;
            
            while (timer < animationLength)
            {
                timer += Time.deltaTime;

                //Find position on the animation curve
                float T = timer / animationLength;
                float curveOutput = star.dimCurve.Evaluate(T);

                //Change emission intensity based on position on the animation curve
                float animationMultiplier = curveOutput + 1;
                float intensityMultiplier = animationMultiplier * animationMultiplier;

                star.UpdateColor(star.initialColor, intensityMultiplier * star.intensity);

                yield return new WaitForEndOfFrame();
            }
        }

        private IEnumerator Dim(TwinklingStar star)
        {
            int animationLength = star.dimCurve.length;
            float timer = animationLength;
            
            while (timer > 0)
            {
                timer -= Time.deltaTime;

                //Find position on the animation curve
                float T = timer / animationLength;
                float curveOutput = star.dimCurve.Evaluate(T);
                
                //Change emission intensity based on position on the animation curve
                float animationMultiplier = curveOutput + 1;
                float intensityMultiplier = animationMultiplier * animationMultiplier;

                star.UpdateColor(star.initialColor, intensityMultiplier * star.intensity);

                yield return new WaitForEndOfFrame();
            }

            star.UpdateColor();
        }
    }
    public class DimmedState : IStarState
    {
        float timer = 0;
        float animationLength = 1;

        public void Enter(TwinklingStar star)
        {
            timer = 0;
            animationLength = star.dimTime;
            star.twinkle = true;
            //star.StartCoroutine(Twinkle(star));
        }

        public void Tick(TwinklingStar star)
        {
            //Loop animation if current state is selected
            if (star.twinkle && timer < animationLength)
            {
                timer += Time.deltaTime;

                if (timer > animationLength)
                {
                    timer = animationLength;
                }

                //Find current position on the animation curve
                float T = timer / animationLength;
                float curveOutput = star.dimCurve.Evaluate(T);

                star.UpdateColor(star.intensity * curveOutput);
            }
            else
            {
                star.UpdateColor(0);
            }
        }

        public void Exit(TwinklingStar star)
        {
            star.UpdateColor();
        }
    }
    // for testing world design event
    [ContextMenu("Simulate Star Found")]
    private void SimulateStarFound()
    {
        OnStarFound?.Invoke();
    }

    public void EnterStep(StarTutorialHelper helper)
    {
        currentHelper = helper;
    }

    public void ExitStep()
    {
        if (currentHelper)
        {
            currentHelper.ExitStep();
        }
        else
        {
            return;
        }

        currentHelper = null;

        if (tutorialStars.Contains(this))
        {
            tutorialStars.Remove(this);
        }
        else
        {
            Debug.LogWarning("the tutorial star isn't in the tutorialstars??");
        }
    }
}