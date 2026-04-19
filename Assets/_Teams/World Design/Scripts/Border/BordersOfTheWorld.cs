using UnityEngine;
using System.Collections;

/// <summary>
/// Code by Alonso
/// </summary>

public class BordersOfTheWorld : MonoBehaviour
{
    [SerializeField] private float duration = 1.5f;
    private bool rotating = false;

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

        Quaternion rotacionInicial = objetivo.rotation;
        Quaternion rotacionFinal = rotacionInicial * Quaternion.Euler(0f, 180f, 0f);

        float tiempo = 0f;
        while (tiempo < 1f)
        {
            tiempo += Time.deltaTime / duration;
            objetivo.rotation = Quaternion.Slerp(rotacionInicial, rotacionFinal, tiempo);
            yield return null;
        }

        objetivo.rotation = rotacionFinal;
        rotating = false;
    }
}
