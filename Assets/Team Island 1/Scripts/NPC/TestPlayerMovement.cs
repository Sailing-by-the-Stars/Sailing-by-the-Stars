using UnityEngine;
using UnityEngine.InputSystem;

public class TestPlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    private void Update()
    {
        HandleMovement();
    }

    /// Moves the object using WASD or arrow keys.
    private void HandleMovement()
    {
        Vector2 input = Vector2.zero;

        if (Keyboard.current.wKey.isPressed) input.y += 1f;
        if (Keyboard.current.sKey.isPressed) input.y -= 1f;
        if (Keyboard.current.aKey.isPressed) input.x -= 1f;
        if (Keyboard.current.dKey.isPressed) input.x += 1f;

        Vector3 moveDirection = new Vector3(input.x, 0f, input.y);
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }
}