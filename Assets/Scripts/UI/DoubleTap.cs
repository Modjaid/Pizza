using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DoubleTap : MonoBehaviour, IPointerClickHandler
{
    public UnityEvent onDoubleTap;
    public float doubleClickThreshold = 0.25f;

    private float lastTimeClicked;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Pointer click.");
        
        if(Time.time - doubleClickThreshold <= lastTimeClicked)
        {
            onDoubleTap?.Invoke();
            Debug.Log("Double tap.");
        }

        lastTimeClicked = Time.time;
    }
}