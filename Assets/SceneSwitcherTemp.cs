using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcherTemp : MonoBehaviour
{
    private void Awake()
    {
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
