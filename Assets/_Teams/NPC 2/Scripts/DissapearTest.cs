using UnityEngine;

public class DissapearTest : MonoBehaviour
{
    void OnBecameInvisible()
    {
        Debug.Log("Testtest");
        gameObject.isVisible = false;
    }

    void OnBecameVisible()
    {
        Debug.Log("Blubblub");
        Destroy(gameObject);
    }
}
