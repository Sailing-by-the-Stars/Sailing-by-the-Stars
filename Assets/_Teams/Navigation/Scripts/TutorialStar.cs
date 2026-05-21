using UnityEngine;

public class TutorialStar : StarInfo
{
    [SerializeField]
    bool meshrendereOn = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Initialize();

        
    }

    // Update is called once per frame
    void Update()
    {
        if (!meshrendereOn)
        {
            GetComponent<MeshRenderer>().enabled = false;
        }
    }
}
