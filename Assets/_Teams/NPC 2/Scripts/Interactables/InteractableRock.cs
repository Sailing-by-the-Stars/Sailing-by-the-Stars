// Contributors: Arch Promchan
using UnityEngine;

public class InteractableRock : MonoBehaviour
{
    [Header("Rock Settings")]
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material activeMaterial;
    [SerializeField] private Material blueMaterial;
    
    [Header("Components")]
    [SerializeField] private MeshRenderer rockRenderer;

    // keep track if player is close
    private bool isPlayerInRange = false;

    private void Start()
    {
        // set default material at start
        if (rockRenderer != null)
        {
            rockRenderer.material = defaultMaterial;
        }
    }

    private void Update()
    {
        // check if player is close first
        if (isPlayerInRange)
        {
            // check for E key
            if (Input.GetKeyDown(KeyCode.E))
            {
                ChangeColor(activeMaterial);
            }
            
            // check for T key
            if (Input.GetKeyDown(KeyCode.T))
            {
                ChangeColor(blueMaterial);
            }

            // check for R key
            if (Input.GetKeyDown(KeyCode.R))
            {
                ChangeColor(defaultMaterial);
            }
        }
    }

    // change the material of the rock to the target material
    private void ChangeColor(Material newMaterial)
    {
        if (rockRenderer != null)
        {
            rockRenderer.material = newMaterial;
        }
    }

    // trigger when player enters the bubble
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    // trigger when player leaves the bubble
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }
}