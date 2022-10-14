using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class OneKeyController : MonoBehaviour, IDragHandler
{

    public Vector2Event onDrag;
    public FloatEvent onDragX;

    public Vector2 deltaNormal;
    public Vector2 startPos;

    public void OnDrag(PointerEventData data)
    {
        deltaNormal.y += (data.delta.y) / (Screen.height / 2);
        deltaNormal.y = Mathf.Abs(deltaNormal.y) > 1 ? Mathf.Sign(deltaNormal.y) : deltaNormal.y;
        onDragX?.Invoke(data.delta.x / Screen.width * 250);
    }

    private void Update()
    {
        var vec = deltaNormal;
        vec.x = 0;
        Debug.Log(vec.y);
        vec.y = Mathf.Abs(vec.y) < 0.2f ? 0 : vec.y;
        Debug.Log(vec.y);
        onDrag?.Invoke(vec);
    }
}
