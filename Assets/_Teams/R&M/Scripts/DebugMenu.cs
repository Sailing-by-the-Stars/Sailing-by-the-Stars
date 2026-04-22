using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Linq;

public class TeleportMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject menuPanel;
    public Button buttonPrefab;
    public Transform container;

    private InputSystem_Actions input;
    private bool isOpen;

    private void Awake()
    {
        input = new InputSystem_Actions();
    }

    // private void OnEnable()
    // {
    //     input.UI.Enable();
    //     input.UI.OpenTeleportMenu.performed += OnToggleMenu;
    // }

    // private void OnDisable()
    // {
    //     input.UI.OpenTeleportMenu.performed -= OnToggleMenu;
    //     input.UI.Disable();
    // }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))  
        {
            ToggleMenu();
        }
    }

    private void Start()
    {
        menuPanel.SetActive(false);
        GenerateButtons();
    }

    private void OnToggleMenu(InputAction.CallbackContext ctx)
    {
        ToggleMenu();
    }

    public void ToggleMenu()
    {
        isOpen = !isOpen;
        menuPanel.SetActive(isOpen);

        TempStateMachine.Instance.gameState =
            isOpen ? GameState.Dialogue : GameState.Moving;
    }

    private void GenerateButtons()
    {
        var points = TeleportManager.Instance.GetTeleportPoints();

        var sortedPoints = points
            .OrderBy(p => p.Key)
            .ToList();

        foreach (var kvp in sortedPoints)
        {
            string pointName = kvp.Key;

            var btn = Instantiate(buttonPrefab, container);

            var label = btn.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            label.text = pointName;

            btn.onClick.AddListener(() =>
            {
                TeleportManager.Instance.TeleportTo(pointName);
                ToggleMenu();
            });
        }
    }
}