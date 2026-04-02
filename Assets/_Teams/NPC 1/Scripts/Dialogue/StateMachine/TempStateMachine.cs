using UnityEngine;
public enum GameState
{
    Moving,
    Dialogue
}
public class TempStateMachine : MonoBehaviour
{
    private Movement movement;

    public static TempStateMachine Instance;
    public GameState gameState;

    public void Awake()
    {
        Instance = this;
        gameState = GameState.Moving;
        movement = FindFirstObjectByType<Movement>().GetComponent<Movement>();
    }

    public void FixedUpdate()
    {
        switch (gameState)
        {
            case GameState.Moving:
                movement.enabled = true;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                break;
            case GameState.Dialogue:
                movement.enabled = false;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                break;
            default:
                break;
        }
    }
}