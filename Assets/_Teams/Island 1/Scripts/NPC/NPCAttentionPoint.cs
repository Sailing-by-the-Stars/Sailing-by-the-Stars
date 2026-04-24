using UnityEngine;
using System.Collections;

public class NPCAttentionPoint : MonoBehaviour
{
    [Header("Shout Settings")] [SerializeField]
    private string[] shoutLines =
    {
        "Hey! Over here!",
        "Important info here! Get it while it's hot!",
        "You should probably come talk to me, you know."
    };

    [SerializeField] private float shoutInterval = 4f;
    
    [Header("References")]
    [SerializeField] private NPCTextBubble textBubble;

    private bool playerIsClose = false;

    private void Start()
    {
        StartCoroutine(Shoutloop());
    }

    private IEnumerator Shoutloop()
    {
        while (!playerIsClose)
        {
            yield return new WaitForSeconds(shoutInterval);

            if (!playerIsClose)
            {
                string chosenLine = shoutLines[Random.Range(0, shoutLines.Length)];
                textBubble.ShowText(chosenLine);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsClose = true;
            textBubble.ShowText("Hi there!");
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, GetComponent<SphereCollider>().radius);
    }
}
