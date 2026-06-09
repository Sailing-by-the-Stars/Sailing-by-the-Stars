using UnityEngine;

public enum GameState { Moving, Dialogue, Sailing, Journal, Astrolabe, Anchor, Sail, Rudder, Paused}

public class TempStateMachine : MonoBehaviour
{
    public static TempStateMachine Instance;

    public PlayerControls PlayerControls { get; private set; }
    public GameState gameState { get; private set; } = (GameState)(-1);

    private void Awake()
    {
        Instance = this;

        PlayerControls = new PlayerControls();

        //PlayerControls.Enable();

        PlayerControls.Debug.Enable();

        SetState(GameState.Moving);
    }

    private void OnDestroy()
    {
        PlayerControls.Disable();
    }

    public void SetState(GameState newState)
    {
        if (newState == gameState) return;

        gameState = newState;

        /*PlayerControls.Land.Disable();
        PlayerControls.Looking.Disable();
        PlayerControls.BoatSail.Disable();
        PlayerControls.BoatRudder.Disable();
        PlayerControls.BoatAnchor.Disable();
        PlayerControls.Dialogue.Disable();*/

        PlayerControls.Disable();
        PlayerControls.Debug.Enable();

        switch (newState)
        {
            case GameState.Moving:
                PlayerControls.Land.Enable();
                PlayerControls.Looking.Enable(); 
                PlayerControls.Interaction.Enable();
                PlayerControls.Journal.Enable(); 
                PlayerControls.Astrolabe.Open.Enable();
                SetCursorVisibility(false);
                break;

            case GameState.Dialogue:
                PlayerControls.Dialogue.Enable();
                SetCursorVisibility(true);
                break;

            case GameState.Sailing:
                PlayerControls.Land.Enable();
                PlayerControls.Looking.Enable();
                PlayerControls.Journal.Enable();
                PlayerControls.Astrolabe.Open.Enable();
                SetCursorVisibility(false);
                break;

            case GameState.Journal:

                PlayerControls.Journal.Enable();
                SetCursorVisibility(true);
                break;

            case GameState.Astrolabe:
                PlayerControls.Land.Enable();
                PlayerControls.Looking.Enable();
                SetCursorVisibility(false);
                break;

            case GameState.Sail:
                PlayerControls.Looking.Enable();
                PlayerControls.BoatSail.Enable();
                SetCursorVisibility(false);
                break;
            case GameState.Anchor:
                PlayerControls.Looking.Enable();
                PlayerControls.BoatAnchor.Enable();
                SetCursorVisibility(false);
                break;
            case GameState.Rudder:
                PlayerControls.Looking.Enable();
                PlayerControls.BoatRudder.Enable();
                SetCursorVisibility(false);
                break;
            case GameState.Paused:
                SetCursorVisibility(true);
                break;
            
            default:
                Debug.LogError("The player state is set completely wrong.");
                break;
        }
    }

    void SetCursorVisibility(bool state)
    {
        if (state)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}