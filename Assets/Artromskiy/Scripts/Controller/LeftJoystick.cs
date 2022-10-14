using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class LeftJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public Vector2Event onDrag;
    public RectTransform stickRing;
    public RectTransform stick;

    private Vector2 deltaNormal;

    public void OnDrag(PointerEventData data)
    {
        Vector3 deltaPixels = data.position - (Vector2)stickRing.position;
        var scale = stickRing.sizeDelta.x / 1920 * Screen.width;
        stick.position = stickRing.position + Vector3.ClampMagnitude(deltaPixels, (scale / 2));
        deltaNormal = (stick.position - stickRing.position) / (scale / 2);
    }

    private void Update()
    {
        var vec = deltaNormal;
#if UNITY_STANDALONE
	PCInput(ref vec);
#elif UNITY_EDITOR
        PCInput(ref vec);
#endif
        onDrag?.Invoke(vec);
    }

    public void OnPointerDown(PointerEventData data)
    {
        stickRing.position = data.position;
        stick.position = data.position;
    }

    public void OnPointerUp(PointerEventData data)
    {
        deltaNormal = Vector2.zero;
    }

    private void PCInput(ref Vector2 vec)
    {
        if (deltaNormal == Vector2.zero)
        {
            vec.x = Input.GetAxisRaw("Horizontal");
            vec.y = Input.GetAxisRaw("Vertical");
            vec.Normalize();
        }
    }
}

[System.Serializable]
public class Vector2Event: UnityEvent<Vector2> {}
