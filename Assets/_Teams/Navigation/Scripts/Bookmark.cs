using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Bookmark : MonoBehaviour, IPointerClickHandler
{
    public SectionName sectionName;

    public void OnPointerClick(PointerEventData eventData)
    {
        transform.parent.parent.parent.parent.GetComponent<Journal>().OpenSection(sectionName);
        GetComponent<Renderer>().material.color = Color.green;
    }

    public void Highlight()
    {

    }

    public void UnHighlight()
    {
        
    }
}
