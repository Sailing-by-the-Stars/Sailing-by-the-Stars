using UnityEngine;

public class FacePlayer : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        transform.LookAt(Camera.main.transform.position);
    }
}
