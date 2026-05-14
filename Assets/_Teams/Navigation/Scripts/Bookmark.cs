using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Bookmark : MonoBehaviour, IPointerClickHandler
{
    [HideInInspector] public int pageNr;

    public void OnPointerClick(PointerEventData eventData)
    {
        transform.parent.parent.parent.parent.GetComponent<Journal>().OpenBookmark(pageNr);
        GetComponent<Renderer>().material.color = Color.green;
    }
}
