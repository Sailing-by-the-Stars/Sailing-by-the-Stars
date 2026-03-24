using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcherTemp : MonoBehaviour
{
    public static SceneSwitcherTemp Instance { get; private set; }

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);
    }


    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Equals))
        {
            SceneManager.LoadScene("Demo1");
        }
        if(Input.GetKeyDown(KeyCode.Minus))
        {
            SceneManager.LoadScene("DemoOrbit");

        }
    }
}
