using UnityEngine;

[RequireComponent (typeof(Collider))]
public class JesusDebugCube : MonoBehaviour
{
    Collider jesusCollider;
    [SerializeField]
    bool turnedOn = false;

    private void Awake()
    {
        jesusCollider = GetComponent<Collider>();
        jesusCollider.enabled = turnedOn;
    }

    public void Toggle()
    {
        turnedOn = !turnedOn;

        jesusCollider.enabled = turnedOn;
    }
}
