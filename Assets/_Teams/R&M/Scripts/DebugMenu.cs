using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections;

public class DebugMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject menuPanel;
    public Button buttonPrefab;
    public Transform container;
    [Header("Debug Spawnables")]
    public Journal journalPrefab;
    public Astrolabe astrolabePrefab;
    private bool isOpen;
    private Button journalBtn;
    private Button astrolabeBtn;

    private PlayerControls controls;
    private PlayerControls.DebugActions debugControls;

    private void Awake()
    {
        controls = TempStateMachine.Instance.PlayerControls;
        debugControls = controls.Debug;
    }
    private void OnEnable()
    {
        debugControls.ToggleDebugMenu.performed += OnToggleMenu;
    }

    private void OnDisable()
    {
        debugControls.ToggleDebugMenu.performed -= OnToggleMenu;
    }
    private IEnumerator Start()
    {
        menuPanel.SetActive(false);
        yield return null;
        GenerateButtons();
    }

    private void OnToggleMenu(InputAction.CallbackContext ctx)
    {
        if (DialogueSystem.Instance.isDialogueActive)
            return;
        ToggleMenu();
    }

    public void ToggleMenu()
    {
        isOpen = !isOpen;
        menuPanel.SetActive(isOpen);

        if (isOpen)
            TeleportManager.Instance.GetTeleportPoints();

        TempStateMachine.Instance.SetState(isOpen ? GameState.Dialogue : GameState.Moving);
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

        var speedBtn = Instantiate(buttonPrefab, container);
        speedBtn.GetComponentInChildren<TextMeshProUGUI>().text = "SPEEEEEDYYY SPRINT";
        speedBtn.onClick.AddListener(() =>
        {
            var movement = FindFirstObjectByType<Movement>();

            if (movement != null)
            {
                movement.SetSprintMultiplier(10f);
            }
        });

        var resetTutorialBtn = Instantiate(buttonPrefab, container);
        resetTutorialBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Reset Tutorials";
        resetTutorialBtn.onClick.AddListener(() =>
        {
            TutorialResetter.Instance.ResetAll();
        });

        journalBtn = Instantiate(buttonPrefab, container);
        journalBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Give Journal";
        journalBtn.onClick.AddListener(() =>
        {
            StartCoroutine(GiveItemRoutine(journalPrefab));
            journalBtn.gameObject.SetActive(false);
        });

        astrolabeBtn = Instantiate(buttonPrefab, container);
        astrolabeBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Give Astrolabe";
        astrolabeBtn.onClick.AddListener(() =>
        {
            StartCoroutine(GiveItemRoutine(astrolabePrefab));
            astrolabeBtn.gameObject.SetActive(false);
        });
    }

    IEnumerator GiveItemRoutine(ToolPickup itemPrefab)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        var pickupController = player.GetComponent<PickupController>();

        var existingItems = FindObjectsOfType(itemPrefab.GetType());
        foreach (var item in existingItems)
        {
            Destroy(((Component)item).gameObject);
        }

        var itemInstance = Instantiate(itemPrefab);

        yield return null;

        itemInstance.Grab(pickupController);
    }
}