/*
 *  Created by vasilis Vlachos
 *  Contributed to by:
 */

using UnityEngine;
using UnityEngine.InputSystem;

public class BoatMovement : MonoBehaviour
{
    
    PlayerInput playerInput;

    InputAction moveAction;

    [SerializeField]
    private float speed = 5;

    [SerializeField]
    private float rotationSpeed = 20;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions.FindAction("Move");
    }

    // Update is called once per frame
    void Update()
    {
        MoveBoat();
    }

    void MoveBoat()
    {
        Vector2 direction = moveAction.ReadValue<Vector2>();

        //Move forward
        transform.position += transform.forward * direction.y * speed * Time.deltaTime;

        //Rotate
        transform.Rotate(Vector3.up * direction.x * rotationSpeed * Time.deltaTime);


        //Samples that i tried before adding the final
        //transform.Translate(Vector3.forward * direction.y * speed * Time.deltaTime);
        //transform.position += new Vector3(direction.x, 0, direction.y) * speed * Time.deltaTime;

    }
}
