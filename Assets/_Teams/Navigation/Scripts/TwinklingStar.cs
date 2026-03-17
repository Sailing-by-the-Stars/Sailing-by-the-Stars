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

    [SerializeField] Vector2 luminosityRange;
    [SerializeField] Vector2 positionRange;

    [SerializeField] bool twinkle = false;
    [SerializeField] bool dim = false;

    private Coroutine twinkleRoutine;
    private Coroutine dimRoutine;
    private Coroutine brightenRoutine;

    [SerializeField] Light light;

    [SerializeField] float twinkleTime = 2f;
    [SerializeField] float dimTime = 2f;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        light = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_states.TryGetValue(starState, out var curState))
        {
            curState.Tick(this);
        }

        //Start the twinkle animation if twinkling is enabled
        if (twinkle && twinkleRoutine == null)
        {
            twinkleRoutine = StartCoroutine(Twinkle());
        }
    }

    private IEnumerator Twinkle()
    {
        float timer = 0;

        while (twinkle)
        {
            timer += Time.deltaTime;

            if (timer > twinkleTime)
            {
                timer -= twinkleTime;
            }

            //Find current position on the animation curve
            float T = timer / twinkleTime;
        
            float curveOutput = twinkleCurve.Evaluate(T);
            
            //Change the light intensity based on the position on the animation curve
            light.intensity = Mathf.Lerp(luminosityRange.x, luminosityRange.y, T);

            transform.position =  new Vector3(transform.position.x, Mathf.Lerp(positionRange.x, positionRange.y, curveOutput), transform.position.z);

            yield return new WaitForEndOfFrame();
        }

        //Reset the luminosity to the value set in the inspector
        StopCoroutine(twinkleRoutine);
        twinkleRoutine = null;
        light.intensity = luminosityRange.x;
    }

    private IEnumerator Brighten()
    {
        float timer = 0;

        //Play the animation as long as the value of dimTime
        while (timer < dimTime)
        {
            timer += Time.deltaTime;

            //Find position on the animation curve
            float T = timer / dimTime;
            float curveOutput = dimCurve.Evaluate(T);

            //Change the luminosity based on position on the animation curve (changes position for now as a test)
            transform.position = new Vector3(transform.position.x, Mathf.Lerp(positionRange.x, positionRange.y, curveOutput), transform.position.z);
            
            yield return new WaitForEndOfFrame();

        }

        StopCoroutine(brightenRoutine);
        brightenRoutine = null;
    }

    private IEnumerator Dim()
    {
        float timer = dimTime;

        while (timer > 0)
        {
            timer -= Time.deltaTime;

            //Find position on the animation curve
            float T = timer / dimTime;
            float curveOutput = dimCurve.Evaluate(T);

            //Change the luminosity based on position on the animation curve (changes position for now as a test)
            transform.position = new Vector3(transform.position.x, Mathf.Lerp(positionRange.x, positionRange.y, curveOutput), transform.position.z);

            yield return new WaitForEndOfFrame();
        }

        StopCoroutine(dimRoutine);
        dimRoutine = null;
    }

    private void OnValidate()
    {
        if (debugState != starState)
        {
            starState = debugState;
        }

        //If the value of the dim boolean changes, start the dim or brighten animation
        if (!dim && brightenRoutine == null)
        {
            brightenRoutine = StartCoroutine(Brighten());
        }

        if (dim && dimRoutine == null)
        {
            dimRoutine = StartCoroutine(Dim());
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
            Debug.LogWarning("entering state none");

            
        }

        public void Tick(TwinklingStar star)
        {
            Debug.Log("updating state none");
        }

        public void Exit(TwinklingStar star)
        {
            Debug.LogError("exiting state none");
        }
    }

    public class MinigameState : IStarState
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

    public class HighlightedState : IStarState
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

    public class SelectedState : IStarState
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
}