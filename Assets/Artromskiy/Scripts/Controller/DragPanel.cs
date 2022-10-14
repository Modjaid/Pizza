using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class DragPanel : MonoBehaviour, IDragHandler, IPointerUpHandler
{
    public FloatEvent onDragX;
    public FloatEvent onDragY;

    private Vector2 deltaNormal;

    public void OnDrag(PointerEventData data)
    {
        deltaNormal = data.delta;
        deltaNormal.x = deltaNormal.x / Screen.width * 250;
        deltaNormal.y = deltaNormal.y / Screen.height * 250;
        onDragX?.Invoke(deltaNormal.x);
        onDragY?.Invoke(deltaNormal.y);
    }

#if UNITY_STANDALONE
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Update()
    {
        deltaNormal.x = Input.GetAxis("Mouse X") * 3;
        deltaNormal.y = Input.GetAxis("Mouse Y") * 3;
        onDragX?.Invoke(deltaNormal.x);
        onDragY?.Invoke(deltaNormal.y);
    }
#elif UNITY_EDITOR
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void Update()
    {
        deltaNormal.x = Input.GetAxis("Mouse X") * 3;
        deltaNormal.y = Input.GetAxis("Mouse Y") * 3;
        onDragX?.Invoke(deltaNormal.x);
        onDragY?.Invoke(deltaNormal.y);
    }
#endif

    public void OnPointerUp(PointerEventData data)
    {
        deltaNormal = Vector2.zero;
        onDragX?.Invoke(deltaNormal.x);
        onDragY?.Invoke(deltaNormal.y);
    }

    private Vector3 lastMousePosition;
}

    [System.Serializable]
    public class FloatEvent: UnityEvent<float>{}
