using UnityEngine;

public class BoatSwitcherTemp : MonoBehaviour
{
    Camera cam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (Transform t in transform)
        {
            if (t.GetComponent<Camera>())
            {
                cam = t.GetComponent<Camera>();
            }
        }

        if (!cam)
        {
            Debug.LogError("no camera found!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            Camera.main.transform.parent.gameObject.SetActive(false);

            cam.gameObject.SetActive(true);
        }
    }
}
