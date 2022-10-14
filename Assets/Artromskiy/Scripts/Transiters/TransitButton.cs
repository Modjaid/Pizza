using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public abstract class TransitButton : TransitBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    [System.Serializable] public class ButtonClickEvent : UnityEvent { };
    [SerializeField()] public Color normalColor;
    [SerializeField()] public Color pressedColor;
    [SerializeField(), HideInInspector] private Image image;
    public ButtonClickEvent onClick = new ButtonClickEvent();


    public void OnPointerClick(PointerEventData data)
    {
        onClick?.Invoke();
        Transit();
    }

    public void OnPointerDown(PointerEventData data)
    {
        image.color = pressedColor;
    }

    public void OnPointerUp(PointerEventData data)
    {
        image.color = normalColor;
    }

    void Reset()
    {
        pressedColor = new Color(200, 200, 200, 255);
        normalColor = Color.white;
        ParentMenu = gameObject.transform.parent.gameObject;
    }

    void OnValidate()
    {
        image = GetComponent<Image>();
    }

}
