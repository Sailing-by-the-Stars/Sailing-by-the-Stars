using UnityEngine;
using System.Collections;

public enum GameState { Moving, Dialogue }

public class TempStateMachine : MonoBehaviour
{
    private Movement movement;
    public static TempStateMachine Instance;
    public GameState gameState;

    private GameState previousState;

    public void Awake()
    {
        Instance = this;
        gameState = GameState.Moving;
        previousState = GameState.Moving;
        movement = FindFirstObjectByType<Movement>().GetComponent<Movement>();
    }

    public void Update()
    {
        if (gameState != previousState)
        {
            if (gameState == GameState.Dialogue)
                StartCoroutine(DisableMovementNextFrame());
            else
            {
                movement.enabled = true;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            previousState = gameState;
        }

        switch (gameState)
        {
            case GameState.Moving:
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                break;
            case GameState.Dialogue:
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                break;
        }
    }

    private IEnumerator DisableMovementNextFrame()
    {
        yield return null;
        movement.enabled = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}