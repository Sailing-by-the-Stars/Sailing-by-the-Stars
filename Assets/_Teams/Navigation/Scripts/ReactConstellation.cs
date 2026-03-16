using UnityEngine;

public class ReactConstellation : MonoBehaviour
{
    void Activate(GraphAnimator animator)
    {
        gameObject.SetActive(true);

        foreach(Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    private void OnDisable()
    {
        GraphAnimator.endAnim -= Activate;
    }

    private void OnEnable()
    {
        GraphAnimator.endAnim += Activate;
    }
}
