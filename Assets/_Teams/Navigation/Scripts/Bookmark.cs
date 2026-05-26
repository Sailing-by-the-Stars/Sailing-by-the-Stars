using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Bookmark : MonoBehaviour, IPointerClickHandler
{
    public SectionName sectionName;
    Vector3 startPos;
    bool selected = false;

    private void Start()
    {
        startPos = transform.localPosition;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        transform.GetComponentInParent<Journal>().OpenSection(sectionName);
    }

    public void Highlight()
    {
        if (!selected)
        {
            transform.Translate(0, 0.025f, 0);
            selected = true;
        }
    }

    public void UnHighlight()
    {
        transform.localPosition = startPos;
        selected = false;
    }
}
