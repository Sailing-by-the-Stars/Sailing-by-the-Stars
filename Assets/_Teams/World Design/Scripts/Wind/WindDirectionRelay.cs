using UnityEngine;

// This is an example class of the wind for testing wind direction. 
// Use it as example of in Wrather manager 
public class WindDirectionRelay : MonoBehaviour
{
    [SerializeField] private WindController windController;
    [SerializeField] private Vector3 testDirection = Vector3.forward;

    private void Awake()
    {
        windController.SetAutoReroll(false);
        windController.SetInfluencedWindDirection(testDirection);
        Debug.Log("WindDirectionRelay initialized with test direction: " + testDirection);
    }
}