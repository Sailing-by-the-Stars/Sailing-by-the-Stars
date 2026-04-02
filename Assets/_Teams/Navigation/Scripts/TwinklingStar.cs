using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static StarDataLoader;
using static UnityEngine.Rendering.DebugUI;

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


public class TwinklingStar : MonoBehaviour
{
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
    
    [SerializeField] AnimationCurve twinkleCurve;
    [SerializeField] AnimationCurve dimCurve;
    [SerializeField] float intensity = 1;
    [SerializeField] float twinkleTime = 1;
    [SerializeField] float targetAngle = 1;
    public static float currentTarget;


    private Color initialEmissionColor;
    private bool twinkle;
    public Vector3 initpos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentTarget = -1;
        initpos = transform.position;
        //Set the emission of the star before making changes

        if (GetComponent<Renderer>())
        {
            initialEmissionColor = GetComponent<Renderer>().material.GetColor("_EmissiveColor");
        }

        UpdateColor(intensity);
    }

    // Update is called once per frame
    void Update()
    {
        if (_states.TryGetValue(starState, out var curState))
        {
            curState.Tick(this);
        }
    }

    public void Hit(float hitAngle)
    {
        if(starState == StarState.dimmed)
        {
            return;
        }

        const float tolerance = 0.5f;
        if (Mathf.Abs(targetAngle - hitAngle) < tolerance)
        {
            currentTarget = -1;
            starState = StarState.dimmed;
        }
        else
        {
            currentTarget = targetAngle;
            starState = StarState.highlighted;
        }
    }

    private void OnValidate()
    {
        if (debugState != starState)
        {
            starState = debugState;
        }

    }


    public void UpdateColor(Material material, Color color, float intensity)
    {
        material.color = color;
        material.EnableKeyword("_EMISSION");

        half intensityMul = (half)MathF.Pow(2.0f, intensity);
        material.SetColor("_EmissiveColor", color * initialEmissionColor * intensityMul);
    }
    public void UpdateColor(Color color, float intensity)
    {
        UpdateColor(GetComponent<Renderer>().material, color, intensity);
    }
    public void UpdateColor(float intensity)
    {
        UpdateColor(Color.white, intensity);
    }
    public void UpdateColor()
    {
        UpdateColor(intensity);
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

                star.UpdateColor(Color.cyan, star.intensity * (curveOutput + 1));
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
        public void Enter(TwinklingStar star)
        {
            star.StartCoroutine(Brighten(star));
        }

        public void Tick(TwinklingStar star)
        {

        }

        public void Exit(TwinklingStar star)
        {
            star.StartCoroutine(Dim(star));
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

                star.UpdateColor(intensityMultiplier * star.intensity);

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

                star.UpdateColor(intensityMultiplier * star.intensity);

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
            animationLength = star.twinkleTime;
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
                float curveOutput = star.dimCurve.Evaluate(1- T);

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

}