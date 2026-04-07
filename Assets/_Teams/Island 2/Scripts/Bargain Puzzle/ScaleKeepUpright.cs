using UnityEngine;

public class ScaleKeepUpright : MonoBehaviour
{
    private void LateUpdate()
    {
        transform.rotation = Quaternion.identity;
    }
}