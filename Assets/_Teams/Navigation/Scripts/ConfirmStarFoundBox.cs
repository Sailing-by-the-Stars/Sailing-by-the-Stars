using UnityEngine;


[RequireComponent (typeof(Collider))]
public class ConfirmStarFoundBox : MonoBehaviour
{
    public TwinklingStar starToConfirm;
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Movement>())
        {
            if(starToConfirm == null)
            {
                Debug.LogError("how the fuck did that happen?");
                return;
            }

            starToConfirm.Hit(float.MaxValue);

            Destroy(gameObject);
        }
    }
}
