using UnityEngine;

public class SpawnButton : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject spawnPrefab;
    [SerializeField] private string objectInteractMessage;
    
    public string InteractMessage => objectInteractMessage;
    
    private void Spawn()
    {
        var spawnedObject = Instantiate(spawnPrefab, transform.position + Vector3.up, Quaternion.identity);

        var randomSize = Random.Range(0.1f, 1f);
        spawnedObject.transform.localScale = Vector3.one * randomSize;
        
        var randomColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        spawnedObject.GetComponent<MeshRenderer>().material.color = randomColor;
    }
    
    public void Interact(InteractionController interactionController)
    {
        Spawn();
    }
}