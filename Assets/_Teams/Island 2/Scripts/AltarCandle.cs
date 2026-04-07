using UnityEngine;

public class AltarCandle : MonoBehaviour
{
    [SerializeField] private ParticleSystem flameParticles;
    [SerializeField] private Light candleLight;

    private bool isLit;

    public bool IsLit => isLit;

    public void Light()
    {
        if (isLit) return;

        isLit = true;
        if (flameParticles != null)
        {
            flameParticles.Play();
        }
        if (candleLight != null)
        {
            candleLight.enabled = true;
        }
    }

    public void Extinguish()
    {
        if (!isLit) return;

        isLit = false;
        if (flameParticles != null)
        {
            flameParticles.Stop();
        }
        if (candleLight != null)
        {
            candleLight.enabled = false;
        }
    }
}
