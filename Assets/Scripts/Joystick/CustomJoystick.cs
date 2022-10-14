using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class CustomJoystick : Joystick
{
    public float MoveThreshold { get { return moveThreshold; } set { moveThreshold = Mathf.Abs(value); } }
    public GameObject lockIcon;

    [SerializeField] private float moveThreshold = 1;

    protected override void Start()
    {
        MoveThreshold = moveThreshold;
        base.Start();
        background.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (sprinting)
        {
            background.gameObject.SetActive(true);
        }

        if (!sprinting && !dragging)
        {
            background.gameObject.SetActive(false);
            input = Vector2.zero;
        }
    }

    public override void LetGoInput()
    {
        background.gameObject.SetActive(false);

        base.LetGoInput();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
        background.gameObject.SetActive(true);

        base.OnPointerDown(eventData);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (!sprinting)
        {
            background.gameObject.SetActive(false); 

            base.OnPointerUp(eventData);
        }

        dragging = false;
    }

    public void OnSprintStarted()
    {
        sprinting = true;
        lockIcon.gameObject.SetActive(true);
    }

    public void OnSprintOver()
    {
        sprinting = false;
        lockIcon.gameObject.SetActive(false);

        if (!dragging)
        {
            background.gameObject.SetActive(false);
        }
        else if (dragging && input.magnitude == 0)
        {
            handle.anchoredPosition = Vector2.zero;
        }
    }

    protected override void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
    {
        if (sprinting)
        {
            base.HandleInput(magnitude * 1000, normalised, radius, cam);
        }
        else
        {
            base.HandleInput(magnitude, normalised, radius, cam);
        }
    }
}