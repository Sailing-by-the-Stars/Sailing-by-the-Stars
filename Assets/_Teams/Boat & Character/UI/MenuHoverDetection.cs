using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButtonHoverDetector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private MainMenuController _controller;
    private int _index;
    private bool _initialized;

    public void Init(MainMenuController controller, int index)
    {
        _controller = controller;
        _index = index;
        _initialized = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_initialized) return;
        _controller.OnButtonHoverEnter(_index);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_initialized) return;
        _controller.OnButtonHoverExit(_index);
    }
}