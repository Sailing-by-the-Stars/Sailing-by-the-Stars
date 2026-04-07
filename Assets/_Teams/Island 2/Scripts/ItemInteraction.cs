using UnityEngine;

public class ItemInteraction : MonoBehaviour
{
    [SerializeField] private Altar altar;
    [SerializeField] private Camera mainCamera;

    private AltarItem selectedItem;
    private Pedestal selectedPedestal;

    private void Start()
    {
        if (altar == null)
        {
            altar = FindObjectOfType<Altar>();
            Debug.Log(altar != null ? "Found Altar" : "Altar not found!");
        }
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            Debug.Log(mainCamera != null ? "Found Main Camera" : "Main Camera not found!");
        }
    }

    private void Update()
    {
        HandleMouseInput();
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleLeftClick();
        }
    }

    private void HandleLeftClick()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log($"Hit object: {hit.collider.gameObject.name}");

            AltarItem item = hit.collider.GetComponent<AltarItem>();
            Pedestal pedestal = hit.collider.GetComponent<Pedestal>();

            if (item != null)
            {
                SelectItem(item);
            }
            else if (pedestal != null)
            {
                PlaceItemOnPedestal(pedestal);
            }
            else
            {
                Debug.Log("Hit object has no AltarItem or Pedestal component");
            }
        }
        else
        {
            Debug.Log("Raycast missed!");
        }
    }

    private void SelectItem(AltarItem item)
    {
        selectedItem = item;
        Debug.Log($"Selected item: {item.Type}");
    }

    private void PlaceItemOnPedestal(Pedestal pedestal)
    {
        if (selectedItem == null)
        {
            Debug.Log("No item selected!");
            return;
        }

        if (altar.PlaceItemOnPedestal(selectedItem, pedestal.PedestalIndex))
        {
            Debug.Log($"Placed {selectedItem.Type} on pedestal {pedestal.PedestalIndex}");
            selectedItem = null;
        }
        else
        {
            Debug.Log("Cannot place item here!");
        }
    }
}
