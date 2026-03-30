using UnityEngine;

public class ConstellationCamera : MonoBehaviour
{
    public static ConstellationCamera Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        cam = GetComponent<Camera>();
    }


    public bool beingMoved = false;

    private Camera cam;

    public void OrientCam(Transform constellation)
    {
        beingMoved = true;
        cam.enabled = true;

        transform.position = Camera.main.transform.position;
        transform.rotation = Camera.main.transform.rotation;

        transform.LookAt(constellation);
    }



    private void Update()
    {
        if (!beingMoved)
        {
            cam.enabled = false;
        }
        else
        {
            beingMoved = false;
        }
    }
}
