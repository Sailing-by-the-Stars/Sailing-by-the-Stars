// Contributors: Arch Promchan
using UnityEngine;

public class InteractableLantern : MonoBehaviour
{
    [Header("Lantern Settings")]
    [SerializeField] private Light lanternLight;

    // keep track if player is close
    private bool isPlayerInRange = false;

    private void Start()
    {
        // make sure light is set and turn it off at start
        if (lanternLight != null)
        {
            lanternLight.enabled = false;
        }
    }

    private void Update()
    {
        // check if player is close and press f key
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F))
        {
            ToggleLight();
        }
    }

    // turn light on if off, and off if on
    private void ToggleLight()
    {
        if (lanternLight != null)
        {
            lanternLight.enabled = !lanternLight.enabled;
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