using UnityEngine;
using System.Collections;

/// <summary>
/// Code by Alonso
/// Modified by Christina
/// </summary>

public class BordersOfTheWorld : MonoBehaviour
{
    [SerializeField] private float duration = 1.5f;
    private bool rotating = false;

    [Tooltip("Script tries to find reference if unassigned")]
    [SerializeField] private PlayOutBounds boundsAudio;

    private void Start()
    {
        if (boundsAudio == null)
        {
            boundsAudio = FindFirstObjectByType<PlayOutBounds>();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (rotating) return;

        if (collision.gameObject.CompareTag("boat"))
        {
            StartCoroutine(Rote180(collision.transform));
        }
    }

    private IEnumerator Rote180(Transform objetivo)
    {
        rotating = true;

        if (boundsAudio != null)
        {
            boundsAudio.PlaySound();
        }

        Quaternion rotacionInicial = objetivo.rotation;
        Quaternion rotacionFinal = rotacionInicial * Quaternion.Euler(0f, 180f, 0f);

        float tiempo = 0f;
        while (tiempo < 1f)
        {
            tiempo += Time.deltaTime / duration;
            objetivo.rotation = Quaternion.Slerp(rotacionInicial, rotacionFinal, tiempo);
            yield return null;
        }

        if (boundsAudio != null)
        {
            boundsAudio.StopSound();
        }
        objetivo.rotation = rotacionFinal;
        rotating = false;
    }
}
