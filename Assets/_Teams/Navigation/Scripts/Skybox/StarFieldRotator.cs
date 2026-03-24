using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarFieldRotator : MonoBehaviour
{
    [Header("Planet Settings")]
    public float planetRadius = 1000f;

    void Update()
    {
        float degreesPerUnit = 360f / (2f * Mathf.PI * planetRadius);

        float rotY = Camera.main.transform.position.x * degreesPerUnit;
        float rotX = -Camera.main.transform.position.z * degreesPerUnit;

        transform.rotation = Quaternion.Euler(rotX, rotY, 0f);
        transform.position = Camera.main.transform.position;
    }
}
