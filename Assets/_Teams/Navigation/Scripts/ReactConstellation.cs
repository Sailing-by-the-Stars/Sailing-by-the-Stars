using UnityEngine;

public class ReactConstellation : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        GraphAnimator.endAnim += Activate;
    }

    void Activate(GraphAnimator animator)
    {
        gameObject.SetActive(true);

        foreach(Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
    }
}
