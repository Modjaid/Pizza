using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarNearbyMarker : MonoBehaviour
{
    public Transform targetCar;

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if(targetCar != null)
        {
            Vector3 newPosition;
            Vector3 screenPos = cam.WorldToScreenPoint(targetCar.position);        

            newPosition = screenPos;
            newPosition.x = Mathf.Clamp(newPosition.x, 0, cam.pixelWidth);    
            newPosition.y = Mathf.Clamp(newPosition.y, 0, cam.pixelHeight);    

            transform.position = newPosition;
        }
    }

    public void SetTargetCar(Transform car)
    {
        targetCar = car;
    }
}