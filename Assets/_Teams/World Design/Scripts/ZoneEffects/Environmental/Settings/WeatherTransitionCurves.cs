/*
 * Created by Christina Pence
 * Contributed to by:
 */
using UnityEngine;

/// <summary>
/// Defines how weather parameters blend from their current value toward a destination WeatherState over time
/// where Y = 0 represents starting value and 1 = the target value
/// and X is the normalized transition time (0 to 1 over the duration)
/// </summary>
[CreateAssetMenu(fileName = "NewWeatherTransitionCurves", menuName = "World Environment Effects/Weather Transition Curves")]
public class WeatherTransitionCurves : ScriptableObject
{
    public AnimationCurve windCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve rainCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve waveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve currentCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve thunderCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
}