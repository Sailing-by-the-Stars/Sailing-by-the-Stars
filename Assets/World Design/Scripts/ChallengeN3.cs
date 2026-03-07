using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEditor;


/// Code by Alonso

public class ChallengeN3 : MonoBehaviour
{
    [SerializeField] private GameObject ocean;
    void Start()
    {
       
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Boat")
        {
            Debug.Log("Hola");
            StartChallenge();
        }
    }


    private void StartChallenge()
    {
        WaterSurface ws = ocean.GetComponent<WaterSurface>();

        SerializedObject so = new SerializedObject(ws);
        SerializedProperty prop = so.FindProperty("largeBand0Multiplier");

        if (prop != null)
        {
            prop.floatValue = 1f;   
            so.ApplyModifiedProperties();

            ws.enabled = false;
            ws.enabled = true; 
        }
    }

    private void PrintWaterProperties(WaterSurface ws)
    {
        SerializedObject so = new SerializedObject(ws);
        SerializedProperty prop = so.GetIterator();

        while (prop.NextVisible(true))
        {
            Debug.Log(prop.propertyPath);
        }
    }
}
