using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StarState
{
    none,
    minigame,
    highlighted,
    selected,
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
    };
    
    [SerializeField] AnimationCurve twinkleCurve;
    [SerializeField] AnimationCurve dimCurve;
    
    private Color initialEmission;
    private bool twinkle;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Set the emission of the star before making changes
        initialEmission = GetComponent<Renderer>().material.GetColor("_EmissiveColor");
    }

    // Update is called once per frame
    void Update()
    {
        if (_states.TryGetValue(starState, out var curState))
        {
            curState.Tick(this);
        }
    }

    private void OnValidate()
    {
        if (debugState != starState)
        {
            starState = debugState;
        }

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
            star.GetComponent<Renderer>().material.SetColor("_EmissiveColor", Color.red * 2000000);
        }

        public void Tick(TwinklingStar star)
        {

        }

        public void Exit(TwinklingStar star)
        {
            star.GetComponent<Renderer>().material.SetColor("_EmissiveColor", star.initialEmission);
        }
    }

    public class HighlightedState : IStarState
    {
        public void Enter(TwinklingStar star)
        {
            star.twinkle = true;
            star.StartCoroutine(Twinkle(star));
        }

        public void Tick(TwinklingStar star)
        {

        }

        public void Exit(TwinklingStar star)
        {
            star.twinkle = false;
            star.StopCoroutine(Twinkle(star));
            
            star.GetComponent<Renderer>().material.SetColor("_EmissiveColor", star.initialEmission);
        }
        
        private IEnumerator Twinkle(TwinklingStar star)
        {
            float timer = 0;
            int animationLength = star.twinkleCurve.length;
            
            //Loop animation if current state is selected
            while (star.twinkle)
            {
                timer += Time.deltaTime;

                if (timer > animationLength)
                {
                    timer -= animationLength;
                }
                
                //Find current position on the animation curve
                float T = timer / animationLength;
                float curveOutput = star.twinkleCurve.Evaluate(T);

                //Change emission intensity based on position on the animation curve
                star.GetComponent<Renderer>().material.SetColor("_EmissiveColor", star.initialEmission * curveOutput);
                
                yield return new WaitForEndOfFrame();
            }
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
                star.GetComponent<Renderer>().material.SetColor("_EmissiveColor", star.initialEmission * intensityMultiplier);

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
                star.GetComponent<Renderer>().material.SetColor("_EmissiveColor", star.initialEmission * intensityMultiplier); 

                yield return new WaitForEndOfFrame();
            }
        }
    }
}