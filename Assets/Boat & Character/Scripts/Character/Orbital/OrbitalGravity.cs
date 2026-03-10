using FMOD;
using UnityEngine;

public class OrbitalGravity : MonoBehaviour
{
    [SerializeField] Transform gravityCenter;
    [SerializeField] Transform gravityHandler;
    [SerializeField] float gravityStrength = 0.1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        //Vector3.Angle(gravityCenter.position, other.transform.position);
        //print(Vector3.Angle(gravityCenter.position, other.transform.position));

        Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
        gravityHandler.position = other.transform.position;
        gravityHandler.LookAt(transform.position);

        Vector3 targetDir = (other.transform.position - transform.position).normalized;
        Vector3 bodyUp = other.transform.up;
        other.transform.rotation = Quaternion.FromToRotation(bodyUp, targetDir) * other.transform.rotation;



        ///figure out how to do this without affecting y rotation.
        //other.transform.up = -gravityHandler.forward;
        //other.transform.LookAt(gravityCenter, -other.transform.up);

        //other.transform.localRotation = Quaternion.Euler(-gravityHandler.forward.x, 0, -gravityHandler.forward.z);
        //other.transform.Rotate(-gravityHandler.forward.x,-gravityHandler.forward.y, -gravityHandler.forward.z);

        rb.AddForce(gravityHandler.forward * gravityStrength);
    }
}
